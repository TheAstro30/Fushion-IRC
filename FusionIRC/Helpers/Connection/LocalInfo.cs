/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircCore.Settings.Theming.Structures;
using ircCore.Utils;

namespace FusionIRC.Helpers.Connection
{
    internal static class LocalInfo
    {
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
            /* Update settings */
            result.Address = result.Lookup;
            SettingsManager.Settings.Connection.LocalInfo.HostInfo = result;
        }
    }
}
