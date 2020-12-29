using System;

namespace midi_filter
{
    public abstract class MidiEvent
    {
        DateTime dateTime;
        protected MidiEventType midiEventType;

        public MidiEvent()
        {
        }

        public MidiEvent(DateTime dateTime, MidiEventType midiEventType)
        {
            this.dateTime = dateTime;
            this.midiEventType = midiEventType;
        }

        public DateTime DateTime
        {
            get => dateTime;
            set => dateTime = value;
        }

        public MidiEventType MidiEventType
        {
            get => midiEventType;
            set => midiEventType = value;
        }

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

