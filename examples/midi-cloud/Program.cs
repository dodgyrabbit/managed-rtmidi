using System;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;

namespace midi_cloud
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string projectId = "";
            string topicId = "";
            
            // The environment variable is required for specifying service account details
            //export GOOGLE_APPLICATION_CREDENTIALS=<path-to-json-file>
            
            PublisherClient publisher = await PublisherClient.CreateAsync(new TopicName(projectId, topicId));
            string messageId = await publisher.PublishAsync("Hello, Pubsub");
            
            // PublisherClient instance should be shutdown after use.
            // The TimeSpan specifies for how long to attempt to publish locally queued messages.
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));
        }
    }
}
