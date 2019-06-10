/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using ircClient;
using ircCore.Settings.Theming;
using ircCore.Users;
using ircCore.Utils;
using ircScript.Classes.ScriptFunctions;

namespace FusionIRC.Helpers.Commands
{
    public static class CommandMisc
    {
        public static void Notify(ClientConnection client, UserListType type, string args)
        {
            var sp = new List<string>(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
            if (sp.Count < 1)
            {
                return;
            }
            var remove = false;
            string nick;
            switch (sp[0].ToUpper())
            {
                case "-R":
                    remove = true;
                    nick = sp[1];
                    for (var i = 1; i >=0; i--)
                    {
                        sp.RemoveAt(i);
                    }
                    break;

                default:
                    nick = sp[0];
                    sp.RemoveAt(0);
                    break;
            }          
            if (remove)
            {
                if (!UserManager.RemoveNotify(nick))
                {
                    return;
                }
            }
            else
            {
                if (!UserManager.AddNotify(nick, string.Join(" ", sp)))
                {
                    return;
                }
            }
            AddRemoveNotifyIgnore(client, type, nick, remove);
        }

        public static void Ignore(ClientConnection client, UserListType type, string args)
        {
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length < 1)
            {
                return;
            }
            var remove = false;
            string address;
            switch (sp[0].ToUpper())
            {
                case "-R":
                    remove = true;
                    address = Address.CheckIrcAddress(sp[1]);
                    break;

                default:
                    address = Address.CheckIrcAddress(sp[0]);
                    break;
            }   
            if (remove)
            {
                if (!UserManager.RemoveIgnore(address))
                {
                    return;
                }
            }
            else
            {
                if (!UserManager.AddIgnore(address))
                {
                    return;
                }
            }
            AddRemoveNotifyIgnore(client, type, address, remove);
        }

        /* Private helper functions */
        private static void AddRemoveNotifyIgnore(ClientConnection client, UserListType type, string nick, bool remove)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.InfoText,
                              TimeStamp = DateTime.Now,
                              Text = string.Format(remove ? "Removed {0} from {1} list" : "Added {0} to {1} list", nick,
                                                   type == UserListType.Ignore ? "ignore" : "notify")
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
        }
    }
}
