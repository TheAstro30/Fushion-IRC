/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircClient;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers.Connection
{
    internal static class Modes
    {
        public static void OnChannelModes(ClientConnection client, string channel, string modes)
        {
            var w = WindowManager.GetWindow(client, channel);
            if (w != null)
            {
                w.Modes.SetModes(modes);
            }
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
            /* Update user modes in title bar */
            c.Modes.SetModes(modes);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
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

                    case 'b':
                    case 'f':
                        modePointer++;
                        break;

                    case 'l':
                        if (plusMode)
                        {
                            c.Modes.AddMode(modes[i], data[modePointer]);
                            modePointer++;
                        }
                        else
                        {
                            c.Modes.Limit = 0;
                            c.Modes.RemoveMode(modes[i]);
                        }
                        break;

                    case 'k':
                        if (plusMode)
                        {
                            c.Modes.AddMode(modes[i], data[modePointer]);
                        }
                        else
                        {
                            c.Modes.Key = string.Empty;
                            c.Modes.RemoveMode(modes[i]);
                        }
                        modePointer++;
                        break;

                    default:
                        /* Update channel modes */
                        c.Modes.AddMode(modes[i], string.Empty);
                        break;
                }
            }
        }
    }
}
