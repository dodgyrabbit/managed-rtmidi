using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Music.Midi;

namespace midi_filter
{
    public class Application
    {
        readonly IMidiAccess2 access;
        IMidiInput input;
        IMidiOutput output;
        readonly MidiPublisher midiPublisher;

        /// <summary>
        /// Creates a new Application instance.
        /// </summary>
        public Application(IMidiAccess2 access, MidiPublisher midiPublisher)
        {
            this.access = access;
            this.midiPublisher = midiPublisher;
        }

        /// <summary>
        /// Writes the MIDI input devices to the console.
        /// </summary>
        public void PrintMidiInputDevices()
        {
            Console.WriteLine("MIDI input devices:");
            foreach (var inputDevice in access.Inputs)
            {
                Console.WriteLine(inputDevice.Name);
            }
        }
        
        /// <summary>
        /// Tries to open the MIDI input device with the provided name, and creates an output port if successful.
        /// </summary>
        /// <param name="name">The MIDI input device name to search for. A partial, case insensitive match is made.</param>
        /// <returns>The MIDI input port details if successful, null otherwise.</returns>
        public async Task<IMidiPortDetails> TryOpenMidiAsync(string name)
        {
            var midiPort = access.Inputs.FirstOrDefault(details => details.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (midiPort != null)
            {
                input = await access.OpenInputAsync(midiPort.Id);
                var portCreator = access.ExtensionManager.GetInstance<MidiPortCreatorExtension> ();
                output = portCreator.CreateVirtualInputSender(new MidiPortCreatorExtension.PortCreatorContext() {PortName = "Filtered Output"});
                return input.Details;
            }

            return null;
        }

        /// <summary>
        /// Runs until the cancellation token is signalled.
        /// </summary>
        public async Task Run(CancellationTokenSource cts)
        {
            if (input is null)
            {
                throw new InvalidOperationException("MIDI input device is not open.");
            }
            
            // Start the publisher task that uploads to GBQ
            var consumer = midiPublisher.Publish();

            // Register event handler, invoked whenever a MIDI message is received.
            input.MessageReceived += (obj, e) =>
            {
                if (cts.IsCancellationRequested)
                {
                    // Application is busy closing, don't accept more MIDI inputs.
                    return;
                }

                MidiEvent note = MidiFilter.Filter(e.Data, e.Start, e.Length, e.Timestamp);
                if (note is null)
                {
                    return;
                }
                // Pass on to virtual port immediately to minimize any delay to any device attached to virtual MIDI out
                output.Send(e.Data, 0, e.Length, e.Timestamp);
                
                // Write to queue - this will asynchronously buffer and upload and keeps this even processing fast.
                midiPublisher.TryWrite(note);
            };

            try
            {
                await Task.Delay(int.MaxValue, cts.Token);
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                // This will allow the writer task to gracefully exit
                midiPublisher.Complete();
            }

            await consumer;
            await input.CloseAsync();
            await output.CloseAsync();
        }
    }
}