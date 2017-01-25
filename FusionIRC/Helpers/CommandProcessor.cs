﻿/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms;
using ircClient;
using ircClient.Classes;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Helpers
{
    public static class CommandProcessor
    {
        private static readonly Timer TmrWaitToReconnectTimeOut;

        /* Constructor */
        static CommandProcessor()
        {
            System.Diagnostics.Debug.Print("Constructor");
            /* Wait at least N number of seconds for socket to disconenct when issuing a new /server connection on a connected socket */
            TmrWaitToReconnectTimeOut = new Timer
                                             {
                                                 Interval = 4000
                                             };
            TmrWaitToReconnectTimeOut.Tick += TimerWaitToReconnectTimeOut;
        }

        /* Client waiting to reconnect after a disconnect */
        public static void OnClientWaitToReconnect(ClientConnection client)
        {
            /* This is called when the server command is issued on a currently connected server */
            if (TmrWaitToReconnectTimeOut.Enabled)
            {
                /* Event raised by connection class before time out timer fired */
                TmrWaitToReconnectTimeOut.Enabled = false;
            }
            ParseServerConnection(client,
                                  string.Format("{0}:{1}", client.Server.Address,
                                                client.Server.IsSsl
                                                    ? string.Format("+{0}", client.Server.Port.ToString())
                                                    : client.Server.Port.ToString()));
        }

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

        /* Private parsing methods */
        private static void ParseCommand(ClientConnection client, FrmChildWindow child, string command, string args)
        {            
            switch (command)
            {
                case "SERVER":
                    ParseServerConnection(client, args);
                    break;

                case "DISCONNECT":
                    ParseServerDisconnection(client);
                    break;

                case "ME":
                case "ACTION":
                case "DESCRIBE":
                    /* Action */
                    ParseAction(client, child, args);
                    break;

                case "AME":
                    ParseAme(client, args);
                    break;

                case "AMSG":
                    ParseAmsg(client, args);
                    break;

                case "MSG":
                    ParseMsg(client, child, args);
                    break;

                case "NOTICE":
                    ParseNotice(client, child, args);
                    break;

                case "PART":
                    ParsePart(client, child, args);
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

        /* Connection events */
        private static void ParseServerConnection(ClientConnection client, string args)
        {
            var s = args.Split(' ');
            string[] address;
            var port = 6667;
            bool ssl = false;
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || s.Length == 0)
            {
                return;
            }
            switch (s.Length)
            {
                case 1:
                    if (s[0].Equals("-m", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }
                    address = s[0].Split(':');
                    break;

                default:
                    /* /server -m server[:port]  or /server -m server[:port] -j #chan */
                    if (s[0].Equals("-m", StringComparison.InvariantCultureIgnoreCase))
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
                    c.Client.Parser.JoinChannelsOnConnect = s.Length > 2 &&
                                                            s[1].Equals("-j", StringComparison.InvariantCultureIgnoreCase)
                                                                ? s[2]
                                                                : s.Length > 3 &&
                                                                  s[2].Equals("-j", StringComparison.InvariantCultureIgnoreCase)
                                                                      ? s[3]
                                                                      : string.Empty;
                    break;
            }
            if (address.Length == 2)
            {
                /* Look for a '+' to determine SSL */
                if (address[1][0] == '+')
                {
                    ssl = true;
                    address[1] = address[1].Substring(1);
                }
                if (!int.TryParse(address[1], out port))
                {
                    port = 6667;
                }
            }
            /* If currently connected, we should send quit message */
            if (c.Client.IsConnecting)
            {
                c.Client.CancelConnection();
            }
            else if (c.Client.IsConnected)
            {
                c.Client.IsWaitingToReconnect = true;
                c.Client.Disconnect();
                c.Client.Server.Address = address[0];
                c.Client.Server.Port = port;
                c.Client.Server.IsSsl = ssl;
                TmrWaitToReconnectTimeOut.Tag = c.Client;
                TmrWaitToReconnectTimeOut.Enabled = true;
                return;
            }
            c.Client.Connect(address[0], port, ssl);
        }

        private static void ParseServerDisconnection(ClientConnection client)
        {
            System.Diagnostics.Debug.Print("Disconnect");
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                System.Diagnostics.Debug.Print("window is null");
                return;
            }
            if (client.IsConnecting)
            {
                /* Cancel current connection */
                System.Diagnostics.Debug.Print("Cancel");
                client.CancelConnection();
                return;
            }
            System.Diagnostics.Debug.Print("Disconnect");
            client.Disconnect();
        }

        /* Text events */
        private static void ParseAction(ClientConnection client, FrmChildWindow child, string args)
        {
            if (child.WindowType == ChildWindowType.Console || !client.IsConnected)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message =
                                  child.WindowType == ChildWindowType.Channel
                                      ? ThemeMessage.ChannelSelfActionText
                                      : child.WindowType == ChildWindowType.Private
                                            ? ThemeMessage.PrivateSelfActionText
                                            : ThemeMessage.ChannelSelfActionText,
                              TimeStamp = DateTime.Now,
                              Nick = client.UserInfo.Nick,
                              Prefix = child.WindowType == ChildWindowType.Channel ? child.Nicklist.GetNickPrefix(client.UserInfo.Nick) : string.Empty,
                              Text = args
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            child.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(child, ConnectionCallbackManager.MainForm, WindowEvent.MessageReceived);
            var action = string.Format("PRIVMSG {0} :{1}ACTION {2}{3}", child.Tag, (char)1, args, (char)1);
            client.Send(action);
        }

        private static void ParseAme(ClientConnection client, string args)
        {
            if (!client.IsConnected)
            {
                return;
            }
            foreach (var c in WindowManager.Windows[client].Where(c => c.WindowType == ChildWindowType.Channel))
            {
                ParseAction(client, c, args);
            }
        }

        private static void ParseMsg(ClientConnection client, FrmChildWindow child, string args)
        {
            /* /msg <target> <text> */
            var i = args.IndexOf(' ');
            if (i == -1 || !client.IsConnected)
            {
                return;
            }
            IncomingMessageData tmd;
            ParsedMessageData pmd;
            var target = args.Substring(0, i).Trim();
            args = args.Substring(i).Trim();
            /* Now we need to find the "target" window, if it exists send text to there, if not we display an "echo" in the acitve window */
            var c = WindowManager.GetWindow(client, target);
            if (c == null)
            {
                /* Echo the message to the active window */                
                if (child != null)
                {
                    tmd = new IncomingMessageData
                              {
                                  Message = ThemeMessage.MessageTargetText,
                                  TimeStamp = DateTime.Now,
                                  Target = target,
                                  Text = args
                              };
                    pmd = ThemeManager.ParseMessage(tmd);
                    child.Output.AddLine(pmd.DefaultColor, pmd.Message);
                    /* Update treenode color */
                    WindowManager.SetWindowEvent(child, ConnectionCallbackManager.MainForm, WindowEvent.MessageReceived);
                    child.Client.Send(string.Format("PRIVMSG {0} :{1}", target, args));                    
                }
                return;
            }
            /* Target exists */
            tmd = new IncomingMessageData
                      {
                          Message =
                              c.WindowType == ChildWindowType.Channel
                                  ? ThemeMessage.ChannelSelfText
                                  : c.WindowType == ChildWindowType.Private
                                        ? ThemeMessage.PrivateSelfText
                                        : ThemeMessage.ChannelSelfText,
                          TimeStamp = DateTime.Now,
                          Nick =
                              c.WindowType == ChildWindowType.Channel
                                  ? c.Client.UserInfo.Nick
                                  : c.WindowType == ChildWindowType.Private ? c.Tag.ToString() : target,
                          Text = args
                      };
            pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, ConnectionCallbackManager.MainForm, WindowEvent.MessageReceived);
            c.Client.Send(string.Format("PRIVMSG {0} :{1}", target, args));
        }

        private static void ParseAmsg(ClientConnection client, string args)
        {
            if (!client.IsConnected)
            {
                return;
            }
            foreach (var c in WindowManager.Windows[client].Where(c => c.WindowType == ChildWindowType.Channel))
            {
                var tmd = new IncomingMessageData
                              {
                                  Message = ThemeMessage.ChannelSelfText,
                                  TimeStamp = DateTime.Now,
                                  Nick = client.UserInfo.Nick,
                                  Prefix = c.Nicklist.GetNickPrefix(client.UserInfo.Nick),
                                  Text = args
                              };
                var pmd = ThemeManager.ParseMessage(tmd);
                c.Output.AddLine(pmd.DefaultColor, pmd.Message);
                /* Update treenode color */
                WindowManager.SetWindowEvent(c, ConnectionCallbackManager.MainForm, WindowEvent.MessageReceived);
                client.Send(string.Format("PRIVMSG {0} :{1}", c.Tag, args));
            }
        }

        private static void ParseNotice(ClientConnection client, FrmChildWindow child, string args)
        {
            if (!client.IsConnected || string.IsNullOrEmpty(args))
            {
                return;
            }
            var i = args.IndexOf(' ');
            if (i == -1)
            {
                return;
            }
            var target = args.Substring(0, i).Trim();
            args = args.Substring(i).Trim();
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.NoticeSelfText,
                              TimeStamp = DateTime.Now,
                              Target = target,
                              Text = args
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            child.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(child, ConnectionCallbackManager.MainForm, WindowEvent.MessageReceived);
            client.Send(string.Format("NOTICE {0} :{1}", target, args));
        }

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
                            ? string.Format("PRIVMSG {0} :{1}{2} {3}{4}", ctcp[0], (char) 1, ct, TimeFunctions.CTime(), (char) 1)
                            : string.Format("PRIVMSG {0} :{1}{2}{3}", ctcp[0], (char) 1, ct, (char) 1));
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
            client.UserInfo.Nick = nick;
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

        private static void ParsePart(ClientConnection client, FrmChildWindow child, string args)
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

        /* Timer callback */
        private static void TimerWaitToReconnectTimeOut(object sender, EventArgs e)
        {
            TmrWaitToReconnectTimeOut.Enabled = false;
            OnClientWaitToReconnect((ClientConnection)TmrWaitToReconnectTimeOut.Tag);
        }
    }
}
