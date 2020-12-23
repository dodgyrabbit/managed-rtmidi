namespace Commons.Music.Midi.RtMidi
{
    class RtMidiPortDetails : IMidiPortDetails
    {
        public RtMidiPortDetails (MidiDeviceInfo deviceInfo)
        {
            RawId = deviceInfo.ID;
            Id = deviceInfo.ID.ToString ();
            // okay, it is not really manufacturer
            Manufacturer = deviceInfo.Interface;
            Name = deviceInfo.Name;
            Version = string.Empty;
        }

        public int RawId { get; private set; }

        public string Id { get; private set; }

        public string Manufacturer { get; private set; }

        public string Name { get; private set; }

        public string Version { get; private set; }
    }
}