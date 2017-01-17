/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms;
using ircClient;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers
{
    public static class ConnectionCallbackManager
    {
        /* All events raised by the client connection class are handled here, rather than on the main form */
        public static Form MainForm { get; set; }

        public static void OnDebugOut(ClientConnection client, string data)
        {
            System.Diagnostics.Debug.Print(data);
            //var c = WindowManager.GetConsoleWindow(client);
            //if (c == null || c.WindowType != ChildWindowType.Console)
            //{
            //    return;
            //}
            //c.Output.AddLine(1, data);
            ///* Update treenode color */
            //WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
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
                              Server = client.Server,
                              Port = client.Port
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, false, pmd.Message);            
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
            c.Output.AddLine(pmd.DefaultColor, false, pmd.Message);
        }

        public static void OnClientCancelConnection(ClientConnection client)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || c.WindowType != ChildWindowType.Console)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ConnectionCancelledText,
                              TimeStamp = DateTime.Now
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, false, pmd.Message);
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
            c.Output.AddLine(pmd.DefaultColor, false, pmd.Message);
            /* Iterate all open channels and clear nick list */
            foreach (var win in WindowManager.Windows[client].Where(win => win.WindowType == ChildWindowType.Channel))
            {
                win.Nicklist.Clear();
            }
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
                              Server = client.Server
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, false, pmd.Message);
        }

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
                              TimeStamp = DateTime.Now,
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, false, pmd.Message);
        }

        /* Text messages */
        public static void OnTextChannel(ClientConnection client, string nick, string address, string channel,
                                         string text)
        {
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
                              Address = address,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        public static void OnTextSelf(ClientConnection client, string nick, string address, string text)
        {
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
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        public static void OnActionChannel(ClientConnection client, string nick, string address, string channel,
                                           string text)
        {
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
                              Address = address,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        public static void OnActionSelf(ClientConnection client, string nick, string address, string text)
        {
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
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
        }

        /* Events */
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
            c.Nicklist.RemoveNick(nick);
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelPartText,
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
            if (nick.ToLower() == client.UserInfo.Nick.ToLower())
            {
                client.UserInfo.Nick = newNick;
            }
            /* This is harder as we have to go through all channels and rename the nick in the nicklist */
            foreach (var c in WindowManager.Windows[client].Where(c => c.WindowType != ChildWindowType.Console))
            {
                if (c.WindowType == ChildWindowType.Channel)
                {
                    c.Nicklist.RenameNick(nick, newNick);
                    var tmd = new IncomingMessageData
                                  {
                                      Message = ThemeMessage.NickChangeText,
                                      TimeStamp = DateTime.Now,
                                      Nick = nick,
                                      NewNick = newNick
                                  };
                    var pmd = ThemeManager.ParseMessage(tmd);
                    c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
                    /* Update treenode color */
                    WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
                }
                else
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
            foreach (var c in WindowManager.Windows[client].Where(c => c.WindowType != ChildWindowType.Console))
            {
                if (c.WindowType == ChildWindowType.Channel)
                {
                    c.Nicklist.RemoveNick(nick);
                    var tmd = new IncomingMessageData
                                  {
                                      Message = ThemeMessage.QuitText,
                                      TimeStamp = DateTime.Now,
                                      Nick = nick,
                                      Address = address,
                                      Text = msg
                                  };
                    var pmd = ThemeManager.ParseMessage(tmd);
                    c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
                    /* Update treenode color */
                    WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
                }
            }
        }

        public static void OnKickSelf(ClientConnection client, string nick, string channel, string msg)
        {
            var c = WindowManager.GetWindow(client, channel);
            if (c == null || c.WindowType != ChildWindowType.Channel)
            {
                return;
            }
            c.AutoClose = true;
            c.Close();
            c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.ChannelSelfKickText,
                              TimeStamp = DateTime.Now,
                              Target = channel,
                              Nick = nick,
                              Text = msg
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
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
                              KickedNick = knick,
                              Text = msg
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
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
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.EventReceived);
        }

        public static void OnModeChannel(ClientConnection client, string nick, string channel, string modes,
                                         string modeData)
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
                              Text = string.Format("{0} {1}", modes, modeData)
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, MainForm, WindowEvent.MessageReceived);
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
    }
}
