using System;

namespace midi_filter
{
    public abstract class MidiEvent
    {
        DateTime dateTime;
        protected byte status;
        protected MidiEventType midiEventType; 

        public MidiEvent(DateTime dateTime, byte status)
        {
            this.dateTime = dateTime;
            this.status = status;
            midiEventType = GetEventType(status);
        }

        public static MidiEventType GetEventType(byte status)
        {
            if ((status >= 0x80) && (status < 0xF0))
            {
                return (MidiEventType) (status & 0xF0);
            }
            return (MidiEventType) status;
        }

        DateTime DateTime => dateTime;
    }
}

