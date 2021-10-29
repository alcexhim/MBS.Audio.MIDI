using System;
using System.Collections.Generic;

namespace MBS.Audio.MIDI
{
	public class InputDevice : Device
	{
        private SoundCard mvarParent = null;
        public SoundCard Parent { get { return mvarParent; } }

		internal InputDevice(uint id, SoundCard parent)
		{
			mvarID = id;
            mvarParent = parent;
			try
			{
				Refresh();
			}
			catch
			{
			}
		}

		public override void Open()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
				{
                    IntPtr dummy = IntPtr.Zero;
                    mvarHandle = IntPtr.Zero;
                    Internal.Linux.Alsa.Methods.snd_rawmidi_open(ref mvarHandle, ref dummy, "hw:" + mvarParent.ID.ToString() + "," + ID.ToString() + ",0", 0);

                    byHandle.Add(mvarHandle, this);
					return;
				}
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				{
					mvarHandle = IntPtr.Zero;

                    mvarCallback = new Internal.Windows.Delegates.MidiCallback(MidiCallback);
					Internal.Windows.Constants.MidiError error = Internal.Windows.Methods.midiInOpen(out mvarHandle, mvarID, mvarCallback, IntPtr.Zero, Internal.Windows.Constants.MidiOpenFlags.Function);
					Internal.Windows.Methods.midiErrorToException(error);

                    byHandle.Add(mvarHandle, this);
					return;
				}
				case PlatformID.Xbox:
				{
					break;
				}
			}
			throw new PlatformNotSupportedException();
		}

        private Internal.Windows.Delegates.MidiCallback mvarCallback = null;

		private System.Threading.Thread tLinuxThread = null;
		private void tLinuxThread_ThreadStart()
		{
			while (true)
			{
				byte[] buffer = new byte[16];
				Internal.Linux.Alsa.Methods.snd_rawmidi_read(mvarHandle, buffer, buffer.Length);

				byte channelId = (byte)(((byte)(buffer[0] << 4)) >> 4);
				byte messageTypeId = (byte)((byte)(buffer[0] >> 4) << 4);
				OnMessageReceived(new MessageReceivedEventArgs(new Message((MessageType)messageTypeId, (byte)(channelId + 1), buffer[1], buffer[2]), IntPtr.Zero));
					
				System.Threading.Thread.Sleep(10);
			}
		}

		/// <summary>
		/// Starts listening for MIDI input.
		/// </summary>
		public void Start()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                {
					tLinuxThread = new System.Threading.Thread(tLinuxThread_ThreadStart);
					tLinuxThread.Start();
                    break;
                }
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                {
                    Internal.Windows.Constants.MidiError error = Internal.Windows.Methods.midiInStart(mvarHandle);
                    Internal.Windows.Methods.midiErrorToException(error);
                    return;
                }
            }
        }

        /// <summary>
        /// Stops listening for MIDI input.
        /// </summary>
        public void Stop()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                {
                    break;
                }
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                {
                    Internal.Windows.Constants.MidiError error = Internal.Windows.Methods.midiInStop(mvarHandle);
                    Internal.Windows.Methods.midiErrorToException(error);
                    return;
                }
            }
        }

		/// <summary>
		/// Updates the MIDI device information.
		/// </summary>
		public override void Refresh()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
				{
					break;
				}
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				{
					Internal.Windows.Structures.MIDIINCAPS caps = new Internal.Windows.Structures.MIDIINCAPS();
					Internal.Windows.Methods.midiInGetDevCaps(mvarID, out caps, (uint)System.Runtime.InteropServices.Marshal.SizeOf(caps));

                    mvarManufacturerID = caps.wMid;
                    mvarProductID = caps.wPid;
                    mvarName = caps.szPname;
                    mvarDriverVersion = new Version(caps.vDriverVersionMajor, caps.vDriverVersionMinor);
                    mvarSupportedFunctionality = caps.dwSupport;
					return;
				}
				case PlatformID.Xbox:
				{
					break;
				}
			}
			throw new InvalidOperationException();
		}

		/// <summary>
		/// Stops input on this MIDI input device.
		/// </summary>
		public override void Reset()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
				{
					break;
				}
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				{
					Internal.Windows.Constants.MidiError error = Internal.Windows.Methods.midiInReset(mvarHandle);
					Internal.Windows.Methods.midiErrorToException(error);
					return;
				}
				case PlatformID.Xbox:
				{
					break;
				}
			}
			throw new PlatformNotSupportedException();
		}

		/// <summary>
		/// Closes this MIDI input device.
		/// </summary>
		public override void Close()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
				{
                    Internal.Linux.Alsa.Methods.snd_rawmidi_close(mvarHandle);
                    byHandle.Remove(mvarHandle);

                    mvarHandle = IntPtr.Zero;
					return;
				}
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				{
                    Internal.Windows.Methods.midiInClose(mvarHandle);
                    byHandle.Remove(mvarHandle);

					mvarHandle = IntPtr.Zero;
					return;
				}
				case PlatformID.Xbox:
				{
					break;
				}
			}
			throw new PlatformNotSupportedException();
		}

		public event MessageReceivedEventHandler MessageReceived;
		protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
		{
			if (MessageReceived != null) MessageReceived(this, e);
		}

        private static Dictionary<IntPtr, InputDevice> byHandle = new Dictionary<IntPtr, InputDevice>();

		private static void MidiCallback(IntPtr hmo, uint wMsg, IntPtr dwInstance, uint dwMidiMessage, uint dwTimestamp)
		{
            InputDevice device = null;
            if (byHandle.ContainsKey(hmo)) device = byHandle[hmo];
            if (device == null) return;

            byte[] msgdata = BitConverter.GetBytes(dwMidiMessage);
            MessageType type = (MessageType)(msgdata[0] & 0xF0);
            byte channel = (byte)((msgdata[0] & 0x0F) + 1);

            byte parameter1 = (byte)msgdata[1];
            byte parameter2 = (byte)msgdata[2];

			device.OnMessageReceived(new MessageReceivedEventArgs(new Message(type, channel, parameter1, parameter2), dwInstance));
		}
	}
}
