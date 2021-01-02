using System;
using System.Threading;
using System.Threading.Tasks;
using Commons.Music.Midi;
using Dodgyrabbit.Google.Cloud.PubSub.V1;
using midi_filter;
using Moq;
using Xunit;

namespace midi_filter_tests
{
    public class ApplicationTest
    {
        [Fact]
        public async Task PublisherClientTest()
        {
            IMidiAccess2 access = new EmptyMidiAccess();
            IMidiInput input = await access.OpenInputAsync("dummy_in");
            var portCreator = access.ExtensionManager.GetInstance<MidiPortCreatorExtension> ();
            var output = portCreator.CreateVirtualInputSender(new MidiPortCreatorExtension.PortCreatorContext() {PortName = "Test port"});
            var publisherClient = new Mock<IPublisherClient>();
            
            PubSubPublishParameters parameters = new PubSubPublishParameters();
            publisherClient.Setup(client => client.PublishAsync(parameters)).ReturnsAsync(true);
            CancellationTokenSource cts = new CancellationTokenSource();
            MidiPublisher midiPublisher = new MidiPublisher(cts, publisherClient.Object);
            
            var app = new Application(new EmptyMidiAccess(), midiPublisher);
            await app.TryOpenMidiAsync("Dummy");

            var theApp = app.Run(cts);

            await Task.Delay(10, CancellationToken.None);

            if (input is IMockMidiInput sender)
            {
                sender.MockMessageReceived(new byte[] {0x90, 2, 3}, 0, 3, 0);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            cts.Cancel();
            theApp.Wait(CancellationToken.None);
        }
        
        [Fact]
        public async Task TryOpenMidiTest()
        {
            var application = new Application(new EmptyMidiAccess(), null);
            Assert.NotNull(await application.TryOpenMidiAsync("Dummy"));
            Assert.Null(await application.TryOpenMidiAsync("DOESNT EXIST"));
        }
    }
}
