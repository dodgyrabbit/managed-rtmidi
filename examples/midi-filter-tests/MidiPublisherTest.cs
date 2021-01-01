using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Dodgyrabbit.Google.Cloud.PubSub.V1;
using midi_filter;
using Moq;
using Xunit;

namespace midi_filter_tests
{
    public class MidiPublisherTest
    {
        [Fact]
        public async Task SerializationTest()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            MockPublisherClient publisherClient = new MockPublisherClient();
            MidiPublisher midiPublisher = new MidiPublisher(cts, publisherClient);
            Task publish = midiPublisher.Publish();
            midiPublisher.TryWrite(new NoteMidiEvent(DateTime.UtcNow, MidiEventType.NoteOn, 0, 1, 2));
            midiPublisher.TryWrite(new NoteMidiEvent(DateTime.UtcNow, MidiEventType.NoteOn, 0, 1, 2));
            var publishedValue = publisherClient.Dequeue(TimeSpan.FromSeconds(1));
            Assert.Single(publishedValue.Messages);
            midiPublisher.Complete();
            cts.Cancel();
            await publish;
        }

        class MockPublisherClient : IPublisherClient
        {
            ConcurrentQueue<PubSubPublishParameters> values = new ConcurrentQueue<PubSubPublishParameters>();

            public PubSubPublishParameters Dequeue(TimeSpan timeout)
            {
                DateTime expiry = DateTime.UtcNow + timeout;

                do
                {
                    if (values.TryDequeue(out var value))
                    {
                        return value;
                    }
                } while (DateTime.UtcNow < expiry);

                throw new TimeoutException("No value in the queue");
            }
            public Task<bool> PublishAsync(PubSubPublishParameters value)
            {
                values.Enqueue(value);
                return Task.FromResult(true);
            }
        }
    }
}