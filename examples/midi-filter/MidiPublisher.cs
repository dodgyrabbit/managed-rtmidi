using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dodgyrabbit.Google.Cloud.PubSub.V1;

namespace midi_filter
{
    /// <summary>
    /// Publishes MIDI events to a PubSub topic.
    /// </summary>
    public class MidiPublisher
    {   
        static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            IgnoreNullValues = true
        };
        int batchSize;
        IPublisherClient publisherClient;
        TimeSpan uploadInterval;
        Channel<MidiEvent> channel = Channel.CreateUnbounded<MidiEvent>();
        ChannelReader<MidiEvent> reader;
        ChannelWriter<MidiEvent> writer;
        CancellationTokenSource cts;

        public MidiPublisher(CancellationTokenSource cts, IPublisherClient publisherClient, int batchSize = 10, long uploadIntervalMilliseconds = 5000)
        {
            this.publisherClient = publisherClient;
            this.batchSize = batchSize;
            uploadInterval = TimeSpan.FromMilliseconds(uploadIntervalMilliseconds);
            reader = channel.Reader;
            writer = channel.Writer;
            this.cts = cts;
        }

        /// <summary>
        /// Returns a Task that runs until Complete() is called on this class.
        /// </summary>
        public Task Publish()
        {
            PubSubPublishParameters parameters = new PubSubPublishParameters();
            parameters.Messages = new List<PubSubMessage>();

            return Task.Run(async () =>
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
            });
        }

        public ValueTask WriteAsync(MidiEvent midiEvent)
        {
            try
            {
                return writer.WriteAsync(midiEvent);
            }
            catch (ChannelClosedException)
            {
                // We expect this when cancellation is requested. If it wasn't, throw since this could be another issue.
                if (!cts.IsCancellationRequested)
                {
                    throw;
                }
            }
            return ValueTask.FromCanceled(cts.Token);
        }

        public void Complete()
        {
            channel.Writer.Complete();
        }
    }
}