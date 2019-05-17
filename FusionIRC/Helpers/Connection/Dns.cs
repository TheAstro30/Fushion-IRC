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
    internal static class Dns
    {
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        public static void OnClientDnsFailed(ClientConnection client, DnsResult result)
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
                              DnsAddress = result.Lookup,
                              DnsHost = "Host unresolvable"
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }
    }
}
