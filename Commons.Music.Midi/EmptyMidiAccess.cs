using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Commons.Music.Midi
{
    class EmptyMidiAccess : IMidiAccess2
    {
        public IEnumerable<IMidiPortDetails> Inputs
        {
            get { yield return EmptyMidiInput.Instance.Details; }
        }
		
        public IEnumerable<IMidiPortDetails> Outputs
        {
            get { yield return EmptyMidiOutput.Instance.Details; }
        }
		
        public Task<IMidiInput> OpenInputAsync (string portId)
        {
            if (portId != EmptyMidiInput.Instance.Details.Id)
                throw new ArgumentException (string.Format ("Port ID {0} does not exist.", portId));
            return Task.FromResult<IMidiInput> (EmptyMidiInput.Instance);
        }
		
        public Task<IMidiOutput> OpenOutputAsync (string portId)
        {
            if (portId != EmptyMidiOutput.Instance.Details.Id)
                throw new ArgumentException (string.Format ("Port ID {0} does not exist.", portId));
            return Task.FromResult<IMidiOutput> (EmptyMidiOutput.Instance);
        }

#pragma warning disable 0067
        // it will never be fired.
        public event EventHandler<MidiConnectionEventArgs> StateChanged;
        public MidiAccessExtensionManager ExtensionManager { get; }
#pragma warning restore 0067
    }
}