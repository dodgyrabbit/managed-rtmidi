using System;

namespace Commons.Music.Midi.RtMidi
{
    /// <summary>
    /// Represents a MIDI output device. You can send messages to MIDI output devices.
    /// </summary>
    public class RtMidiOutputDevice : RtMidiDevice
    {
        public RtMidiOutputDevice ()
            : base (RtMidi.rtmidi_out_create_default ())
        {
        }

        public RtMidiOutputDevice (RtMidiApi api, string clientName)
            : base (RtMidi.rtmidi_out_create (api, clientName))
        {
        }

        public override RtMidiApi CurrentApi => RtMidi.rtmidi_out_get_current_api (Handle);

        protected override void ReleaseDevice ()
        {
            RtMidi.rtmidi_out_free (Handle);
        }

        public void SendMessage (byte [] message, int length)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            // While it could emit message parsing error, it still returns 0...!
            RtMidi.rtmidi_out_send_message (Handle, message, length);
        }
    }
}