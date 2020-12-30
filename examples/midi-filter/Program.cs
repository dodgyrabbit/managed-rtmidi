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
        static async Task Main(string[] args)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, eventArgs) =>
                {
                    // Process will not end
                    eventArgs.Cancel = true;

                    // Signal that we're ending
                    cts.Cancel();
                });

                var pubSub = new PublisherClient("", "", "");
                var application = new Application(pubSub, MidiAccessManager.Default);

                application.PrintMidiInputDevices();

                Console.WriteLine($"Searching for MIDI input device...");
                IMidiPortDetails details = null;
                do
                {
                    details = await application.TryOpenMidiAsync("VMPK");
                    if (details is not null)
                    {
                        Console.WriteLine($"Found {details.Name}");
                    }
                    else
                    {
                        try
                        {
                            await Task.Delay(1000, cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            // Expected if the user cancels with <ctrl>+c
                        }
                    }
                } while (details is null && !cts.IsCancellationRequested);

                if (!cts.IsCancellationRequested)
                {
                    try
                    {
                        await application.Run(cts);
                    }
                    catch (TaskCanceledException)
                    {
                        // Expected if the user cancels with <ctrl>+c
                    }
                }
            }
        }
    }
}