/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using FusionIRC.Forms.ChannelProperties;
using FusionIRC.Forms.ChannelProperties.Controls;
using ircClient;
using ircClient.Parsing;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Helpers.Connection
{
    internal static class Channel
    {
        public static void OnTopicIs(ClientConnection client, string channel, string text)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelTopic,
                              TimeStamp = DateTime.Now,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update title bar */
            c.Modes.SetTopic(text);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnTopicSetBy(ClientConnection client, string channel, string text)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelTopicSet,
                              TimeStamp = DateTime.Now,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnTopicChanged(ClientConnection client, string nick, string channel, string text)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelTopicChange,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Prefix = c.Nicklist.GetNickPrefix(nick),
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update title bar */
            c.Modes.SetTopic(text);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnJoinUser(ClientConnection client, string nick, string address, string channel)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            c.Nicklist.AddNicks(nick);
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelJoinText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Target = channel
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnJoinSelf(ClientConnection client, string channel)
        {
            /* Create a new channel - need to check for open channels */
            var c = WindowManager.AddWindow(client, ChildWindowType.Channel, WindowManager.MainForm, channel, channel, true);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelSelfJoinText,
                              TimeStamp = DateTime.Now,
                              Target = channel
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Send /WHO to get user addresses */
            client.Send(string.Format("WHO {0}{1}MODE {0}", channel, Environment.NewLine));
            /* Remember to unset the halt disconnect message bool */
            c.DisconnectedShown = false;
        }

        public static void OnPartSelf(ClientConnection client, string channel)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            c.AutoClose = true;
            c.Close();
        }

        public static void OnPartUser(ClientConnection client, string nick, string address, string channel, string text)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = !string.IsNullOrEmpty(text) ? ThemeMessage.ChannelPartTextMessage : ThemeMessage.ChannelPartText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Prefix = c.Nicklist.GetNickPrefix(nick),
                              Address = address,
                              Target = channel,
                              Text = text
                          };
            c.Nicklist.RemoveNick(nick);
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnNames(ClientConnection client, string channel, string names)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            c.Nicklist.AddNicks(names);
        }

        public static void OnWho(ClientConnection client, string nick, string channel, string address)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            c.Nicklist.UpdateNickAddress(nick, address);
        }

        public static void OnNick(ClientConnection client, string nick, string newNick)
        {
            /* If the nick is me, update client user info data */
            if (nick.Equals(client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase))
            {
                /* If the newnick == alternative nick, we switch them back */
                if (newNick.Equals(client.UserInfo.Alternative, StringComparison.InvariantCultureIgnoreCase))
                {
                    /* Otherwise both alternative and nick will be the same defeating having an alternative to begin with */
                    client.UserInfo.Alternative = client.UserInfo.Nick;
                }
                client.UserInfo.Nick = newNick;
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
                                  NewNick = newNick
                              };
                var pmd = ThemeManager.ParseMessage(tmd);
                console.Output.AddLine(pmd.DefaultColor, pmd.Message);
                /* Update title bar */
                var net = !string.IsNullOrEmpty(client.Network)
                              ? client.Network
                              : client.Server.Address;
                console.Text = string.Format("{0}: {1} ({2}:{3}) {4}", net, client.UserInfo.Nick, client.Server.Address,
                                             client.Server.Port, console.Modes);
                console.DisplayNode.Text = string.Format("{0}: {1} ({2})", net, client.UserInfo.Nick,
                                                         client.Server.Address);
                /* Update treenode color */
                WindowManager.SetWindowEvent(console, WindowManager.MainForm, WindowEvent.EventReceived);
            }
            /* This is harder as we have to go through all channels and rename the nick in the nicklist */
            foreach (var c in WindowManager.Windows[client].Where(c => c.WindowType != ChildWindowType.Console))
            {
                if (c.WindowType == ChildWindowType.Channel && c.Nicklist.ContainsNick(nick))
                {
                    var tmd = new IncomingMessageData
                                  {
                                      Message = ThemeMessage.NickChangeUserText,
                                      TimeStamp = DateTime.Now,
                                      Nick = nick,
                                      Prefix = c.Nicklist.GetNickPrefix(nick),
                                      NewNick = newNick
                                  };
                    c.Nicklist.RenameNick(nick, newNick);
                    var pmd = ThemeManager.ParseMessage(tmd);
                    c.Output.AddLine(pmd.DefaultColor, pmd.Message);
                    /* Update treenode color */
                    WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
                }
                else if (c.WindowType == ChildWindowType.Private && c.Tag.ToString().Equals(nick, StringComparison.InvariantCultureIgnoreCase))
                {
                    /* Rename any open query windows to keep track of it */
                    c.Text = newNick;
                    c.Tag = newNick;
                    c.DisplayNode.Text = c.ToString();
                }
            }
        }

        public static void OnQuit(ClientConnection client, string nick, string address, string msg)
        {
            /* This is basically the same as OnNick - remove the nick from the nicklist and display the message in channels */
            foreach (var c in WindowManager.Windows[client].Where(c => c.WindowType == ChildWindowType.Channel && c.Nicklist.ContainsNick(nick)))
            {
                var tmd = new IncomingMessageData
                              {
                                  Message = ThemeMessage.QuitText,
                                  TimeStamp = DateTime.Now,
                                  Nick = nick,
                                  Prefix = c.Nicklist.GetNickPrefix(nick),
                                  Address = address,
                                  Text = msg
                              };
                c.Nicklist.RemoveNick(nick);
                var pmd = ThemeManager.ParseMessage(tmd);
                c.Output.AddLine(pmd.DefaultColor, pmd.Message);
                /* Update treenode color */
                WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
            }
        }

        public static void OnKickSelf(ClientConnection client, string nick, string channel, string msg)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            if (!SettingsManager.Settings.Client.Channels.KeepChannelsOpen)
            {
                c.AutoClose = true;
                c.Close();
                c = null;
            }
            var console = WindowManager.GetConsoleWindow(client);
            if (console != null)
            {
                var tmd = new IncomingMessageData
                              {
                                  Message = ThemeMessage.ChannelSelfKickText,
                                  TimeStamp = DateTime.Now,
                                  Target = channel,
                                  Nick = nick,
                                  Text = msg
                              };
                var pmd = ThemeManager.ParseMessage(tmd);
                console.Output.AddLine(pmd.DefaultColor, pmd.Message);
                /* Update treenode color */
                WindowManager.SetWindowEvent(console, WindowManager.MainForm, WindowEvent.EventReceived);
                if (c != null)
                {
                    /* IE: channel is still open ... */
                    c.Output.AddLine(pmd.DefaultColor, pmd.Message);
                    /* Update treenode color */
                    WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
                }
            }
            /* Attempt to rejoin the channel */
            if (SettingsManager.Settings.Client.Channels.ReJoinChannelsOnKick)
            {
                client.Send(string.Format("JOIN {0}", channel));
            }
        }

        public static void OnKickUser(ClientConnection client, string nick, string knick, string channel, string msg)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            c.Nicklist.RemoveNick(knick);
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelKickText,
                              TimeStamp = DateTime.Now,
                              Target = channel,
                              Nick = nick,
                              Prefix = c.Nicklist.GetNickPrefix(nick),
                              KickedNick = knick,
                              Text = msg
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnInvite(ClientConnection client, string nick, string address, string channel)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.InviteText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Target = channel
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnModeListData(ClientConnection client, ModeListType type, string data)
        {
            if (WindowManager.ChannelProperties == null)
            {
                return;                
            }
            var sp = data.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length < 4)
            {
                return;
            }
            var p = new ChannelPropertyData
                        {
                            Address = sp[1],
                            SetByNick = sp[2],
                            Date = TimeFunctions.FormatAsciiTime(sp[3], "ddd dd/MM/yyyy h:nnt")
                        };
            switch (type)
            {
                case ModeListType.Invite:
                    WindowManager.ChannelProperties.Invites.Add(p);
                    break;

                case ModeListType.Except:
                    WindowManager.ChannelProperties.Excepts.Add(p);
                    break;

                case ModeListType.Ban:
                    WindowManager.ChannelProperties.Bans.Add(p);
                    break;
            }
        }

        public static void OnEndOfChannelProperties(ClientConnection client)
        {
            if (WindowManager.ChannelProperties == null || WindowManager.ChannelProperties.Visible)
            {
                return;
            }
            WindowManager.ChannelProperties.ShowDialog();
            WindowManager.ChannelProperties = null;
        }
    }
}
