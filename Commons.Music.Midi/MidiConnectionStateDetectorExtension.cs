using System;

namespace Commons.Music.Midi
{
    public class MidiConnectionStateDetectorExtension
    {
        public event EventHandler<MidiConnectionEventArgs> StateChanged;
    }
}