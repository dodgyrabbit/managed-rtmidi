using Commons.Music.Midi.RtMidi;

namespace Commons.Music.Midi
{
	public class MidiAccessManager
	{
		static MidiAccessManager ()
		{
			Default = Empty = new EmptyMidiAccess ();
			new MidiAccessManager().InitializeDefault();
		}
		
		public static IMidiAccess2 Default { get; private set; }
		public static IMidiAccess2 Empty { get; internal set; }

		void InitializeDefault()
		{
			Default = new RtMidiAccess();
		}
	}
}
