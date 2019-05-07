/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using ircCore.Utils.DirectX;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandSoundPlay
    {
        private static readonly Sound Sound;

        static CommandSoundPlay()
        {
            Sound = new Sound {Volume = 100};
        }

        public static double Position
        {
            get { return Sound.Position; }
            set { Sound.Position = value; }
        }

        public static double Length
        {
            get { return Sound.Duration; }
        }

        public static void Parse(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            string com;
            string file;
            var i = args.IndexOf(' ');
            if (i > -1)
            {
                /* Look at first word to see if it's a command */
                com = args.Substring(0, i);
                file = args.Substring(i + 1);
            }
            else
            {
                com = args;
                file = com;
            }
            switch (com.ToUpper())
            {
                case "STOP":
                    Sound.Stop();
                    return;

                case "PAUSE":
                    Sound.Pause();
                    return;

                case "RESUME":
                    Sound.Resume();
                    return;
            }
            Sound.Clip = file;
            Sound.PlayAsync();        
        }
    }
}
