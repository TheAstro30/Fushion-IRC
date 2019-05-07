/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandServer
    {
        private static readonly Timer TmrWaitToReconnectTimeOut;

        static CommandServer()
        {
            /* Wait at least N number of seconds for socket to disconenct when issuing a new /server connection on a connected socket */
            TmrWaitToReconnectTimeOut = new Timer
            {
                Interval = 4000
            };
            TmrWaitToReconnectTimeOut.Tick += TimerWaitToReconnectTimeOut;
        }
        public static void ParseServerConnection(ClientConnection client, string args)
        {
            string[] s = null;
            string[] address = null;
            var port = SettingsManager.Settings.Connection.Options.DefaultPort;
            var ssl = false;
            if (String.IsNullOrEmpty(args))
            {
                var recent = ServerManager.Servers.Recent.Server.Count > 0
                                 ? ServerManager.Servers.Recent.Server[0].ToString()
                                 : string.Format("irc.dal.net:{0}", port);
                var server = !String.IsNullOrEmpty(client.Server.Address) ? client.Server.ToString() : recent;
                address = server.Split(':');
            }
            else
            {
                s = args.Split(' ');
            }
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            if (s != null)
            {
                switch (s.Length)
                {
                    case 1:
                        if (s[0].Equals("-m", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return;
                        }
                        address = s[0].Split(':');
                        break;

                    default:
                        /* /server -m server[:port]  or /server -m server[:port] -j #chan */
                        if (s[0].Equals("-m", StringComparison.InvariantCultureIgnoreCase))
                        {
                            /* Create new connection */
                            c = WindowManager.AddWindow(null, ChildWindowType.Console,
                                                        ConnectionCallbackManager.MainForm,
                                                        "Console", "Console", true);
                            if (c == null)
                            {
                                return;
                            }
                            address = s[1].Split(':');
                        }
                        else
                        {
                            address = s[0].Split(':');
                        }
                        c.Client.Parser.JoinChannelsOnConnect = s.Length > 2 &&
                                                                s[1].Equals("-j",
                                                                            StringComparison.InvariantCultureIgnoreCase)
                                                                    ? s[2]
                                                                    : s.Length > 3 &&
                                                                      s[2].Equals("-j",
                                                                                  StringComparison.InvariantCultureIgnoreCase)
                                                                          ? s[3]
                                                                          : String.Empty;
                        break;
                }
            }
            if (address.Length == 2)
            {
                /* Look for a '+' to determine SSL */
                if (address[1][0] == '+')
                {
                    ssl = true;
                    address[1] = address[1].Substring(1);
                }
                if (!Int32.TryParse(address[1], out port))
                {
                    port = SettingsManager.Settings.Connection.Options.DefaultPort;
                }
            }
            /* If currently connected, we should send quit message */
            if (c.Client.IsConnecting)
            {
                c.Client.CancelConnection();
            }
            else if (c.Client.IsConnected)
            {
                c.Client.IsWaitingToReconnect = true;
                c.Client.Disconnect();
                c.Client.Server.Address = address[0];
                c.Client.Server.Port = port;
                c.Client.Server.IsSsl = ssl;
                TmrWaitToReconnectTimeOut.Tag = c.Client;
                TmrWaitToReconnectTimeOut.Enabled = true;
                return;
            }
            c.Client.Connect(address[0], port, ssl);
        }

        public static void ParseServerDisconnection(ClientConnection client)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            client.IsManualDisconnect = true;
            if (client.IsConnecting)
            {
                /* Cancel current connection */
                client.CancelConnection();
                return;
            }
            client.Disconnect();
        }

        /* Client waiting to reconnect after a disconnect */

        public static void OnClientWaitToReconnect(ClientConnection client)
        {
            /* This is called when the server command is issued on a currently connected server */
            if (TmrWaitToReconnectTimeOut.Enabled)
            {
                /* Event raised by connection class before time out timer fired */
                TmrWaitToReconnectTimeOut.Enabled = false;
            }
            ParseServerConnection(client,
                                  String.Format("{0}:{1}", client.Server.Address,
                                                client.Server.IsSsl
                                                    ? String.Format("+{0}", client.Server.Port.ToString())
                                                    : client.Server.Port.ToString()));
        }

        /* Timer callback */
        private static void TimerWaitToReconnectTimeOut(object sender, EventArgs e)
        {
            TmrWaitToReconnectTimeOut.Enabled = false;
            OnClientWaitToReconnect((ClientConnection)TmrWaitToReconnectTimeOut.Tag);
        }
    }
}
