using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;
using midi_filter;
using Google.Events.Protobuf.Cloud.PubSub.V1;

namespace Dodgyrabbit.Cloud.Piano
{
    public class Function : ICloudEventFunction<MessagePublishedData>
    {
        /// <summary>
        /// Called whenever a PubSub message is available for the topic this function is associated with. It will
        /// automatically be ACKed. Uploads the value to GBQ.
        /// </summary>
        public Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData pubsub, CancellationToken cancellationToken)
        {
            BigQueryClient client = BigQueryClient.Create("cloud-piano");
            string text = Encoding.UTF8.GetString(pubsub.Message.Data.ToByteArray());
            
            MidiEventBatch batch = System.Text.Json.JsonSerializer.Deserialize<MidiEventBatch>(text);
            var rows = new List<BigQueryInsertRow>();

            if (batch.Notes != null)
            {
                foreach (var note in batch.Notes)
                {
                    rows.Add(new BigQueryInsertRow
                    {
                        {"timestamp", note.DateTime},
                        {"tenant", batch.Tenant},
                        {"device", batch.Device},
                        {"eventtype", (int) note.MidiEventType},
                        {"value1", (int) note.Note},
                        {"value2", (int) note.Velocity},
                        {"description1", note.SPN}
                    });
                }
            }

            if (batch.ControlChanges != null)
            {
                foreach (var controlChange in batch.ControlChanges)
                {
                    rows.Add(new BigQueryInsertRow
                    {
                        {"timestamp", controlChange.DateTime},
                        {"tenant", batch.Tenant},
                        {"device", batch.Device},
                        {"eventtype", (int) controlChange.MidiEventType},
                        {"value1", (int) controlChange.Controller},
                        {"value2", (int) controlChange.Value},
                    });
                }
            }

            client.InsertRows("music", "notes2", rows);
            return Task.CompletedTask;
        }
    }
}