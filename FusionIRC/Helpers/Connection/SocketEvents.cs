/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using FusionIRC.Forms.Warning;
using ircClient;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircScript.Classes.Structures;

namespace FusionIRC.Helpers.Connection
{
    internal static class SocketEvents
    {
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
            /* Update title bar of this console window */
            var net = !string.IsNullOrEmpty(client.Network) ? client.Network : client.Server.Address;
            c.Text = string.Format("{0}: {1} ({2}:{3})", net, client.UserInfo.Nick, client.Server.Address,
                                   client.Server.Port);
            c.DisplayNode.Text = string.Format("{0}: {1} ({2})", net, client.UserInfo.Nick, client.Server.Address);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
            /* Process event script */
            var e = new ScriptArgs
                        {
                            ChildWindow = c,
                            ClientConnection = client
                        };
            Events.Execute("connect", e);
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
            c.Reconnect.Cancel();
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
            /* Iterate all open channels and clear nick list (or close it's window) */
            Misc.UpdateChannelsOnDisconnect(client, pmd);
            client.UserInfo.AlternateUsed = false;
            /* Notification */
            ((TrayIcon) WindowManager.MainForm).ShowNotificationPopup(client.Network, "Disconnect from network", 50);
            if (client.IsManualDisconnect)
            {
                client.IsManualDisconnect = false;
                return;
            }
            /* Now we process re-connection code if the server wasn't manually disconnected by the user */
            c.Reconnect.BeginReconnect();
            /* Process event script */
            var e = new ScriptArgs
                        {
                            ChildWindow = c,
                            ClientConnection = client
                        };
            Events.Execute("disconnect", e);
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
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
            /* Iterate all open channels and clear nick list (or close it's window) */
            if (!client.IsConnecting)
            {
                Misc.UpdateChannelsOnDisconnect(client, pmd);
            }
            client.UserInfo.AlternateUsed = false;
            /* Notification */
            ((TrayIcon) WindowManager.MainForm).ShowNotificationPopup(client.Network, "Disconnect from network", 50);
            if (client.IsManualDisconnect)
            {
                client.IsManualDisconnect = false;
                return;
            }            
            /* Now we process re-connection code if the server wasn't manually disconnected by the user */
            c.Reconnect.BeginReconnect();
            /* Process event script */
            var e = new ScriptArgs
                        {
                            ChildWindow = c,
                            ClientConnection = client
                        };
            Events.Execute("disconnect", e);
        }

        public static void OnClientSslInvalidCertificate(ClientConnection client, X509Certificate certificate)
        {
            if (!SettingsManager.Settings.Connection.SslAcceptRequests)
            {
                return;
            }
            var store = new X509Store("FusionIRC", StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            if (store.Certificates.Cast<X509Certificate2>().Any(cert => cert.Subject == certificate.Subject))
            {
                client.SslAcceptCertificate(true);
                store.Close();
                return;
            }
            if (!SettingsManager.Settings.Connection.SslAutoAcceptInvalidCertificates)
            {
                using (var ssl = new FrmSslError())
                {
                    ssl.Server = client.Server.Address;
                    ssl.CertificateStore = store;
                    ssl.Certificate = certificate;
                    if (ssl.ShowDialog(WindowManager.MainForm) == DialogResult.Cancel)
                    {
                        client.SslAcceptCertificate(false);
                        store.Close();
                        return;
                    }
                }
            }
            store.Close();
            client.SslAcceptCertificate(true);
        }
    }
}
