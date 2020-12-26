using System.Collections.Generic;

namespace Dodgyrabbit.Google.Cloud.PubSub.V1
{
    public class PubSubPublishParameters
    {
        public List<PubSubMessage> Messages
        {
            get;
            set;
        }
    }
}