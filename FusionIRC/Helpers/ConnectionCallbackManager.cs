/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
            var w = WindowManager.GetConsoleWindow(client);
            if (w == null || w.WindowType != ChildWindowType.Console)
            {
                return;
            }
            w.Output.AddLine(1, data);
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

        public static void OnTextChannel(ClientConnection client, string nick, string address, string channel, string text)
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
        }

        public static void OnActionChannel(ClientConnection client, string nick, string address, string channel, string text)
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
                                      Message = ThemeMessage.NickChange,
                                      TimeStamp = DateTime.Now,
                                      Nick = nick,
                                      NewNick = newNick                                      
                                  };
                    var pmd = ThemeManager.ParseMessage(tmd);
                    c.Output.AddLine(pmd.DefaultColor, true, pmd.Message);
                }
                else
                {
                    /* Rename any open query windows to keep track of it */
                    c.Tag = newNick;
                }
            }
        }
    }
}
