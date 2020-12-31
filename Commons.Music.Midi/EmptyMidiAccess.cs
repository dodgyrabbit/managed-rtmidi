using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Commons.Music.Midi
{
    public class EmptyMidiAccess : IMidiAccess2
    {
	    public EmptyMidiAccess()
	    {
		    ExtensionManager = new EmptyMidiAccessExtensionManager(this);
	    }
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
	        {
		        throw new ArgumentException(string.Format("Port ID {0} does not exist.", portId));
	        }

	        return Task.FromResult<IMidiInput> (EmptyMidiInput.Instance);
        }
		
        public Task<IMidiOutput> OpenOutputAsync (string portId)
        {
	        if (portId != EmptyMidiOutput.Instance.Details.Id)
	        {
		        throw new ArgumentException(string.Format("Port ID {0} does not exist.", portId));
	        }

	        return Task.FromResult<IMidiOutput> (EmptyMidiOutput.Instance);
        }

        public event EventHandler<MidiConnectionEventArgs> StateChanged;
        
        public MidiAccessExtensionManager ExtensionManager { get; private set; }
        
		class EmptyMidiAccessExtensionManager : MidiAccessExtensionManager
		{
			EmptyMidiPortCreatorExtension port_creator;

			public EmptyMidiAccessExtensionManager(EmptyMidiAccess access)
			{
				Access = access;
				port_creator = new EmptyMidiPortCreatorExtension(this);
			}

			public EmptyMidiAccess Access { get; private set; }

			public override T GetInstance<T>()
			{
				if (typeof(T) == typeof(MidiPortCreatorExtension))
				{
					return (T) (object) port_creator;
				}

				return null;
			}
		}

		class EmptyMidiPortCreatorExtension : MidiPortCreatorExtension
		{
			EmptyMidiAccessExtensionManager manager;

			public EmptyMidiPortCreatorExtension(EmptyMidiAccessExtensionManager extensionManager)
			{
				manager = extensionManager;
			}

			public override IMidiOutput CreateVirtualInputSender(PortCreatorContext context)
			{
				EmptyMidiOutput output = new EmptyMidiOutput();
				var details = new EmptyMidiPortDetails("id", context.PortName);
				return new SimpleVirtualMidiOutput(details, () => output.Dispose())
				{
					OnSend = (buffer, index, length, timestamp) =>
					{
						output.Send(buffer, index, length, timestamp);
					}
				};
			}

			public override IMidiInput CreateVirtualOutputReceiver(PortCreatorContext context)
			{
				throw new NotImplementedException();
				// var seq = new AlsaSequencer (AlsaIOType.Duplex, AlsaIOMode.NonBlocking);
				// var portNumber = seq.CreateSimplePort (
				// 	context.PortName ?? "managed-midi virtual out",
				// 	AlsaMidiAccess.virtual_output_connected_cap,
				// 	AlsaMidiAccess.midi_port_type);
				// seq.SetClientName (context.ApplicationName ?? "managed-midi output port creator");
				// var port = seq.GetPort (seq.CurrentClientId, portNumber);
				// var details = new AlsaMidiPortDetails (port);
				// return new SimpleVirtualMidiInput (details, () => seq.DeleteSimplePort (portNumber));
			}
		}

    }
}