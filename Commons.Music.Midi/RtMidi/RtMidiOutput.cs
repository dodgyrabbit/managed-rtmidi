using System;
using System.Threading.Tasks;

namespace Commons.Music.Midi.RtMidi
{
    class RtMidiOutput : RtMidiPort, IMidiOutput
    {
        public RtMidiOutput (RtMidiPortDetails portDetails)
            : base (portDetails)
        {
        }

        RtMidiOutputDevice impl;

        public override Task CloseAsync ()
        {
            if (Connection != MidiPortConnectionState.Open || impl == null)
                throw new InvalidOperationException ("No open output.");
            impl.Close ();
            Connection = MidiPortConnectionState.Closed;
            return completed_task;
        }

        public override Task OpenAsync ()
        {
            Connection = MidiPortConnectionState.Pending;
            impl = MidiDeviceManager.OpenOutput (((RtMidiPortDetails) Details).RawId);
            Connection = MidiPortConnectionState.Open;
            return completed_task;
        }

        public void Send (byte [] mevent, int offset, int length, long timestamp)
        {
            if (timestamp > 0)
            {
                throw new InvalidOperationException("non-zero timestamp is not supported");
            }

            if (mevent == null)
            {
                throw new ArgumentNullException("mevent");
            }

            if (mevent.Length == 0)
            {
                return; // do nothing
            }

            impl.SendMessage (mevent, length);
        }
    }
}