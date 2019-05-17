/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircClient;
using ircCore.Autos;
using ircCore.Settings.Theming;
using ircCore.Users;

namespace FusionIRC.Helpers.Connection
{
    internal static class Message
    {
        public static void OnTextChannel(ClientConnection client, string nick, string address, string channel, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(String.Format("{0}!{1}", nick, address)))
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
        }

        public static void OnTextSelf(ClientConnection client, string nick, string address, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(String.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            var c = WindowManager.GetWindow(client, nick) ??
                    WindowManager.AddWindow(client, ChildWindowType.Private, WindowManager.MainForm, nick, nick, false);
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
        }

        public static void OnActionChannel(ClientConnection client, string nick, string address, string channel, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(String.Format("{0}!{1}", nick, address)))
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
        }

        public static void OnActionSelf(ClientConnection client, string nick, string address, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(String.Format("{0}!{1}", nick, address)))
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
        }

        public static void OnNotice(ClientConnection client, string nick, string address, string text)
        {
            /* Check ignored */
            if (nick.Equals("nickserv", StringComparison.InvariantCultureIgnoreCase) && text.Contains("nickname is registered") && AutomationsManager.Automations.Identify.Enable)
            {
                var n = AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Identify,
                                                                  client.Network);
                if (n != null)
                {
                    foreach (var nn in n.Data)
                    {
                        if (nn.Item.Equals(client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
                        {
                            client.Send(String.Format("PRIVMSG NICKSERV :IDENTIFY {0}", nn.Value));
                            break;
                        }
                    }
                }
            }
            if (UserManager.IsIgnored(String.Format("{0}!{1}", nick, address)))
            {
                return;
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
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
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
