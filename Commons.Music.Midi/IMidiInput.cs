using System;

namespace Commons.Music.Midi
{
    public interface IMidiInput : IMidiPort, IDisposable
    {
        event EventHandler<MidiReceivedEventArgs> MessageReceived;
    }
}