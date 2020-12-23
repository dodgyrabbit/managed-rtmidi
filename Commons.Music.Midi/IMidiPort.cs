using System.Threading.Tasks;

namespace Commons.Music.Midi
{
    public interface IMidiPort
    {
        IMidiPortDetails Details { get; }
        MidiPortConnectionState Connection { get; }
        Task CloseAsync ();
    }
}