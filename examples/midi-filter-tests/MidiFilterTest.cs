using Xunit;
using midi_filter;

namespace midi_filter_tests
{
    public class MidiFilterTest
    {
        [Fact]
        public void DontFilterNoteOnWithNonZeroVelocityTest()
        {
            MidiEvent midiEvent = MidiFilter.Filter(new[] {(byte) MidiEventType.NoteOn, (byte) 1, (byte) 1}, 0, 3, 0);
            Assert.NotNull(midiEvent);
        }
        
        [Fact]
        public void DontFilterNoteOffTest()
        {
            MidiEvent midiEvent = MidiFilter.Filter(new[] {(byte) MidiEventType.NoteOff, (byte) 1, (byte) 1}, 0, 3, 0);
            Assert.NotNull(midiEvent);
            
            midiEvent = MidiFilter.Filter(new[] {(byte) MidiEventType.NoteOff, (byte) 1, (byte) 0}, 0, 3, 0);
            Assert.NotNull(midiEvent);
        }
        
        [Fact]
        public void FilterNoteOnWithZeroVelocityTest()
        {
            MidiEvent midiEvent = MidiFilter.Filter(new[] {(byte) MidiEventType.NoteOn, (byte) 1, (byte) 0}, 0, 3, 0);
            Assert.Null(midiEvent);
        }
        
        [Fact]
        public void EmptyMessageTest()
        {
            MidiEvent midiEvent = MidiFilter.Filter(new byte[0], 0, 0, 0);
            Assert.Null(midiEvent);
        }
    }
}