using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Music.Midi;
using Dodgyrabbit.Google.Cloud.PubSub.V1;

namespace midi_filter
{
    static class Program
    {
        // ReSharper disable once UnusedParameter.Local
        static async Task Main(string[] args)
        {
            var devices = new[] {"VMPK", "mio"};

            using CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                // Process will not end
                eventArgs.Cancel = true;

                // Signal that we're ending
                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore if cancellation token is already disposed (event fired as we're existing)
                }
            };

            var pubSub = new PublisherClient("", "", "");
            var midiPublisher = new MidiPublisher(cts, pubSub);
            var application = new Application(MidiAccessManager.Default, midiPublisher);

            application.PrintMidiInputDevices();

            Console.WriteLine("Press <ctrl>+c to exit");
            Console.WriteLine("Searching for MIDI input device...");
            IMidiPortDetails details =  null;
            do
            {
                details = await TryOpenMidiDeviceAsync(application, devices);

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

        static async Task<IMidiPortDetails> TryOpenMidiDeviceAsync(Application application, string[] devices)
        {
            IMidiPortDetails details = null;
            foreach (var device in devices)
            {
                details = await application.TryOpenMidiAsync(device);
                if (details != null)
                {
                    break;
                }
            }

            return details;
        }
    }
}