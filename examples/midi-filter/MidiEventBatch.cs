using System.Collections.Generic;

namespace midi_filter
{
    /// <summary>
    /// Represents a batch of MIDI events associated with a device and is used to publish a PubSub message.
    /// </summary>
    public class MidiEventBatch
    {
        public string Device { get; set; }
        public string Tenant { get; set; }
        public List<NoteMidiEvent> Notes { get; set; }
        public List<ControlChangeMidiEvent> ControlChanges { get; set; }
    }
}
