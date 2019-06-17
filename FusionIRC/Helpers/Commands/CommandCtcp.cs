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

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandCtcp
    {
        public static void ParseCtcp(ClientConnection client, string args)
        {
            if (!client.IsConnected || string.IsNullOrEmpty(args))
            {
                return;
            }
            var ctcp = args.Split(' ');
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null || ctcp.Length < 2)
            {
                return;
            }
            var ct = ctcp[1].ToUpper();
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.CtcpSelfText,
                              TimeStamp = DateTime.Now,
                              Target = ct,
                              Nick = ctcp[0],
                              Text = args
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            if (SettingsManager.Settings.Client.Show.Ctcps)
            {
                var active = WindowManager.GetActiveWindow();
                if (active != c && active.Client == client)
                {
                    /* Active window of this connection */
                    active.Output.AddLine(pmd.DefaultColor, pmd.Message);
                }
            }
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.MessageReceived);
            client.Send(ct == "PING"
                            ? string.Format("PRIVMSG {0} :{1}{2} {3}{4}", ctcp[0], (char)1, ct, TimeFunctions.CTime(), (char)1)
                            : string.Format("PRIVMSG {0} :{1}{2}{3}", ctcp[0], (char)1, ct, (char)1));
        }
    }
}
