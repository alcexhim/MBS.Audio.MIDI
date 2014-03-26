using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoMidi
{
    public class Listener
    {
        private static MonoMidi.InputDevice input = null;
        private static MonoMidi.OutputDevice output = null;
        public static MonoMidi.OutputDevice OutputDevice { get { return output; } }

        private static MonoMidi.SoundCard sc = null;
        private static MessageReceivedEventHandler eventHandler = null;

        public static void Start()
        {
            sc = MonoMidi.SoundCard.GetDefaultSoundCard();
            sc.Open();
            input = sc.GetDefaultMidiInputDevice();
            // output = sc.GetMidiOutputDevices()[1];

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
        private static void Listener_MessageReceived(object sender, MonoMidi.MessageReceivedEventArgs e)
        {
            if (MessageReceived != null) MessageReceived(sender, e);
        }
    }
}
