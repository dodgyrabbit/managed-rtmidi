using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Commons.Music.Midi;
using System.Text.Json;
using Dodgyrabbit.Google.Cloud.PubSub.V1;

namespace midi_filter
{
    class Program
    {
        // Search for a midi input device containing this string. This sample looks for the Virtual MIDI Piano Keyboard.
        const string MidiInputDeviceName = "VMPK Output";
        
        static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            IgnoreNullValues = true
        };

        static async Task Main(string[] args)
        {
            var access = MidiAccessManager.Default;
            var pubSub = new PublisherClient("cloud-piano", "notes", "");
            bool isVerbose = true;

            var channel = Channel.CreateUnbounded<MidiEvent>();
            var reader = channel.Reader;
            var writer = channel.Writer;

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
            var uploadBatchTime = TimeSpan.FromSeconds(1);
            
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

                        if (await pubSub.PublishAsync(parameters))
                        {
                            Console.WriteLine("Success.");
                        }
                        else
                        {
                            Console.WriteLine("Fail.");
                        }
                    }

                    await Task.Delay(uploadBatchTime);
                }
            });

            Task.Run(async () =>
            {
                input.MessageReceived += async (obj, e) =>
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

                            await writer.WriteAsync(note);
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
                channel.Writer.Complete();

            }).Wait();

            await consumer;
            Console.WriteLine("Consumer exited");    

            await input.CloseAsync();
        }
    }
}