using System;

namespace Commons.Music.Midi.RtMidi
{
    // Fix from the original library: Changed size from long to UIntPtr (to see if we can get a platform specific declaration).
    // On RPI, the size is 32 bit, making it a long will likely fail here.
    public unsafe delegate void RtMidiCCallback (double timestamp, byte* message, UIntPtr size, IntPtr userData);
}