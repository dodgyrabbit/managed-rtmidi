using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dodgyrabbit.Google.Cloud.PubSub.V1;
using Google.Cloud.PubSub.V1;
using System.Text.Json.Serialization;
using Google.Cloud.BigQuery.V2;
using midi_filter;

namespace midi_cloud
{
    class Program
    {
        static string ServiceAccountCredentialFile = "";
        
        static async Task Main(string[] args)
        {
            // await PublishAsync();
            PullAndUploadToGBQ("", "", "", "");

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        
        static int PullAndUploadToGBQ(string projectId, string subscriptionId, string datasetId, string tableId)
        {
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
            SubscriberServiceApiClient subscriber = SubscriberServiceApiClient.Create();

            BigQueryClient client = BigQueryClient.Create(projectId);
            ConcurrentBag<BigQueryInsertRow> rows = new ConcurrentBag<BigQueryInsertRow>();
            
            // Pull messages from the subscription. We're returning immediately, whether or not there
            // are messages; in other cases you'll want to allow the call to wait until a message arrives.
            PullResponse response = subscriber.Pull(subscriptionName, returnImmediately: true, maxMessages: Int32.MaxValue);
            foreach (ReceivedMessage received in response.ReceivedMessages)
            {
                PubsubMessage message = received.Message;
                string text = Encoding.UTF8.GetString(message.Data.ToByteArray());
                
                NoteMidiEvent note = System.Text.Json.JsonSerializer.Deserialize<NoteMidiEvent>(text);
                
                Console.WriteLine($"Message {message.MessageId}: {text}");
                // The insert ID is optional, but can avoid duplicate data when retrying inserts.
                rows.Add(new BigQueryInsertRow()
                {
                    {"timestamp", note.DateTime},
                    {"device", "1"},
                    {"type", 1},
                    {"name", note.SPN},
                    {"value", (int) note.Note},
                    {"velocity", (int) note.Velocity},
                    {"noteon", note.IsNoteOn}
                });
            }

            if (rows.Count > 0)
            {
                client.InsertRows(datasetId, tableId, rows);
                subscriber.Acknowledge(subscriptionName, response.ReceivedMessages.Select(m => m.AckId));
            }
            return rows.Count;
        }

        static async Task PublishAsync()
        {
            var publisherClient = new Dodgyrabbit.Google.Cloud.PubSub.V1.PublisherClient("", "", ServiceAccountCredentialFile);
            PubSubPublishParameters parameters = new PubSubPublishParameters();
            parameters.Messages = new List<PubSubMessage>();

            for (int i = 0; i < 1; i++)
            {
                var pubSubMessage = new PubSubMessage();
                pubSubMessage.Data = Convert.ToBase64String(Encoding.UTF8.GetBytes($"Message={i}"));
                parameters.Messages.Add(pubSubMessage);
            }
            await publisherClient.PublishAsync(parameters);
        }
    }
}
