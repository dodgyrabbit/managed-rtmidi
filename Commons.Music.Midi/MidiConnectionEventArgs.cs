using System;

namespace Commons.Music.Midi
{
    public class MidiConnectionEventArgs : EventArgs
    {
        public IMidiPortDetails Port { get; private set; }
    }
}