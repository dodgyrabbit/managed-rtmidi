namespace midi_filter
{
    public enum MidiEventType : byte
    {
        // Channel messages (8n-En)
        NoteOff = 0x80,
        NoteOn = 0x90,
        PAf = 0xA0,
        CC = 0xB0,
        Program = 0xC0,
        CAf = 0xD0,
        Pitch = 0xE0,
        
        // System Messages (F0-FF)
        SysEx1 = 0xF0,
        MtcQuarterFrame = 0xF1,
        SongPositionPointer = 0xF2,
        SongSelect = 0xF3,
        TuneRequest = 0xF6,
        SysEx2 = 0xF7,
        MidiClock = 0xF8,
        MidiTick = 0xF9,
        MidiStart = 0xFA,
        MidiContinue = 0xFB,
        MidiStop = 0xFC,
        ActiveSense = 0xFE,
        Reset = 0xFF,
        EndSysEx = 0xF7,
        Meta = 0xFF
    }
}