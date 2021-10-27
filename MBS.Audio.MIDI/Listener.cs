using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBS.Audio.MIDI
{
    public class Listener
    {
        private static MBS.Audio.MIDI.InputDevice input = null;
        private static MBS.Audio.MIDI.OutputDevice output = null;
        public static MBS.Audio.MIDI.OutputDevice OutputDevice { get { return output; } }

        private static MBS.Audio.MIDI.SoundCard sc = null;
        private static MessageReceivedEventHandler eventHandler = null;

        public static void Start()
        {
            sc = MBS.Audio.MIDI.SoundCard.GetDefaultSoundCard();
			if (sc != null)
			{
				sc.Open();
				input = sc.GetDefaultMidiInputDevice();
				// output = sc.GetMidiOutputDevices()[1];
			}
			eventHandler = new MessageReceivedEventHandler(Listener_MessageReceived);
			if (input != null)
			{
				input.Open();
				input.MessageReceived += eventHandler;
				input.Start();
			}
			if (output != null)
			{
				output.Open();
			}
        }
        public static void Stop()
        {
            if (input != null)
            {
                input.Stop();
                input.Close();
            }
            if (output != null)
            {
                output.Close();
            }
            sc.Close();
        }

        public static event MessageReceivedEventHandler MessageReceived;
        private static void Listener_MessageReceived(object sender, MBS.Audio.MIDI.MessageReceivedEventArgs e)
        {
            if (MessageReceived != null) MessageReceived(sender, e);
        }
    }
}
