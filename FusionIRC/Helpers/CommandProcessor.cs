/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms.Child;
using FusionIRC.Helpers.Commands;
using ircClient;
using ircClient.Parsing.Helpers;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Helpers
{
    public static class CommandProcessor
    {
        /* Main parsing entry point */
        public static void Parse(ClientConnection client, FrmChildWindow child, string data)
        {
            var i = data.IndexOf(' ');
            string com;
            var args = string.Empty;
            if (i != -1)
            {
                com = data.Substring(0, i).Trim().ToUpper().Replace("/", "");
                args = data.Substring(i).Trim();
            }
            else
            {
                com = data.ToUpper().Replace("/", "");
            }
            ParseCommand(client, child, com, args);
        }

        public static void ParseCommand(ClientConnection client, FrmChildWindow child, string command, string args)
        {
            /* First check it's not an alias */
            if (CommandAlias.ParseAlias(client, child, command, args))
            {
                return; /* Process no further */
            }
            switch (command)
            {
                case "CLR":
                case "CLEAR":
                    /* Clear child output window of text */
                    child.Output.Clear();
                    break;

                case "SERVER":
                    CommandServer.ParseServerConnection(client, args);
                    break;

                case "DISCONNECT":
                    CommandServer.ParseServerDisconnection(client);
                    break;

                case "ME":
                case "ACTION":
                case "DESCRIBE":
                    /* Action */
                    CommandText.ParseAction(client, child, args);
                    break;

                case "AME":
                    CommandText.ParseAme(client, args);
                    break;

                case "AMSG":
                    CommandText.ParseAmsg(client, args);
                    break;

                case "MSG":
                    CommandText.ParseMsg(client, child, args);
                    break;

                case "SAY":
                    CommandText.ParseSay(client, child, args);
                    break;

                case "NOTICE":
                    CommandText.ParseNotice(client, child, args);
                    break;

                case "PART":
                    CommandPartHop.ParsePart(client, child, args);
                    break;

                case "HOP":
                    CommandPartHop.ParseHop(client, child);
                    break;

                case "NICK":
                    ParseNick(client, args);
                    break;

                case "NAMES":
                    ParseNames(client, args);
                    break;

                case "TOPIC":
                    ParseTopic(client, args);
                    break;

                case "WHOIS":
                    if (!client.IsConnected || string.IsNullOrEmpty(args))
                    {
                        return;
                    }
                    client.Parser.Whois = new WhoisInfo();
                    var n = args.Split(' ');
                    if (n.Length == 0)
                    {
                        return;
                    }
                    client.Send(string.Format("WHOIS {0}", n[0]));
                    break;

                case "CTCP":
                    ParseCtcp(client, args);
                    break;

                case "DNS":
                    ParseDns(client, args);
                    break;

                case "PASS":
                    client.Send(string.Format("PASS :{0}", args));
                    break;

                case "ECHO":
                    CommandText.ParseEcho(client, args);
                    break;

                case "SPLAY":
                    CommandSoundPlay.Parse(args);
                    break;

                case "WRITEINI":
                    CommandFiles.WriteIni(args);
                    break;

                default:
                    /* Send command to server */
                    if (!client.IsConnected)
                    {
                        return;
                    }
                    client.Send(string.Format("{0} {1}", command, args));
                    break;
            }
        }

        /* CTCP */
        private static void ParseCtcp(ClientConnection client, string args)
        {
            if (!client.IsConnected || string.IsNullOrEmpty(args))
            {
                return;
            }
            var ctcp = args.Split(' ');
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || ctcp.Length < 2)
            {
                return;
            }
            var ct = ctcp[1].ToUpper();
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.CtcpSelfText,
                              TimeStamp = DateTime.Now,
                              Target = ct,
                              Nick = ctcp[0],
                              Text = args
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, ConnectionCallbackManager.MainForm, WindowEvent.MessageReceived);
            client.Send(ct == "PING"
                            ? string.Format("PRIVMSG {0} :{1}{2} {3}{4}", ctcp[0], (char)1, ct, TimeFunctions.CTime(), (char)1)
                            : string.Format("PRIVMSG {0} :{1}{2}{3}", ctcp[0], (char)1, ct, (char)1));
        }

        private static void ParseDns(ClientConnection client, string args)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.DnsText,
                              TimeStamp = DateTime.Now,
                              Text = args
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, ConnectionCallbackManager.MainForm, WindowEvent.EventReceived);
            /* Now, get the DNS information */
            client.ResolveDns(args);
        }

        private static void ParseNames(ClientConnection client, string args)
        {
            if (!client.IsConnected || string.IsNullOrEmpty(args))
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
            client.Send(string.Format("NAMES {0}\r\nWHO {0}", channel));
        }

        private static void ParseTopic(ClientConnection client, string args)
        {
            if (string.IsNullOrEmpty(args) || !client.IsConnected)
            {
                return;
            }
            var i = args.IndexOf(' ');
            if (i == -1)
            {
                /* Most likely /topic #chan */
                client.Send(string.Format("TOPIC {0}", args));
                return;
            }
            var channel = args.Substring(0, i).Trim();
            args = args.Substring(i).Trim();
            client.Send(string.Format("TOPIC {0} :{1}", channel, args));
        }

        private static void ParseNick(ClientConnection client, string args)
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
            WindowManager.SetWindowEvent(console, ConnectionCallbackManager.MainForm, WindowEvent.EventReceived);
        }        
    }
}
