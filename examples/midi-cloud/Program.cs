using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dodgyrabbit.Google.Cloud.PubSub.V1;

namespace midi_cloud
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string ServiceAccountCredentialFile = "";

            PublisherClient publisherClient = new PublisherClient("cloud-piano", "notes", ServiceAccountCredentialFile);
            PubSubPublishParameters parameters = new PubSubPublishParameters();
            parameters.Messages = new List<PubSubMessage>();

            for (int i = 0; i < 1; i++)
            {
                var pubSubMessage = new PubSubMessage();
                pubSubMessage.Data = Convert.ToBase64String(Encoding.UTF8.GetBytes($"Message={i}"));
                parameters.Messages.Add(pubSubMessage);
            }

            await publisherClient.PublishAsync(parameters);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
