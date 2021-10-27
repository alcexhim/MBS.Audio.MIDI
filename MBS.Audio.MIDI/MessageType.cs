using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBS.Audio.MIDI
{
	public enum MessageType : byte
	{
		ControlChange = 0xB0,
		Note = 0x90
	}
}
