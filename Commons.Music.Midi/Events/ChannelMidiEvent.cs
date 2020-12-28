using System;

namespace midi_filter
{
    public class ChannelMidiEvent : MidiEvent
    {
        public ChannelMidiEvent(DateTime dateTime, byte status) : base(dateTime, status)
        {
        }

        public byte Channel => (byte) (status & 0xF);
    }
}