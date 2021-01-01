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
    /// Publishes MIDI events to a PubSub topic. Handles periodic uploading/batching.
    /// </summary>
    public class MidiPublisher
    {   
        static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            IgnoreNullValues = true
        };
        readonly int batchSize;
        readonly IPublisherClient publisherClient;
        readonly TimeSpan uploadInterval;

        readonly Channel<MidiEvent> channel = Channel.CreateUnbounded<MidiEvent>(new UnboundedChannelOptions
        {
            SingleReader = true
        });
        readonly ChannelReader<MidiEvent> reader;
        readonly ChannelWriter<MidiEvent> writer;
        readonly CancellationTokenSource cts;

        public MidiPublisher(CancellationTokenSource cts, IPublisherClient publisherClient, int batchSize = 10, long uploadIntervalMilliseconds = 5000)
        {
            this.publisherClient = publisherClient;
            this.batchSize = batchSize;
            uploadInterval = TimeSpan.FromMilliseconds(uploadIntervalMilliseconds);
            reader = channel.Reader;
            writer = channel.Writer;
            // TODO: Better to hide this CTS and have a private one when channel is completed?
            this.cts = cts;
        }

        /// <summary>
        /// Returns a Task that runs until Complete() is called on this class.
        /// </summary>
        public Task Publish()
        {
            PubSubPublishParameters parameters = new PubSubPublishParameters {Messages = new List<PubSubMessage>()};
            MidiEventBatch midiEventBatch = new MidiEventBatch {Notes = new List<NoteMidiEvent>(), ControlChanges = new List<ControlChangeMidiEvent>(), Device = "1", Tenant = "1"};

            return Task.Run(async () =>
            {
                while (await reader.WaitToReadAsync())
                {
                    parameters.Messages.Clear();
                    midiEventBatch.Notes.Clear();
                    midiEventBatch.ControlChanges.Clear();
                    while (reader.TryRead(out var midiEvent))
                    {
                        if (midiEvent is NoteMidiEvent note)
                        {
                            midiEventBatch.Notes.Add(note);
                        }

                        if (midiEvent is ControlChangeMidiEvent controlChange)
                        {
                            midiEventBatch.ControlChanges.Add(controlChange);
                        }
                    }
                    
                    var message = new PubSubMessage();
                    var serializedValue = JsonSerializer.Serialize(midiEventBatch, serializerOptions);
                    message.Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedValue));
                    parameters.Messages.Add(message);

                    if (parameters.Messages.Count > 0)
                    {
                        Console.WriteLine($"Uploading {midiEventBatch.Notes.Count} notes and {midiEventBatch.ControlChanges.Count} pedal events");
                        try
                        {
                            var isUploaded = await publisherClient.PublishAsync(parameters);
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
                        await Task.Delay(uploadInterval, cts.Token) ;
                    }
                    catch (TaskCanceledException)
                    {
                    }
                }
            });
        }

        public bool TryWrite(MidiEvent midiEvent)
        {
            try
            {
                return writer.TryWrite(midiEvent);
            }
            catch (ChannelClosedException)
            {
                // We expect this when cancellation is requested. If it wasn't, throw since this could be another issue.
                if (!cts.IsCancellationRequested)
                {
                    throw;
                }

                return false;
            }
        }

        public void Complete()
        {
            channel.Writer.Complete();
        }
    }
}