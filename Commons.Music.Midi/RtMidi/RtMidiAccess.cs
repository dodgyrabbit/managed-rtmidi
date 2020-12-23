using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Commons.Music.Midi.RtMidi
{
	public class RtMidiAccess : IMidiAccess2
	{
		public event EventHandler<MidiConnectionEventArgs> StateChanged;

		public MidiAccessExtensionManager ExtensionManager { get; private set; }

		public RtMidiAccess()
		{
			ExtensionManager = new RtMidiAccessExtensionManager(this);
		}

		public IEnumerable<IMidiPortDetails> Inputs
		{
			get { return MidiDeviceManager.AllDevices.Where(d => d.IsInput).Select(d => new RtMidiPortDetails(d)); }
		}

		public IEnumerable<IMidiPortDetails> Outputs
		{
			get { return MidiDeviceManager.AllDevices.Where(d => d.IsOutput).Select(d => new RtMidiPortDetails(d)); }
		}

		public Task<IMidiInput> OpenInputAsync(string portId)
		{
			var p = new RtMidiInput((RtMidiPortDetails) Inputs.First(i => i.Id == portId));
			return p.OpenAsync().ContinueWith(t => (IMidiInput) p);
		}

		public Task<IMidiOutput> OpenOutputAsync(string portId)
		{
			var p = new RtMidiOutput((RtMidiPortDetails) Outputs.First(i => i.Id == portId));
			return p.OpenAsync().ContinueWith(t => (IMidiOutput) p);
		}

		class RtMidiAccessExtensionManager : MidiAccessExtensionManager
		{
			RtMidiPortCreatorExtension port_creator;

			public RtMidiAccessExtensionManager(RtMidiAccess access)
			{
				Access = access;
				port_creator = new RtMidiPortCreatorExtension(this);
			}

			public RtMidiAccess Access { get; private set; }

			public override T GetInstance<T>()
			{
				if (typeof(T) == typeof(MidiPortCreatorExtension))
				{
					return (T) (object) port_creator;
				}

				return null;
			}
		}

		class RtMidiPortCreatorExtension : MidiPortCreatorExtension
		{
			RtMidiAccessExtensionManager manager;

			public RtMidiPortCreatorExtension(RtMidiAccessExtensionManager extensionManager)
			{
				manager = extensionManager;
			}

			public override IMidiOutput CreateVirtualInputSender(PortCreatorContext context)
			{
				RtMidiOutputDevice output =
					new RtMidiOutputDevice(RtMidiApi.Unspecified, context.ApplicationName ?? "managed-rtmidi");
				output.OpenVirtualPort(context.PortName ?? "managed-rtmidi virtual input");

				// TODO: Review if output.PortCount is in fact the correct object
				var details = new RtMidiPortDetails(MidiDeviceManager.GetDeviceInfo(output.PortCount));

				return new SimpleVirtualMidiOutput(details, () => output.Dispose())
				{
					OnSend = (buffer, index, length, timestamp) => output.SendMessage(buffer, length)
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
