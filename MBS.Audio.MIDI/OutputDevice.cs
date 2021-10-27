using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBS.Audio.MIDI
{
#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode()]
#endif
	public class OutputDevice : Device
	{
        private SoundCard mvarParent = null;
        public SoundCard Parent { get { return mvarParent; } }

		internal OutputDevice(uint id, SoundCard parent)
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
                    mvarHandle = IntPtr.Zero;
                    IntPtr dummy = IntPtr.Zero;
                    string str = "hw:" + mvarParent.ID.ToString() + "," + mvarID.ToString() + ",0";
                    int error = Internal.Linux.Alsa.Methods.snd_rawmidi_open(ref dummy, ref mvarHandle, str, 0);
                    Internal.Linux.Alsa.Methods.snd_error_code_to_exception(error);

                    Refresh();
                    return;
				}
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				{
					mvarHandle = IntPtr.Zero;

					Internal.Windows.Constants.MidiError error = Internal.Windows.Methods.midiOutOpen(out mvarHandle, mvarID, new Internal.Windows.Delegates.MidiCallback(MidiCallback), IntPtr.Zero, Internal.Windows.Constants.MidiOpenFlags.Function);
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
		/// Updates the MIDI device information.
		/// </summary>
		public override void Refresh()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
				{
                    if (mvarHandle == IntPtr.Zero) return;

                    IntPtr hInfo = IntPtr.Zero;
                    Internal.Linux.Alsa.Methods.snd_rawmidi_info_malloc(ref hInfo);
                    Internal.Linux.Alsa.Methods.snd_rawmidi_info_set_device(hInfo, mvarID);
                    Internal.Linux.Alsa.Methods.snd_rawmidi_info_set_stream(hInfo, Internal.Linux.Alsa.Constants.snd_rawmidi_stream.Output);
                    Internal.Linux.Alsa.Methods.snd_rawmidi_info_set_subdevice(hInfo, -1);

                    Internal.Linux.Alsa.Methods.snd_rawmidi_info(mvarHandle, hInfo);
                    // mvarName = Internal.Linux.Alsa.Methods.snd_rawmidi_info_get_name(ref hInfo);
                    Internal.Linux.Alsa.Methods.snd_rawmidi_info_free(hInfo);
					return;
				}
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				{
					Internal.Windows.Structures.MIDIOUTCAPS caps = new Internal.Windows.Structures.MIDIOUTCAPS();
					Internal.Windows.Methods.midiOutGetDevCaps(mvarID, out caps, (uint)System.Runtime.InteropServices.Marshal.SizeOf(caps));

					mvarManufacturerID = caps.wMid;
					mvarProductID = caps.wPid;
					mvarName = caps.szPname;
					mvarDeviceType = caps.wTechnology;
					mvarMaximumVoices = caps.wVoices;
					mvarMaximumNotes = caps.wNotes;
					mvarDriverVersion = new Version(caps.vDriverVersionMajor, caps.vDriverVersionMinor);
					mvarSupportedChannels = caps.wChannelMask;
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
		/// Turns off all notes on all MIDI channels for this MIDI output device.
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
					Internal.Windows.Constants.MidiError error = Internal.Windows.Methods.midiOutReset(mvarHandle);
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

        private bool mvarAutoFlush = true;
        public bool AutoFlush { get { return mvarAutoFlush; } set { mvarAutoFlush = true; } }

        private System.IO.MemoryStream stream = new System.IO.MemoryStream();
        public int Write(byte[] buffer)
        {
            return Write(buffer, 0, buffer.Length);
        }
        public int Write(byte[] buffer, int start, int length)
        {
            stream.Write(buffer, start, length);
            if (mvarAutoFlush) return Flush();
            return length;
        }
        public int Flush()
        {
            // get the bytes currently in the stream
            byte[] buffer = stream.ToArray();

            // clear out the memory stream
            stream = new System.IO.MemoryStream();

            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                {
                    int error = Internal.Linux.Alsa.Methods.snd_rawmidi_write(mvarHandle, buffer, buffer.Length);
                    Internal.Linux.Alsa.Methods.snd_error_code_to_exception(error);
                    return error;
                }
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                {
                    System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.MemoryStream(buffer));
					while (br.BaseStream.Position < br.BaseStream.Length)
                    {
                        byte msgtype = br.ReadByte();
                        byte param1 = 0;
                        byte param2 = 0;
						if (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            param1 = br.ReadByte();
							if (br.BaseStream.Position < br.BaseStream.Length)
                            {
                                param2 = br.ReadByte();
                            }
                        }

                        int dwMsg = BitConverter.ToInt32(new byte[] { msgtype, param1, param2, 0 }, 0);

                        Internal.Windows.Constants.MidiError error = Internal.Windows.Methods.midiOutShortMsg((uint) mvarHandle.ToInt32(), dwMsg);
                        Internal.Windows.Methods.midiErrorToException(error);
                    }
                    br.Close();
                    buffer = new byte[0];
                    return 0;
                }
                case PlatformID.Xbox:
                {
                    break;
                }
            }
            throw new PlatformNotSupportedException();
        }

		/// <summary>
		/// Closes this MIDI output device.
		/// </summary>
		public override void Close()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
				{
                    int error = Internal.Linux.Alsa.Methods.snd_rawmidi_close(mvarHandle);
                    Internal.Linux.Alsa.Methods.snd_error_code_to_exception(error);

                    mvarHandle = IntPtr.Zero;
                    return;
				}
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				{
                    try
                    {
                        Internal.Windows.Methods.midiOutClose(mvarHandle);
                    }
                    catch
                    {
                    }
					mvarHandle = IntPtr.Zero;
					return;
				}
				case PlatformID.Xbox:
				{
					break;
				}
			}
			throw new InvalidOperationException();
		}

		public event MessageReceivedEventHandler MessageReceived;
		protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
		{
			if (MessageReceived != null)
			{
				MessageReceived(this, e);
			}
		}

		private void MidiCallback(IntPtr hmo, uint wMsg, IntPtr dwInstance, uint dwMidiMessage, uint dwTimestamp)
		{
			// OnMessageReceived(new MessageReceivedEventArgs(hmo, wMsg, dwInstance, dwParam1, dwParam2));
		}

        public void Send(params Message[] messages)
        {
            foreach (Message message in messages)
            {
                byte status = (byte)((byte)message.MessageType | (message.Channel - 1));

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                byte[] payload = new byte[] { status, message.Parameter1, message.Parameter2 }; // 93, 90 };
                bw.Write(payload);
                bw.Close();

                Write(ms.ToArray());
            }
        }
    }
}
