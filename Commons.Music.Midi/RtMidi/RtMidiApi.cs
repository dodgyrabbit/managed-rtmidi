namespace Commons.Music.Midi.RtMidi
{
    public enum RtMidiApi
    {
        Unspecified,
        MacOsxCore,
        LinuxAlsa,
        UnixJack,
        WindowsMultimediaMidi,
        WindowsKernelStreaming,
        RtMidiDummy,
    }
}