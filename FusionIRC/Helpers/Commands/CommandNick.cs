/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircClient;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandNick
    {
        public static void ParseNick(ClientConnection client, string args)
        {
            var i = args.IndexOf(' ');
            var nick = i > -1 ? args.Substring(i).Trim() : args;
            if (nick.Equals(client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            if (client.IsConnecting && !client.IsConnected)
            {
                /* Client is in the middle of connecting - but not fully connected, update credentials */
                client.UserInfo.Nick = nick;
            }
            client.Send(string.Format("NICK {0}", nick));
            if (client.IsConnected || client.IsConnecting)
            {
                return;
            }
            /* Offline nick change ... if the newnick == alternative nick, we switch them back */
            if (nick.Equals(client.UserInfo.Alternative, StringComparison.InvariantCultureIgnoreCase))
            {
                /* Otherwise both alternative and nick will be the same defeating having an alternative to begin with */
                client.UserInfo.Alternative = client.UserInfo.Nick;
            }
            client.UserInfo.Nick = nick;
            client.UserInfo.AlternateUsed = false;
            var console = WindowManager.GetConsoleWindow(client);
            if (console == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.NickChangeSelfText,
                              TimeStamp = DateTime.Now,
                              NewNick = nick
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            console.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(console, WindowManager.MainForm, WindowEvent.EventReceived);
        }
    }
}
