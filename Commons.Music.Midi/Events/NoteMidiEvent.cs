using System;
using System.Globalization;

namespace midi_filter
{
    public class NoteMidiEvent : ChannelMidiEvent
    {
        byte velocity;
        byte note;

        public NoteMidiEvent(DateTime dateTime, MidiEventType midiEventType, byte channel, byte note, byte velocity) : base (dateTime, midiEventType, channel)
        {
            this.note = note;
            this.velocity = velocity;
        }
        
        /// <summary>
        /// <c>true</c> if this is a Note On;<c>false</c> otherwise. 
        /// </summary>
        public bool IsNoteOn => midiEventType == MidiEventType.NoteOn;

        /// <summary>
        /// The note velocity from 0 to 255.
        /// </summary>
        public byte Velocity => velocity;

        /// <summary>
        /// The note value from 0 to 127. 0 is C-2, 60 is C3 (middle C).
        /// </summary>
        public byte Note => note;
        
        /// <summary>
        /// Scientific Pitch Notation. For example, Middle C is "C3".
        /// </summary>
        public string SPN
        {
            get
            {
                // C0 is the value 24. Calculate the octave for SPN.
                int octave = (note-24) / 12;
                
                // Pitch for C starts at 0, so modulus to get pitch regardless of octave
                int pitch = note % 12;

                string noteName = string.Empty;
                switch (pitch)
                {
                    case 0:
                        noteName = "C";
                        break;
                    case 1:
                        noteName = "C#";
                        break;
                    case 2:
                        noteName = "D";
                        break;
                    case 3:
                        noteName = "D#";
                        break;
                    case 4:
                        noteName = "E";
                        break;
                    case 5:
                        noteName = "F";
                        break;
                    case 6:
                        noteName = "F#";
                        break;
                    case 7:
                        noteName = "G";
                        break;
                    case 8:
                        noteName = "G#";
                        break;
                    case 9:
                        noteName = "A";
                        break;
                    case 10:
                        noteName = "A#";
                        break;
                    case 11:
                        noteName = "B";
                        break;
                }

                return $"{noteName}{octave}";
            }
        }
    }
}