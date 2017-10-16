/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
<<<<<<< HEAD
=======
using FusionIRC.Forms.Child;
>>>>>>> origin/master
using FusionIRC.Forms.Misc;
using FusionIRC.Forms.Warning;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;
using ircCore.Users;
using ircCore.Utils;

namespace FusionIRC.Helpers
{
    public static class ConnectionCallbackManager
    {
        /* All events raised by the client connection class are handled here, rather than on the main form */
        public static Form MainForm { get; set; }

        public static void OnOther(ClientConnection client, string data)
        {
            System.Diagnostics.Debug.Print(data);
        }

        /* Connection events */
        public static void OnClientBeginConnect(ClientConnection client)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ConnectingText,
                              TimeStamp = DateTime.Now,
                              Server = client.Server.Address,
                              Port = client.Server.Port
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnClientConnected(ClientConnection client)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ConnectedText,
                              TimeStamp = DateTime.Now
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);            
        }

        public static void OnClientCancelConnection(ClientConnection client)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            client.IsManualDisconnect = false;
            client.UserInfo.AlternateUsed = false;
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ConnectionCancelledText,
                              TimeStamp = DateTime.Now
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnClientDisconnected(ClientConnection client)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.DisconnectedText,
                              TimeStamp = DateTime.Now
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
            /* Iterate all open channels and clear nick list (or close it's window) */
            UpdateChannelsOnDisconnect(client, pmd);
            client.UserInfo.AlternateUsed = false;
            if (client.IsManualDisconnect)
            {
                client.IsManualDisconnect = false;
                return;
            }            
            /* Now we process re-connection code if the server wasn't manually disconnected by the user */
        }

        public static void OnClientConnectionError(ClientConnection client, string error)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ConnectionErrorText,
                              TimeStamp = DateTime.Now,
                              Text = error,
                              Server = client.Server.Address
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
            /* Iterate all open channels and clear nick list (or close it's window) */
            UpdateChannelsOnDisconnect(client, pmd);
            client.UserInfo.AlternateUsed = false;
        }

        public static void OnClientSslInvalidCertificate(ClientConnection client, X509Certificate certificate)
        {
            var store = new X509Store("FusionIRC", StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            if (store.Certificates.Cast<X509Certificate2>().Any(cert => cert.Subject == certificate.Subject))
            {
                client.SslAcceptCertificate(true);
                store.Close();
                return;
            }
            using (var ssl = new FrmSslError())
            {
                ssl.Server = client.Server.Address;
                ssl.CertificateStore = store;
                ssl.Certificate = certificate;
                if (ssl.ShowDialog(MainForm) == DialogResult.Cancel)
                {
                    client.SslAcceptCertificate(false);
                    store.Close();
                    return;
                }
            }
            store.Close();
            client.SslAcceptCertificate(true);
        }

        /* Local info/DNS callbacks */
        public static void OnClientLocalInfoResolved(ClientConnection client, DnsResult result)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.LocalInfoReplyText,
                              TimeStamp = DateTime.Now,
                              DnsAddress = result.Address,
                              DnsHost = result.HostName
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
            /* Update settings */
            SettingsManager.Settings.Connection.LocalInfo.HostInfo = result;
        }

        public static void OnClientLocalInfoFailed(ClientConnection client, DnsResult result)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.LocalInfoReplyText,
                              TimeStamp = DateTime.Now,
                              DnsAddress = result.Lookup,
                              DnsHost = "Unknown host"
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
            /* Update settings */
            result.Address = result.Lookup;
            SettingsManager.Settings.Connection.LocalInfo.HostInfo = result;
        }

        public static void OnClientDnsResolved(ClientConnection client, DnsResult result)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.DnsLookupReplyText,
                              TimeStamp = DateTime.Now,
                              DnsAddress = result.Address,
                              DnsHost = result.HostName
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnClientDnsFailed(ClientConnection client, DnsResult result)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
<<<<<<< HEAD
            }           
=======
            }
>>>>>>> origin/master
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.DnsLookupReplyText,
                              TimeStamp = DateTime.Now,
                              DnsAddress = result.Lookup,
                              DnsHost = "Host unresolvable"
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        /* IRC events */
        public static void OnServerPingPong(ClientConnection client)
        {
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        /* Text messages */
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        public static void OnTextSelf(ClientConnection client, string nick, string address, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            var c = WindowManager.GetWindow(client, nick) ??
                    WindowManager.AddWindow(client, ChildWindowType.Private, MainForm, nick, nick, false);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        public static void OnActionSelf(ClientConnection client, string nick, string address, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
            {
                return;
            }
            var c = WindowManager.GetWindow(client, nick) ??
                    WindowManager.AddWindow(client, ChildWindowType.Private, MainForm, nick, nick, false);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        public static void OnNotice(ClientConnection client, string nick, string address, string text)
        {
            /* Check ignored */
            if (UserManager.IsIgnored(string.Format("{0}!{1}", nick, address)))
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        /* Events */
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        public static void OnMotd(ClientConnection client, string text, bool isEnd)
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
            if (!isEnd)
            {
                return;
            }
            /* Set invisible */
            if (client.UserInfo.Invisible)
            {
                client.Send(string.Format("MODE {0} +i", client.UserInfo.Nick));
            }
            System.Diagnostics.Debug.Print("Updating recent server for " + client.Server.Address);
            /* Update recent servers list */
<<<<<<< HEAD
            UpdateRecentServers(client);
            /* Resolve local IP */
            ResolveLocalInfo(client);            
            /* Finally process auto join/rejoin open channels */
            ProcessAutoJoin(client);
=======
            foreach (var s in ServerManager.Servers.Recent.Server.Where(s => s.Address.Equals(client.Server.Address, StringComparison.InvariantCultureIgnoreCase)))
            {
                System.Diagnostics.Debug.Print("remove " + s.Address);
                /* Remove it from it's current position */
                ServerManager.Servers.Recent.Server.Remove(s);
                break;
            }
            /* Insert current server at the top of the recent list */
            ServerManager.Servers.Recent.Server.Insert(0, client.Server);
            /* Keep the list length down */
            if (ServerManager.Servers.Recent.Server.Count > 25)
            {
                ServerManager.Servers.Recent.Server.RemoveAt(ServerManager.Servers.Recent.Server.Count - 1);
            }
            /* Resolve local IP */
            switch (SettingsManager.Settings.Connection.LocalInfo.LookupMethod)
            {
                case LocalInfoLookupMethod.Socket:
                    client.ResolveLocalInfo(client.SocketLocalIp);
                    break;

                case LocalInfoLookupMethod.Server:
                    client.Send(string.Format("USERHOST {0}", client.UserInfo.Nick));
                    break;
            }
            /* Finally, join open channels */
            if (SettingsManager.Settings.Client.Channels.JoinOpenChannelsOnConnect)
            {
                foreach (var chan in WindowManager.Windows[client].Where(chan => chan.WindowType == ChildWindowType.Channel))
                {
                    client.Send(string.Format("JOIN {0}", chan.Tag));
                }
            }
            /* Process auto-join ... */
>>>>>>> origin/master
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

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
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnJoinSelf(ClientConnection client, string channel)
        {
            /* Create a new channel - need to check for open channels */
            var c = WindowManager.AddWindow(client, ChildWindowType.Channel, MainForm, channel, channel, true);
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
            client.Send(string.Format("WHO {0}", channel));
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

        public static void OnPartUser(ClientConnection client, string nick, string address, string channel)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }            
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelPartText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Prefix = c.Nicklist.GetNickPrefix(nick),
                              Address = address,
                              Target = channel
                          };
            c.Nicklist.RemoveNick(nick);
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
                /* Update treenode color */
                WindowManager.SetWindowEvent(console, MainForm, WindowEvent.EventReceived);
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
                    WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
                WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
                WindowManager.SetWindowEvent(console, MainForm, WindowEvent.EventReceived);
                if (c != null)
                {
                    /* IE: channel is still open ... */
                    c.Output.AddLine(pmd.DefaultColor, pmd.Message);
                    /* Update treenode color */
                    WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnModeSelf(ClientConnection client, string nick, string modes)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ModeSelfText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Text = modes
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnModeChannel(ClientConnection client, string nick, string channel, string modes, string modeData)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ModeChannelText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Prefix = c.Nicklist.GetNickPrefix(nick),
                              Text = string.Format("{0} {1}", modes, modeData)
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
            /* We need to parse the mode data */
            var plusMode = false;
            var data = modeData.Split(' ');
            var length = data.Length - 1;
            if (length == -1)
            {
                return;
            }
            var modePointer = 0;
            for (var i = 0; i <= modes.Length - 1; i++)
            {
                /* Iterate the +v-o+b etc ... */
                switch (modes[i])
                {
                    case '+':
                        plusMode = true;
                        break;

                    case '-':
                        plusMode = false;
                        break;

                    case 'v':
                    case 'o':
                    case 'a':
                    case 'h':
                    case 'q':
                        if (modePointer > length)
                        {
                            return;
                        }
                        if (plusMode)
                        {
                            c.Nicklist.AddUserMode(data[modePointer], modes[i].ToString());
                        }
                        else
                        {
                            c.Nicklist.RemoveUserMode(data[modePointer], modes[i].ToString());
                        }
                        modePointer++;
                        break;
                }
            }
        }

        public static void OnRaw(ClientConnection client, string message)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.RawText,
                              TimeStamp = DateTime.Now,
                              Text = message
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnUserInfo(ClientConnection client, string info)
        {
            System.Diagnostics.Debug.Print(info);
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
                whois.ShowDialog(MainForm);
            }
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
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnCtcp(ClientConnection client, string nick, string address, string ctcp)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.CtcpText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Target = ctcp
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnCtcpReply(ClientConnection client, string nick, string address, string ctcp, string text)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            string s;            
            if (ctcp == "PING")
            {
                /* We need to convert the "text" field to a time (current ctime - sent ctime) */
                long time;
                long.TryParse(text, out time);
                long currentTime;
                long.TryParse(TimeFunctions.CTime(), out currentTime);
                s = TimeFunctions.GetDuration((int) (currentTime - time), false);
            }
            else
            {
                s = text;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.CtcpReplyText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Target = ctcp,
                              Text = s
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        /* Private methods */
        private static void UpdateChannelsOnDisconnect(ClientConnection client, ParsedMessageData message)
        {
            /* Here we either close all open windows or just clear the nicklist - dependant on settings */
            foreach (var win in WindowManager.Windows[client].Where(win => win.WindowType == ChildWindowType.Channel))
            {
                win.Nicklist.Clear();
                win.Output.AddLine(message.DefaultColor, message.Message);
                /* Update treenode color */
                WindowManager.SetWindowEvent(win, MainForm, WindowEvent.EventReceived);
            }
        }

        private static void UpdateRecentServers(ClientConnection client)
        {
            var servers = ServerManager.Servers.Recent.Server;
            var index = servers.FindIndex(s => s.Address.Equals(client.Server.Address, StringComparison.InvariantCultureIgnoreCase));
            if (index > 0)
            {
                servers.RemoveAt(index);
                /* Insert current server at the top of the recent list */
                servers.Insert(0, client.Server);
            }
            /* Keep the list length down */
            if (servers.Count > 25)
            {
                servers.RemoveAt(servers.Count - 1);
            }
        }

        private static void ResolveLocalInfo(ClientConnection client)
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

        private static void ProcessAutoJoin(ClientConnection client)
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
        }
    }
}
