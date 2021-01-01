using System;
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
            IMidiPortDetails details;
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