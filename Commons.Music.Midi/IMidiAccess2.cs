using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Commons.Music.Midi
{
    public interface IMidiAccess2 
    {
        IEnumerable<IMidiPortDetails> Inputs { get; }
        IEnumerable<IMidiPortDetails> Outputs { get; }

        Task<IMidiInput> OpenInputAsync (string portId);
        Task<IMidiOutput> OpenOutputAsync (string portId);
        [Obsolete ("This will be removed in the next API-breaking change. It is not functional at this state anyways.")]
        event EventHandler<MidiConnectionEventArgs> StateChanged;
		
        MidiAccessExtensionManager ExtensionManager { get; }
    }
}