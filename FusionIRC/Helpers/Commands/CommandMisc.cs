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
            if (sp.Count == 0)
            {
                return;
            }
            var remove = false;
            var flush = false;
            var nick = string.Empty;
            switch (sp[0].ToUpper())
            {
                case "-R":
                    remove = true;
                    if (sp.Count > 1)
                    {
                        nick = sp[1];
                        for (var i = 1; i >= 0; i--)
                        {
                            sp.RemoveAt(i);
                        }
                    }
                    break;

                default:
                    nick = sp[0];
                    sp.RemoveAt(0);
                    break;
            }          
            if (remove)
            {
                if (string.IsNullOrEmpty(nick))
                {
                    UserManager.ClearNotify();
                    flush = true;
                }
                else if (!UserManager.RemoveNotify(nick))
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
            AddRemoveNotifyIgnore(client, type, nick, remove, flush);
        }

        public static void Ignore(ClientConnection client, UserListType type, string args)
        {
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length == 0)
            {
                return;
            }
            var remove = false;
            var flush = false;
            string address;
            switch (sp[0].ToUpper())
            {
                case "-R":
                    remove = true;
                    address = sp.Length > 1 ? Address.CheckIrcAddress(sp[1]) : string.Empty;
                    break;

                default:
                    address = Address.CheckIrcAddress(sp[0]);
                    break;
            }   
            if (remove)
            {
                if (string.IsNullOrEmpty(address))
                {
                    UserManager.ClearIgnore();
                    flush = true;
                }
                else if (!UserManager.RemoveIgnore(address))
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
            AddRemoveNotifyIgnore(client, type, address, remove, flush);
        }

        /* Private helper functions */
        private static void AddRemoveNotifyIgnore(ClientConnection client, UserListType type, string nick, bool remove, bool flush)
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
                              Text =
                                  !flush
                                      ? string.Format(remove ? "Removed {0} from {1} list" : "Added {0} to {1} list",
                                                      nick,
                                                      type == UserListType.Ignore ? "ignore" : "notify")
                                      : string.Format("{0} list flushed",
                                                      type == UserListType.Ignore ? "Ignore" : "Notify")
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
        }
    }
}
