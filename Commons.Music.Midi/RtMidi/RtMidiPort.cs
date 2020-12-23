using System;
using System.Threading.Tasks;

namespace Commons.Music.Midi.RtMidi
{
    abstract class RtMidiPort : IMidiPort
    {
        internal static Task completed_task = Task.FromResult (false);

        protected RtMidiPort (RtMidiPortDetails portDetails)
        {
            if (portDetails == null)
            {
                throw new ArgumentNullException("portDetails");
            }

            Details = portDetails;
            Connection = MidiPortConnectionState.Closed;
        }

        public MidiPortConnectionState Connection { get; internal set; }
        public IMidiPortDetails Details { get; private set; }

        public abstract Task CloseAsync ();
        public abstract Task OpenAsync ();

        public void Dispose ()
        {
            if (Connection == MidiPortConnectionState.Open)
            {
                CloseAsync();
            }
        }
    }
}