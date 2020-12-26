using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;

namespace midi_filter
{
    class Program
    {
        // Search for a midi input device containing this string. This sample looks for the Virtual MIDI Piano Keyboard.
        const string MidiInputDeviceName = "VMPK Output";

        static async Task Main(string[] args)
        {
            var access = MidiAccessManager.Default;
            bool isVerbose = true;

            Console.WriteLine("MIDI input devices:");
            foreach (var inputDevice in access.Inputs)
            {
                Console.WriteLine(inputDevice.Name);
            }
            
            var midiPort = access.Inputs.FirstOrDefault(input => input.Name.Contains(MidiInputDeviceName, StringComparison.OrdinalIgnoreCase));
            IMidiInput input = null;
            if (midiPort != null)
            {
                Console.WriteLine($"Found {midiPort.Name}. Opening...");
                input = access.OpenInputAsync(midiPort.Id).Result;
            }
            else
            {
                Console.WriteLine("No suitable MIDI device found.");
                return;
            }
            
            var portCreator = access.ExtensionManager.GetInstance<MidiPortCreatorExtension> ();
            var sender = portCreator.CreateVirtualInputSender(new MidiPortCreatorExtension.PortCreatorContext() {PortName = "Filtered Output"});

            Task.Run(async () =>
            {
                input.MessageReceived += (obj, e) =>
                {
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
                    
                    // Is it a status byte?
                    if ((statusByte & 0xF0) > 0)
                    {
                        MidiEventType eventType = MidiEvent.GetEventType(statusByte);
                        if (eventType == MidiEventType.NoteOn || eventType == MidiEventType.NoteOff)
                        {
                            var note = new NoteMidiEvent(DateTime.UtcNow, statusByte, e.Data[1], e.Data[2]);
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
                        }
                    }
                    if (isVerbose)
                    {
                        Console.Write("Sending ");
                        for (int i=0; i < e.Length; i++)
                        {
                            Console.Write($"byte {i}={e.Data[i]} ");
                        }
                        Console.WriteLine();
                    }
                    sender.Send(e.Data, 0, e.Length, e.Timestamp);
                };
                Console.WriteLine("Press <Ctrl>+c to exit...");
                await Task.Delay(int.MaxValue);

            }).Wait();

            await input.CloseAsync();
        }
    }
}