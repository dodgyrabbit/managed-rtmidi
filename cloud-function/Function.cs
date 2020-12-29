using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Cloud.Functions.Framework.GcfEvents;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
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
            NoteMidiEvent note = System.Text.Json.JsonSerializer.Deserialize<NoteMidiEvent>(text);
                
            var row = new BigQueryInsertRow()
            {
                {"timestamp", note.DateTime},
                {"device", "1"},
                {"type", 1},
                {"name", note.SPN},
                {"value", (int) note.Note},
                {"velocity", (int) note.Velocity},
                {"noteon", note.IsNoteOn}
            };

            client.InsertRow("music", "notes", row);
            return Task.CompletedTask;
        }
    }
}