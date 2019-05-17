/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms.Child;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandChannel
    {
        public static void ParsePart(ClientConnection client, FrmChildWindow child, string args)
        {
            if (!client.IsConnected)
            {
                return;
            }
            string channel;
            if (String.IsNullOrEmpty(args))
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
            client.Send(String.Format("PART {0}", channel));
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
            client.Send(String.Format("PART {0}\r\nJOIN {0}", child.Tag));
        }

        public static void ParseNames(ClientConnection client, string args)
        {
            if (!client.IsConnected || String.IsNullOrEmpty(args))
            {
                return;
            }
            var i = args.IndexOf(' ');
            var channel = i == -1 ? args : args.Substring(0, i).Trim();
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            c.Nicklist.Clear();
            client.Send(String.Format("NAMES {0}\r\nWHO {0}", channel));
        }

        public static void ParseTopic(ClientConnection client, string args)
        {
            if (String.IsNullOrEmpty(args) || !client.IsConnected)
            {
                return;
            }
            var i = args.IndexOf(' ');
            if (i == -1)
            {
                /* Most likely /topic #chan */
                client.Send(String.Format("TOPIC {0}", args));
                return;
            }
            var channel = args.Substring(0, i).Trim();
            args = args.Substring(i).Trim();
            client.Send(String.Format("TOPIC {0} :{1}", channel, args));
        }
    }
}
