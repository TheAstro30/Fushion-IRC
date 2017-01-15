/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms;
using ircClient;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers
{
    public static class CommandProcessor
    {
        public static void Parse(ClientConnection client, FrmChildWindow child, string data)
        {
            var i = data.IndexOf(' ');
            if (i == -1)
            {
                return;
            }
            var com = data.Substring(0, i).Trim().ToUpper().Replace("/", "");
            var args = data.Substring(i).Trim();
            ParseCommand(client, child, com, args);
        }

        /* Private parsing methods */
        private static void ParseCommand(ClientConnection client, FrmChildWindow child, string command, string args)
        {
            switch (command)
            {
                case "SERVER":
                    ParseServerConnection(client, args);
                    break;

                case "ME":
                case "ACTION":
                case "DESCRIBE":
                    /* Action */
                    ParseAction(client, child, args);
                    break;

                case "NICK":
                    ParseNick(client, args);
                    break;

                default:
                    /* Send command to server */
                    if (!client.IsConnected)
                    {
                        return;
                    }
                    client.Send(string.Format("{0} :{1}", command, args));
                    break;
            }
        }

        private static void ParseServerConnection(ClientConnection client, string args)
        {
            var s = args.Split(' ');
            string[] address = null;
            var port = 6667;
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || s.Length == 0)
            {
                return;
            }
            switch (s.Length)
            {
                case 1:
                    if (s[0].ToLower() == "-m")
                    {
                        return;
                    }
                    address = s[0].Split(':');
                    break;

                default:
                    /* /server -m server[:port]  or /server -m server[:port] -j #chan */
                    if (s[0].ToLower() == "-m")
                    {
                        /* Create new connection */
                        c = WindowManager.AddWindow(null, ChildWindowType.Console, ConnectionCallbackManager.MainForm,
                                                        "Console", "Console", true);
                        if (c == null)
                        {
                            return;
                        }
                        address = s[1].Split(':');
                    }
                    else
                    {
                        address = s[0].Split(':');
                    }
                    c.Client.Parser.JoinChannelsOnConnect = s.Length > 2 && s[1].ToLower() == "-j"
                                                                ? s[2]
                                                                : s.Length > 3 && s[2].ToLower() == "-j"
                                                                      ? s[3]
                                                                      : string.Empty;
                    break;
            }
            if (address.Length == 2)
            {
                if (!int.TryParse(address[1], out port))
                {
                    port = 6667;
                }
            }
            c.Client.Connect(address[0], port);
        }

        private static void ParseAction(ClientConnection client, FrmChildWindow child, string args)
        {
            if (child.WindowType == ChildWindowType.Console)
            {
                return;
            }
            var c = WindowManager.GetWindow(client, child.Tag.ToString());
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelSelfActionText,
                              TimeStamp = DateTime.Now,
                              Nick = client.UserInfo.Nick,
                              Text = args
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
            var action = string.Format("PRIVMSG {0} :{1}ACTION {2}{3}", child.Tag, (char)1, args, (char)1);
            client.Send(action);
        }

        private static void ParseNick(ClientConnection client, string args)
        {
            var i = args.IndexOf(' ');
            var nick = i > -1 ? args.Substring(i).Trim() : args;
            if (client.IsConnecting)
            {
                /* Client is in the middle of connecting - but not fully connected, update credentials */               
                client.UserInfo.Nick = nick;
            }
            client.Send(string.Format("NICK {0}", nick));
        }
    }
}
