using System;
using System.Threading.Tasks;

namespace Commons.Music.Midi.RtMidi
{
    class RtMidiInput : RtMidiPort, IMidiInput
    {
        public RtMidiInput (RtMidiPortDetails portDetails)
            : base (portDetails)
        {
        }

        public event EventHandler<MidiReceivedEventArgs> MessageReceived;

        RtMidiInputDevice impl;
        RtMidiCCallback callback;

        public override Task CloseAsync ()
        {
            if (Connection != MidiPortConnectionState.Open || impl == null)
            {
                throw new InvalidOperationException("No open input.");
            }

            impl.Close ();
            Connection = MidiPortConnectionState.Closed;
            return completed_task;
        }

        public override unsafe Task OpenAsync ()
        {
            Connection = MidiPortConnectionState.Pending;
            impl = MidiDeviceManager.OpenInput (((RtMidiPortDetails)Details).RawId);
			
            // An explicit reference to the callback function must be maintained. Since the callback is only referenced
            // by unmanaged code, it will get garbage collected at some point and an exception will be thrown.
            callback = (timestamp, message, size, data) =>
            {
                var bytes = new byte [(int)size];
                System.Runtime.InteropServices.Marshal.Copy ((IntPtr) message, bytes, 0, (int) size);
                MessageReceived (this, new MidiReceivedEventArgs { Data = bytes, Start = 0, Length = bytes.Length, Timestamp = (long) timestamp });
            };
			
            impl.SetCallback(callback, IntPtr.Zero );
		
            Connection = MidiPortConnectionState.Open;
            return completed_task;
        }
    }
}