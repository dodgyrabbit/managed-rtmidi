using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Commons.Music.Midi;
using Dodgyrabbit.Google.Cloud.PubSub.V1;

namespace midi_filter
{
    public class Application
    {
        // Search for a midi input device containing this string
        static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    IgnoreNullValues = true
                };
        int batchSize;
        TimeSpan uploadInterval;
        IMidiAccess2 access;
        IPublisherClient publisherClient;
        IMidiInput input;
        IMidiOutput output;

        /// <summary>
        /// Creates a new Application instance.
        /// </summary>
        /// <param name="serviceCredentialsFileName">The full path to the service account JSON file.</param>
        /// <param name="projectId">The GCP project that contains the PubSub topic.</param>
        /// <param name="topicId">The PubSub topic to publish to.</param>
        /// <param name="batchSize">The maximum number of MIDI messages per PubSub message.</param>
        /// <param name="uploadIntervalMilliseconds">The interval to publish to PubSub.</param>
        public Application(IPublisherClient publisherClient, IMidiAccess2 access, int batchSize = 10, long uploadIntervalMilliseconds = 5000)
        {
            this.publisherClient = publisherClient;
            this.access = access;
            this.batchSize = batchSize;
            uploadInterval = TimeSpan.FromMilliseconds(uploadIntervalMilliseconds);
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
            var midiPort = access.Inputs.FirstOrDefault(input => input.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (midiPort != null)
            {
                
                input = await access.OpenInputAsync(midiPort.Id);
                var portCreator = access.ExtensionManager.GetInstance<MidiPortCreatorExtension> ();
                output = portCreator.CreateVirtualInputSender(new MidiPortCreatorExtension.PortCreatorContext() {PortName = "Filtered Output"});
                return input.Details;
            }
            else
            {
                return null;
            }
        }

        public async Task Run(CancellationTokenSource cts)
        {
            if (input is null)
            {
                throw new InvalidOperationException("MIDI input device is not open.");
            }
            
            bool isVerbose = true;
            var channel = Channel.CreateUnbounded<MidiEvent>();
            var reader = channel.Reader;
            var writer = channel.Writer;
            
            PubSubPublishParameters parameters = new PubSubPublishParameters();
            parameters.Messages = new List<PubSubMessage>();
            
            var consumer = Task.Run(async () =>
            {
                while (await reader.WaitToReadAsync())
                {
                    parameters.Messages.Clear();
                    MidiEvent midiEvent;
                    while (reader.TryRead(out midiEvent))
                    {
                        NoteMidiEvent note = midiEvent as NoteMidiEvent;
                        if (note is not null)
                        {
                            var message = new PubSubMessage();
                            var serializedValue = JsonSerializer.Serialize(note, serializerOptions);
                            message.Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedValue));
                            parameters.Messages.Add(message);
                        }
                    }
                    if (parameters.Messages.Count > 0)
                    {
                        Console.WriteLine($"Uploading: {parameters.Messages.Count} MIDI events");
                        bool isUploaded;
                        try
                        {
                            isUploaded = await publisherClient.PublishAsync(parameters);
                            if (!isUploaded)
                            {
                                Console.WriteLine("Failed to upload to PubSub without exception. Probably auth issue.");
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("Failed to upload to PubSub: " + exception.Message);
                        }
                    }

                    // If we're not busy cancelling, wait for the next upload slot. Otherwise loop and upload until
                    // we're done. This effectively means we're draining the queue without waiting when cancelling.
                    try
                    {
                        await Task.Delay(uploadInterval, cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
                Console.WriteLine("Writer task exiting");
            });

            Task.Run(async () =>
            {
                input.MessageReceived += async (obj, e) =>
                {
                    if (cts.IsCancellationRequested)
                    {
                        // Application is busy closing, don't accept more MIDI inputs.
                        return;
                    }

                    // MIDI message format
                    // byte 0 = status byte
                    // byte 1 data
                    // byte 2 data

                    // Nothing to send
                    if (e.Length < 1)
                    {
                        return;
                    }

                    byte statusByte = e.Data[0];

                    // Reality check - is the high bit set? Status bytes starts with
                    if ((statusByte & 0b1000_0000) > 0)
                    {
                        MidiEventType eventType = MidiEvent.ToMidiEventType(statusByte);
                        if (eventType == MidiEventType.NoteOn || eventType == MidiEventType.NoteOff)
                        {
                            var note = new NoteMidiEvent(DateTime.UtcNow, eventType, ChannelMidiEvent.ToChannel(statusByte), e.Data[1], e.Data[2]);
                            if (note.IsNoteOn && note.Velocity == 0)
                            {
                                if (isVerbose)
                                {
                                    Console.WriteLine("Removing 0 velocity note off");
                                }
                                return;
                            }

                            if (isVerbose)
                            {
                                Console.WriteLine($"{note.SPN} at velocity {note.Velocity} on channel {note.Channel} is {note.IsNoteOn}");
                            }

                            try
                            {
                                await writer.WriteAsync(note);
                            }
                            catch (ChannelClosedException)
                            {
                                // We expect this when cancellation is requested. If it wasn't, throw since this could be another issue.
                                if (!cts.IsCancellationRequested)
                                {
                                    throw;
                                }
                            }
                        }
                    }

                    if (isVerbose)
                    {
                        Console.Write("Sending ");
                        for (int i = 0; i < e.Length; i++)
                        {
                            Console.Write($"byte {i}={e.Data[i]} ");
                        }
                        Console.WriteLine();
                    }

                    output.Send(e.Data, 0, e.Length, e.Timestamp);
                };

                Console.WriteLine("Press <Ctrl>+c to exit...");

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
                    channel.Writer.Complete();
                }
            }).Wait();

            await consumer;
            Console.WriteLine("Consumer exited");    

            await input.CloseAsync();
            await output.CloseAsync();
        }
    }
}