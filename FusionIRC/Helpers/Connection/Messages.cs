/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircClient;
using ircCore.Autos;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircCore.Settings.Theming.Structures;
using ircCore.Users;
using ircScript.Classes.Structures;

namespace FusionIRC.Helpers.Connection
{
    internal static class Messages
    {
        public static void OnTextChannel(ClientConnection client, string nick, string address, string channel, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Prefix = c.Nicklist.GetNickPrefix(nick),
                              Address = address,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
            /* Flash main window if inactive */
            if (SettingsManager.Settings.Client.Flash.Channel)
            {
                ((TrayIcon) WindowManager.MainForm).FlashWindow();
            }
            /* Process event script */
            var e = new ScriptArgs
                        {
                            ChildWindow = c,
                            ClientConnection = client,
                            Nick = nick,
                            Address = address,
                            Channel = c.Tag.ToString()
                        };
            Events.Execute("text", e, text);
        }

        public static void OnTextSelf(ClientConnection client, string nick, string address, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            var c = WindowManager.GetWindow(client, nick);
            if (c == null)
            {
                c = WindowManager.AddWindow(client, ChildWindowType.Private, WindowManager.MainForm, nick, nick, false);
                ThemeManager.PlaySound(ThemeSound.PrivateMessage);
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.PrivateText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
            /* Flash main window if inactive */
            if (SettingsManager.Settings.Client.Flash.Private)
            {
                ((TrayIcon)WindowManager.MainForm).FlashWindow();
            }
            /* Process event script */
            var e = new ScriptArgs
                        {
                            ChildWindow = c,
                            ClientConnection = client,
                            Nick = nick, 
                            Address = address,
                            Channel = c.Tag.ToString()
                        };
            Events.Execute("text", e, text);
        }

        public static void OnActionChannel(ClientConnection client, string nick, string address, string channel, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelActionText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Prefix = c.Nicklist.GetNickPrefix(nick),
                              Address = address,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
            /* Flash main window if inactive */
            if (SettingsManager.Settings.Client.Flash.Channel)
            {
                ((TrayIcon)WindowManager.MainForm).FlashWindow();
            }
            /* Process event script */
            var e = new ScriptArgs
                        {
                            ChildWindow = c,
                            ClientConnection = client,
                            Nick = nick,
                            Address = address,
                            Channel = c.Tag.ToString()
                        };
            Events.Execute("action", e, text);
        }

        public static void OnActionSelf(ClientConnection client, string nick, string address, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            var c = WindowManager.GetWindow(client, nick) ??
                    WindowManager.AddWindow(client, ChildWindowType.Private, WindowManager.MainForm, nick, nick, false);
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.PrivateActionText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
            /* Flash main window if inactive */
            if (SettingsManager.Settings.Client.Flash.Private)
            {
                ((TrayIcon)WindowManager.MainForm).FlashWindow();
            }
            /* Process event script */
            var e = new ScriptArgs
                        {
                            ChildWindow = c,
                            ClientConnection = client,
                            Nick = nick,
                            Address = address,
                            Channel = c.Tag.ToString()
                        };
            Events.Execute("action", e, text);
        }

        public static void OnNotice(ClientConnection client, string nick, string address, string target, string text)
        {            
            if (nick.Equals("nickserv", StringComparison.InvariantCultureIgnoreCase) && text.Contains("nickname is registered") && AutomationsManager.Automations.Identify.Enable)
            {
                /* ALL takes priority */
                var n = AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Identify, "All") ??
                        AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Identify,
                                                                  client.Network);
                if (n != null)
                {
                    foreach (var nn in n.Data)
                    {
                        if (nn.Item.Equals(client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
                        {
                            client.Send(string.Format("PRIVMSG NICKSERV :IDENTIFY {0}", nn.Value));
                            break;
                        }
                    }
                }
            }
            /* Check ignored */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            var channel = string.Empty;
            if (target.Length > 0 && target[0] == client.Parser.ChannelPrefixTypes.MatchChannelType(target[0]))
            {
                /* It's to a channel - I will be using this later */
                channel = target;
            }
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.NoticeText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            if (SettingsManager.Settings.Client.Show.Notices)
            {
                var active = WindowManager.GetActiveWindow();
                if (active != c && active.Client == client)
                {
                    /* Active window of this connection */
                    active.Output.AddLine(pmd.DefaultColor, pmd.Message);
                }
            }
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
            /* Process event script */
            var e = new ScriptArgs
                        {
                            ChildWindow = !string.IsNullOrEmpty(channel) ? WindowManager.GetWindow(c.Client, channel) : null,
                            ClientConnection = client,
                            Nick = nick,
                            Address = address,
                            Channel = channel
                        };
            Events.Execute("notice", e, text);
        }

        public static void OnWallops(ClientConnection client, string nick, string address, string text)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.WallopsText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
        }
    }
}
