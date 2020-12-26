using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;

namespace Dodgyrabbit.Google.Cloud.PubSub.V1
{
    /// <summary>
    /// A lightweight REST based PubSub publisher client, modelled very loosely on the official grpc based version
    /// by Google.
    /// </summary>
    public class PublisherClient
    {
        Uri uri;
        TokenResponse token;
        static readonly HttpClient client = new HttpClient();
        ServiceAccountCredential credential;
        readonly JsonSerializerOptions serializerOptions;
        
        public PublisherClient(string projectId, string topic, string serviceAccountCredentialFile)
        {
            // Setup URI and Accept headers once 
            uri = new Uri($"https://pubsub.googleapis.com/v1/projects/{projectId}/topics/{topic}:publish");
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            
            using (var stream = new FileStream(serviceAccountCredentialFile, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(new[] {"https://www.googleapis.com/auth/pubsub"})
                    .UnderlyingCredential as ServiceAccountCredential;
            }
            
            serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                IgnoreNullValues = true
            };
        }

        public async Task<bool> PublishAsync(PubSubPublishParameters value)
        {
            var serializedValue = JsonSerializer.Serialize(value, serializerOptions);
            
            int authenticationAttempts = 1;
            do
            {
                if (token == null || token.IsExpired(SystemClock.Default))
                {
                    if (await credential.RequestAccessTokenAsync(CancellationToken.None))
                    {
                        token = credential.Token;
                    }
                    authenticationAttempts--;
                }

                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, uri);
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                message.Content = new StringContent(serializedValue, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    token = null;
                }
            } while (authenticationAttempts > 0);
            return false;
        }
    }
}
