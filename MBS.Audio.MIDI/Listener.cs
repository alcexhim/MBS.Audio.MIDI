using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBS.Audio.MIDI
{
    public class Listener
    {
        private MBS.Audio.MIDI.InputDevice input = null;
        private MBS.Audio.MIDI.OutputDevice output = null;
        public MBS.Audio.MIDI.OutputDevice OutputDevice { get { return output; } }

        public SoundCard SoundCard { get; set; } = null;

		private static MessageReceivedEventHandler eventHandler = null;

        public void Start()
        {
			if (SoundCard == null)
			{
				SoundCard = SoundCard.GetDefaultSoundCard();
			}
			if (SoundCard != null)
			{
				SoundCard.Open();
				input = SoundCard.GetDefaultMidiInputDevice();
				// output = sc.GetMidiOutputDevices()[1];
			}

			if (input != null)
			{
				input.Open();
				input.MessageReceived += Listener_MessageReceived;
				input.Start();
			}
			if (output != null)
			{
				output.Open();
			}
        }
        public void Stop()
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
			SoundCard.Close();
        }

        public event MessageReceivedEventHandler MessageReceived;
		protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
		{
			MessageReceived?.Invoke(this, e);
		}

		private void Listener_MessageReceived(object sender, MBS.Audio.MIDI.MessageReceivedEventArgs e)
        {
			OnMessageReceived(e);
        }
    }
}
