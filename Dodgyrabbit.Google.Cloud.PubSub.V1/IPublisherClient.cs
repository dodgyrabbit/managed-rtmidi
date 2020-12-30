using System.Threading.Tasks;

namespace Dodgyrabbit.Google.Cloud.PubSub.V1
{
    public interface IPublisherClient
    {
        public Task<bool> PublishAsync(PubSubPublishParameters value);
    }
}