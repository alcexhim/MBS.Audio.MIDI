using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBS.Audio.MIDI
{
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode()]
#endif
	public abstract class Device
	{
		protected uint mvarID = 0;
		public uint ID { get { return mvarID; } }

		protected IntPtr mvarHandle = IntPtr.Zero;
		public IntPtr Handle { get { return mvarHandle; } }

		public abstract void Open();
		public abstract void Refresh();
		public abstract void Reset();
		public abstract void Close();

		protected string mvarName = String.Empty;
		public string Name { get { return mvarName; } }

		protected uint mvarManufacturerID = 0;
		public uint ManufacturerID { get { return mvarManufacturerID; } }
		protected uint mvarProductID = 0;
		public uint ProductID { get { return mvarProductID; } }
		protected ushort mvarMaximumVoices = 0;
		public ushort MaximumVoices { get { return mvarMaximumVoices; } }
		protected ushort mvarMaximumNotes = 0;
		public ushort MaximumNotes { get { return mvarMaximumNotes; } }
		protected ushort mvarSupportedChannels = 0;
		public ushort SupportedChannels { get { return mvarSupportedChannels; } }
		protected Version mvarDriverVersion = new Version(1, 0);
		public Version DriverVersion { get { return mvarDriverVersion; } }
		protected DeviceType mvarDeviceType = DeviceType.None;
		public DeviceType DeviceType { get { return mvarDeviceType; } }
		protected DeviceOptionalFunctionality mvarSupportedFunctionality = DeviceOptionalFunctionality.None;
		public DeviceOptionalFunctionality SupportedFunctionality { get { return mvarSupportedFunctionality; } }
	}
}
