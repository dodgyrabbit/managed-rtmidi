using System;

namespace Commons.Music.Midi.RtMidi
{
    /// <summary>
    /// A data class that provides some convenient access to details of a device, such as it's name, port etc.
    /// </summary>
    public class MidiDeviceInfo
    {
        readonly RtMidiDevice manager;
        readonly int id;
        readonly int port;
        readonly bool is_input;

        internal MidiDeviceInfo (RtMidiDevice manager, int id, int port, bool isInput)
        {
            this.manager = manager;
            this.id = id;
            this.port = port;
            is_input = isInput;
        }

        public int ID => id;

        public int Port => port;

        public string Interface => manager.CurrentApi.ToString ();

        public string Name => manager.GetPortName (port);

        public bool IsInput => is_input;

        public bool IsOutput => !is_input;

        public override string ToString ()
        {
            return String.Format ("{0} - {1} ({2})", Interface, Name, IsInput ? (IsOutput ? "I/O" : "Input") : (IsOutput ? "Output" : "N/A"));
        }
    }
}