using System;

namespace midi_filter
{
    public class ChannelMidiEvent : MidiEvent
    {
        protected byte channel;
        
        public ChannelMidiEvent(DateTime dateTime, MidiEventType midiEventType, byte channel) : base(dateTime, midiEventType)
        {
            this.channel = channel;
        }

        public byte Channel => channel;

        public static byte ToChannel(byte status)
        {
            if ((status >= 0x80) && (status < 0xF0))
            {
                return (byte) (status & 0xF);
            }
            throw  new InvalidOperationException("The status byte is not for a channel message.");
        }
    }
}