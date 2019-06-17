/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Reflection;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircCore.Settings.Theming.Structures;
using ircCore.Users;
using ircCore.Utils;

namespace FusionIRC.Helpers.Connection
{
    internal static class Ctcp
    {
        public static void OnCtcp(ClientConnection client, string nick, string address, string ctcp, string ctcpMsg)
        {
            /* Check ignore */
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
                              Message = ThemeMessage.CtcpText,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Address = address,
                              Target = ctcp
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
            /* CTCP finger */
            switch (ctcp)
            {
                case "PING":
                    client.Send(string.Format("NOTICE {0} :{1}PING {2}{3}", nick, (char) 1, ctcpMsg, (char) 1));
                    break;

                case "VERSION":
                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    client.Send(
                        string.Format(
                            "NOTICE {0} :{1}VERSION FusionIRC v{2}.{3}.{4} by Jason James Newland{5}", nick,
                            (char) 1, version.Major, version.Minor, version.MinorRevision, (char) 1));
                    break;

                case "TIME":
                    client.Send(string.Format("NOTICE {0} :{1}TIME {2}{3}", nick, (char) 1,
                                              string.Format("{0:ddd dd MMM yyyy, H:mm:ss tt}", DateTime.Now), (char) 1));
                    break;

                case "FINGER":
                    var msg = SettingsManager.Settings.Client.Messages.FingerReply;
                    var reply = string.IsNullOrEmpty(msg)
                                    ? string.Format("NOTICE {0} :{1}FINGER {2} ({3}) Idle {4} seconds{5}", nick,
                                                    (char) 1,
                                                    client.UserInfo.Nick, client.UserInfo.RealName, client.Idle,
                                                    (char) 1)
                                    : string.Format("NOTICE {0} :{1}FINGER {2} ({3}) Idle {4} seconds ({5}){6}", nick,
                                                    (char) 1,
                                                    client.UserInfo.Nick, client.UserInfo.RealName, client.Idle, msg,
                                                    (char) 1);
                    client.Send(reply);
                    break;
            }
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }
    }
}
