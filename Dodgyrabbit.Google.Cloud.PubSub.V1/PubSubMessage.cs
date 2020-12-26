namespace Dodgyrabbit.Google.Cloud.PubSub.V1
{
    public class PubSubMessage
    {
        // Must be base64 encoded
        // In our case, this should be a MIDI message abstraction (Note on, Velocity, Device, TimeStamp) 
        public string Data { get; set; }
        public string MessageId { get; set; }
        
        // A timestamp in RFC3339 UTC "Zulu" format, with nanosecond resolution and up to nine fractional digits.
        // Examples: "2014-10-02T15:01:23Z" and "2014-10-02T15:01:23.045123456Z".
        public string PublishTime { get; set; }
        public string OrderingKey { get; set; }
    }
}