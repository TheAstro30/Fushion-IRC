/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using FusionIRC.Forms.Child;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandPartHop
    {
        public static void ParsePart(ClientConnection client, FrmChildWindow child, string args)
        {
            if (!client.IsConnected)
            {
                return;
            }
            string channel;
            if (string.IsNullOrEmpty(args))
            {
                if (child.WindowType != ChildWindowType.Channel)
                {
                    /* Cannot part a console, query or DCC chat! */
                    return;
                }
                /* Part the active window */
                channel = child.Tag.ToString();
            }
            else
            {
                var c = args.Split(' ');
                channel = c[0];
            }
            client.Send(string.Format("PART {0}", channel));
        }

        public static void ParseHop(ClientConnection client, FrmChildWindow child)
        {
            /* Cannot hop a non-channel (or on a connection that isn't even connected...) */
            if (child.WindowType != ChildWindowType.Channel || !client.IsConnected)
            {
                return;
            }
            if (SettingsManager.Settings.Client.Channels.KeepChannelsOpen)
            {
                child.KeepOpen = true;
            }
            else
            {
                child.AutoClose = true; /* This will stop the child window sending "PART" on closing */
            }
            client.Send(string.Format("PART {0}\r\nJOIN {0}", child.Tag));
        }
    }
}
