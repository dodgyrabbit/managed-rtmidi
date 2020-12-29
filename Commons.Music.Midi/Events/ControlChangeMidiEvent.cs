using System;

namespace midi_filter
{
    /// <summary>
    /// Represents a Control Change even from a MIDI controller, such as depressing the piano pedal.
    /// </summary>
    public class ControlChangeMidiEvent : ChannelMidiEvent
    {
        protected byte controller;
        protected byte value;

        public ControlChangeMidiEvent()
        {
        }
        
        public ControlChangeMidiEvent(DateTime dateTime, MidiEventType midiEventType, byte channel, byte controller, byte value) : base(dateTime, midiEventType, channel)
        {
            this.controller = controller;
            this.value = value;
        }

        public byte Value
        {
            get => value;
            set => this.value = value;
        }

        public byte Controller
        {
            get => controller;
            set => controller = value;
        }
    }
}