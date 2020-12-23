using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Music.Midi;

namespace midi_filter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var access = MidiAccessManager.Default;

            Console.WriteLine("MIDI inputs:");
            foreach (var port in access.Inputs)
            {
                Console.WriteLine(port.Name);
            }
            
            var midiPort = access.Inputs.FirstOrDefault(input => input.Name.Contains("out", StringComparison.OrdinalIgnoreCase));
            
            var portCreator = access.ExtensionManager.GetInstance<MidiPortCreatorExtension> ();
            var sender = portCreator.CreateVirtualInputSender(new MidiPortCreatorExtension.PortCreatorContext() {PortName = "whee"});
            
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

            Task.Run(async () =>
            {
                input.MessageReceived += (obj, e) =>
                {
                    Console.WriteLine($"INPUT: {e.Length} {e.Timestamp} {e.Data[0]}");
                    sender.Send(e.Data, 0, e.Length, e.Timestamp);
                };
                Console.WriteLine("Press <Ctrl>+c to exit...");
                await Task.Delay(int.MaxValue);

            }).Wait();

            await input.CloseAsync();
        }
    }
}
