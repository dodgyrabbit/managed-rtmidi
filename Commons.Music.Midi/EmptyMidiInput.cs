using System;

namespace Commons.Music.Midi
{
    class EmptyMidiInput : EmptyMidiPort, IMidiInput, IMockMidiInput
    {
        static EmptyMidiInput ()
        {
            Instance = new EmptyMidiInput ();
        }

        public static EmptyMidiInput Instance { get; private set; }

        public event EventHandler<MidiReceivedEventArgs> MessageReceived;

        internal override IMidiPortDetails CreateDetails ()
        {
            return new EmptyMidiPortDetails ("dummy_in", "Dummy MIDI Input");
        }

        public void MockMessageReceived(byte[] bytes, int offset, int length, long timestamp)
        {
            MessageReceived (this, new MidiReceivedEventArgs { Data = bytes, Start = 0, Length = bytes.Length, Timestamp = (long) timestamp });
        }
    }
}