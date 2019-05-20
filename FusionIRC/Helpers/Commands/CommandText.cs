/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using FusionIRC.Forms.Child;
using ircClient;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandText
    {
        public static void ParseAction(ClientConnection client, FrmChildWindow child, string args)
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
            WindowManager.SetWindowEvent(child, WindowManager.MainForm, WindowEvent.MessageReceived);
            var action = string.Format("PRIVMSG {0} :{1}ACTION {2}{3}", child.Tag, (char)1, args, (char)1);
            client.Send(action);
        }

        public static void ParseAme(ClientConnection client, string args)
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

        public static void ParseSay(ClientConnection client, FrmChildWindow child, string args)
        {
            /* Message args to active window */
            if (child.WindowType == ChildWindowType.Console || !client.IsConnected)
            {
                return;
            }
            ParseMsg(client, child, string.Format("{0} {1}", child.Tag, args));
        }

        public static void ParseMsg(ClientConnection client, FrmChildWindow child, string args)
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
                    WindowManager.SetWindowEvent(child, WindowManager.MainForm, WindowEvent.MessageReceived);
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
            c.Client.Send(string.Format("PRIVMSG {0} :{1}", target, args));
        }

        public static void ParseAmsg(ClientConnection client, string args)
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
                WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
                client.Send(string.Format("PRIVMSG {0} :{1}", c.Tag, args));
            }
        }

        public static void ParseNotice(ClientConnection client, FrmChildWindow child, string args)
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
            WindowManager.SetWindowEvent(child, WindowManager.MainForm, WindowEvent.MessageReceived);
            client.Send(string.Format("NOTICE {0} :{1}", target, args));
        }

        public static void ParseEcho(ClientConnection client, string args)
        {
            /* Echo text to window */
            string text;
            FrmChildWindow w;
            var i = args.IndexOf(' ');
            if (i == -1)
            {
                /* Single word as <text> - possibly just active window only */
                w = WindowManager.GetActiveWindow();
                text = args;
            }
            else
            {
                var s = args.Substring(0, i);
                text = args.Substring(i + 1);
                switch (s.ToUpper())
                {
                    case "-A":
                        /* Active window (of active server called) */
                        w = WindowManager.GetActiveWindow();
                        break;

                    case "-S":
                        /* Console window (of active server called) */
                        w = WindowManager.GetConsoleWindow(client);
                        break;

                    default:
                        /* Search for window by name */
                        w = WindowManager.GetWindow(client, s);
                        if (w == null)
                        {              
                            /* Possibly "echo <text>" */
                            w = WindowManager.GetActiveWindow();
                            text = args;
                        }                        
                        break;
                }
            }            
            if (w == null)
            {
                /* Shouldn't be null ... */
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.EchoText,
                              TimeStamp = DateTime.Now,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            w.Output.AddLine(pmd.DefaultColor, pmd.Message);
        }
    }
}
