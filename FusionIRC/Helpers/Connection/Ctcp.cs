/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircClient;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Helpers.Connection
{
    internal static class Ctcp
    {
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
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
                Int64.TryParse(text, out time);
                long currentTime;
                Int64.TryParse(TimeFunctions.CTime(), out currentTime);
                s = TimeFunctions.GetDuration((int)(currentTime - time), false);
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }
    }
}
