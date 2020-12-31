using System;
using System.Threading.Channels;

namespace midi_filter
{
    public static class MidiFilter
    {
        public static MidiEvent Filter(byte[] data, int start, int length, long timestamp)
        {
            // MIDI message format
            // byte 0 = status byte
            // byte 1 data
            // byte 2 data

            // Nothing to send
            if (length < 1)
            {
                return null;
            }

            byte statusByte = data[0];

            // Reality check - is the high bit set? Status bytes starts with
            if ((statusByte & 0b1000_0000) > 0)
            {
                MidiEventType eventType = MidiEvent.ToMidiEventType(statusByte);
                if (eventType == MidiEventType.NoteOn || eventType == MidiEventType.NoteOff)
                {
                    var note = new NoteMidiEvent(DateTime.UtcNow, eventType, ChannelMidiEvent.ToChannel(statusByte), data[1], data[2]);
                    if (note.IsNoteOn && note.Velocity == 0)
                    {
                        // Filter out note
                        return null;
                    }

                    return note;
                }
            }

            return null;
        }
    }
}