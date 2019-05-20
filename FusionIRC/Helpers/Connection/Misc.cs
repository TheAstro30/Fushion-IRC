﻿/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using FusionIRC.Forms.Favorites;
using FusionIRC.Forms.Misc;
using ircClient;
using ircCore.Autos;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers.Connection
{
    internal static class Misc
    {
        public static void OnServerPingPong(ClientConnection client)
        {
            if (!SettingsManager.Settings.Client.Show.PingPong)
            {
                return;
            }
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ServerPingPongText,
                              TimeStamp = DateTime.Now
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnErrorLink(ClientConnection client, string message)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.InfoText,
                              TimeStamp = DateTime.Now,
                              Text = message
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnWelcome(ClientConnection client, string text)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.WelcomeText,
                              TimeStamp = DateTime.Now,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
        }

        public static void OnMotd(ClientConnection client, string text, bool isEnd)
        {
            if (SettingsManager.Settings.Client.Show.PingPong)
            {
                var c = WindowManager.GetConsoleWindow(client);
                if (c == null)
                {
                    return;
                }
                var tmd = new IncomingMessageData
                              {
                                  Message = ThemeMessage.MotdText,
                                  TimeStamp = DateTime.Now,
                                  Text = text
                              };
                var pmd = ThemeManager.ParseMessage(tmd);
                c.Output.AddLine(pmd.DefaultColor, pmd.Message);
                /* Update treenode color */
                WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
            }
            if (!isEnd)
            {
                return;
            }
            /* Set invisible */
            if (client.UserInfo.Invisible)
            {
                client.Send(string.Format("MODE {0} +i", client.UserInfo.Nick));
            }
            /* Update recent servers list */
            UpdateRecentServers(client);
            /* Resolve local IP */
            ResolveLocalInfo(client);
            /* Finally process auto join/rejoin open channels */
            ProcessAutoJoin(client);
        }

        public static void OnLUsers(ClientConnection client, string text)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.LUsersText,
                              TimeStamp = DateTime.Now,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
        }

        public static void OnUserInfo(ClientConnection client, string info)
        {
            var n = info.Split('=');
            if (n.Length < 2)
            {
                return;
            }
            if (string.Compare(n[0], client.UserInfo.Nick, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return;
            }
            var address = n[1].Split('@');
            if (address.Length < 2)
            {
                return;
            }
            client.ResolveLocalInfo(address[1]);
        }

        public static void OnWhois(ClientConnection client)
        {
            using (var whois = new FrmWhoisInfo(client.Parser.Whois))
            {
                whois.ShowDialog(WindowManager.MainForm);
            }
        }

        public static void OnNetworkNameChanged(ClientConnection client, string network)
        {
            client.Network = network;
            /* Update console window title associated with this event */
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            c.Text = string.Format("{0}: {1} ({2}:{3})",
                                   !string.IsNullOrEmpty(client.Network) ? client.Network : client.Server.Address,
                                   client.UserInfo.Nick, client.Server.Address, client.Server.Port);
            c.DisplayNode.Text = string.Format("{0}: {1} ({2})", network, client.UserInfo.Nick,
                                               client.Server.Address);
        }

        public static void UpdateChannelsOnDisconnect(ClientConnection client, ParsedMessageData message)
        {
            /* Here we either close all open windows or just clear the nicklist - dependant on settings */
            foreach (var win in WindowManager.Windows[client].Where(win => win.WindowType == ChildWindowType.Channel).Where(win => !win.DisconnectedShown))
            {
                win.DisconnectedShown = true;
                win.Nicklist.Clear();
                win.Output.AddLine(message.DefaultColor, message.Message);
                /* Update treenode color */
                WindowManager.SetWindowEvent(win, WindowManager.MainForm, WindowEvent.EventReceived);
            }
        }

        public static void UpdateRecentServers(ClientConnection client)
        {
            var servers = ServerManager.Servers.Recent.Server;
            var index = servers.FindIndex(s => s.Address.Equals(client.Server.Address, StringComparison.InvariantCultureIgnoreCase));
            if (index > -1)
            {
                servers.RemoveAt(index);
            }
            /* Lol... helps to actually add the damn thing if it doesn't exist */
            servers.Insert(0, client.Server);
            /* Keep the list length down */
            if (servers.Count > 25)
            {
                servers.RemoveAt(servers.Count - 1);
            }
        }

        public static void ResolveLocalInfo(ClientConnection client)
        {
            switch (SettingsManager.Settings.Connection.LocalInfo.LookupMethod)
            {
                case LocalInfoLookupMethod.Socket:
                    client.ResolveLocalInfo(client.SocketLocalIp);
                    break;

                case LocalInfoLookupMethod.Server:
                    client.Send(string.Format("USERHOST {0}", client.UserInfo.Nick));
                    break;
            }
        }

        public static void ProcessAutoJoin(ClientConnection client)
        {
            /* Join any open channels */
            if (SettingsManager.Settings.Client.Channels.JoinOpenChannelsOnConnect)
            {
                foreach (var chan in WindowManager.Windows[client].Where(chan => chan.WindowType == ChildWindowType.Channel))
                {
                    client.Send(string.Format("JOIN {0}", chan.Tag));
                }
            }
            /* Process auto-join ... */
            if (AutomationsManager.Automations.Join.Enable)
            {
                var join = AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Join,
                                                                     client.Network);
                if (@join != null)
                {
                    foreach (var j in @join.Data)
                    {
                        client.Send(string.IsNullOrEmpty(j.Value)
                                        ? string.Format("JOIN {0}", j.Item)
                                        : string.Format("JOIN {0} {1}", j.Item, j.Value));
                    }
                }
            }
            /* Show favorites dialog */
            if (!SettingsManager.Settings.Client.Channels.ShowFavoritesDialogOnConnect)
            {
                return;
            }
            using (var fave = new FrmFavorites(client))
            {
                fave.ShowDialog(WindowManager.MainForm);
            }
        }

        public static void OnClientIdentDaemonRequest(ClientConnection client, string remoteHost, string data)
        {
            if (!SettingsManager.Settings.Connection.Identd.ShowRequests)
            {
                return;
            }
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.InfoText,
                              TimeStamp = DateTime.Now,
                              Text = string.Format("Identd request: ({0}) {1}", remoteHost, data)
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }
    }
}