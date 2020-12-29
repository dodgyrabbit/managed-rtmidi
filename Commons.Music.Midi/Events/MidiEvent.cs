using System;

namespace midi_filter
{
    public abstract class MidiEvent
    {
        DateTime dateTime;
        protected MidiEventType midiEventType; 

        public MidiEvent(DateTime dateTime, MidiEventType midiEventType)
        {
            this.dateTime = dateTime;
            this.midiEventType = midiEventType;
        }

        public DateTime DateTime => dateTime;

        public MidiEventType MidiEventType => midiEventType;

        public static MidiEventType ToMidiEventType(byte status)
        {
            if ((status >= 0x80) && (status < 0xF0))
            {
                return (MidiEventType) (status & 0xF0);
            }
            return (MidiEventType) status;
        }
    }
}

