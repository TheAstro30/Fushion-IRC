/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using FusionIRC.Forms;
using FusionIRC.Forms.ChannelProperties;
using FusionIRC.Forms.Child;
using FusionIRC.Forms.DirectClientConnection.Helper;
using FusionIRC.Helpers.Commands;
using FusionIRC.Helpers.Connection;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Helpers
{
    public enum WindowEvent
    {
        None = 0,
        EventReceived = 1,
        MessageReceived = 2
    }

    internal static class WindowManager
    {
        public static Form MainForm { get; set; }

        /* If form is minimized and hidden, ActiveMDIChild will return NULL */
        public static FrmChildWindow LastActiveChild { get; set; }

        /* Channel properties window */
        public static FrmChannelProperties ChannelProperties { get; set; }

        /* Main child window list */
        public static Dictionary<ClientConnection, List<FrmChildWindow>> Windows = new Dictionary<ClientConnection, List<FrmChildWindow>>();

        public static FrmChildWindow AddWindow(ClientConnection client, ChildWindowType type, Form mdiOwner, string text, string tag, bool manualOpen)
        {
            FrmChildWindow win = null;            
            if (type == ChildWindowType.Console)
            {
                /* Create a new dictionary entry */
                var connection = new ClientConnection(mdiOwner, SettingsManager.Settings.UserInfo)
                                     {ConnectionId = GetHighestConnectionId()};
                win = new FrmChildWindow(connection, type, mdiOwner)
                          {
                              Text = string.Format("{0}: {1}", text, connection.UserInfo.Nick),
                              Tag = tag
                          };
                var wins = new List<FrmChildWindow>
                               {
                                   win
                               };
                Windows.Add(connection, wins);
                AddConnectionHandlers(connection);                
            }
            else
            {
                /* Get current dictionary object ... */
                if (client == null)
                {
                    return null;
                }
                var c = Windows.ContainsKey(client) ? Windows[client] : null;
                if (c != null)
                {
                    /* Check the window isn't all ready in the list */
                    win = c.FirstOrDefault(o => o.Tag.ToString().Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                    if (win != null)
                    {
                        return win;
                    }                    
                    /* Add the window to the list */
                    win = new FrmChildWindow(client, type, mdiOwner)
                              {
                                  Text = text,
                                  Tag = tag
                              };
                    c.Add(win);
                }
            }
            /* Show and return the new window */
            if (win != null)
            {                
                if (manualOpen)
                {
                    win.Show();
                }
                else
                {
                    win.ShowWithoutActive();
                }
                /* Add to tree view as well */
                ((FrmClientWindow)mdiOwner).SwitchView.AddWindow(win);
                if (win.DisplayNodeRoot != null)
                {                    
                    /* Update the window type count */
                    var count = GetChildWindowCount(win);
                    win.DisplayNodeRoot.Text = win.WindowType == ChildWindowType.Channel
                                                     ? string.Format("Channels ({0})", count)
                                                     : win.WindowType == ChildWindowType.Private
                                                           ? string.Format("Queries ({0})", count)
                                                           : win.WindowType == ChildWindowType.DccChat
                                                                 ? string.Format("Chats ({0})", count)
                                                                 : win.DisplayNodeRoot.Text;
                }
            }           
            return win;
        }

        public static void RemoveWindow(ClientConnection client, FrmChildWindow window)
        {
            /* Romoves a window from the dictionary object */
            if (!Windows.ContainsKey(client))
            {                
                return;
            }
            if (window.WindowType == ChildWindowType.Console)
            {
                RemoveConnectionHandlers(client);
                /* We should send disconnect command ... */
                Windows.Remove(client);
                ((FrmClientWindow)window.MdiParent).SwitchView.RemoveWindow(window);             
                return;
            }
            /* Else, we remove the child window from this current console connection */
            Windows[client].Remove(window);
            /* Update treeview */
            ((FrmClientWindow)window.MdiParent).SwitchView.RemoveWindow(window);
            if (window.DisplayNodeRoot == null)
            {
                return;
            }
            /* Update the window type count */
            var count = GetChildWindowCount(window);
            window.DisplayNodeRoot.Text = window.WindowType == ChildWindowType.Channel
                                              ? string.Format("Channels ({0})", count)
                                              : window.WindowType == ChildWindowType.Private
                                                    ? string.Format("Queries ({0})", count)
                                                    : window.WindowType == ChildWindowType.DccChat
                                                          ? string.Format("Chats ({0})", count)
                                                          : window.DisplayNodeRoot.Text;
        }

        public static void RemoveAllWindowsOfConsole(ClientConnection client)
        {
            if (!Windows.ContainsKey(client))
            {                
                return;
            }
            /* The first window is always the console - list must remain UNSORTED at all times */
            for (var i = Windows[client].Count - 1; i > 0; i--)
            {
                Windows[client][i].Close();
            }
        }

        public static int GetConsoleWindowIndex(ClientConnection client)
        {
            /* $cid */
            var i = 0;
            foreach (var w in Windows)
            {
                if (w.Key == client)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public static FrmChildWindow GetWindow(ClientConnection client, string tag)
        {
            var c = Windows.ContainsKey(client) ? Windows[client] : null;
            return c != null ? c.FirstOrDefault(o => o.Tag.ToString().Equals(tag, StringComparison.InvariantCultureIgnoreCase)) : null;
        }

        public static FrmChildWindow GetActiveWindow()
        {
            return (FrmChildWindow) MainForm.ActiveMdiChild ?? LastActiveChild;
        }

        public static FrmChildWindow GetConsoleWindow(ClientConnection client)
        {
            var c = Windows.ContainsKey(client) ? Windows[client] : null;
            return c == null ? null : c.FirstOrDefault(o => o.Tag.ToString().Equals("console", StringComparison.InvariantCultureIgnoreCase));
        }

        public static ClientConnection GetActiveConnection()
        {
            var win = GetActiveWindow();
            return win != null ? win.Client : null;
        }

        public static int GetChildWindowCount(FrmChildWindow child)
        {
            /* Get's the number of child windows that are of the same type */
            var c = Windows.ContainsKey(child.Client) ? Windows[child.Client] : null;
            return c == null ? 0 : c.Count(o => o.WindowType == child.WindowType);
        }

        public static void SetWindowEvent(FrmChildWindow child, Form owner, WindowEvent windowEvent)
        {
            if (child != null && child != owner.ActiveMdiChild)
            {
                child.CurrentWindowEvent = windowEvent;
            }
        }

        public static Form FindClientWindow(string text)
        {
            /* This will search for an instance of a form that's already open by its title (.Text) */
            return Application.OpenForms.Cast<Form>().FirstOrDefault(of => of.Text.Equals(text, StringComparison.InvariantCultureIgnoreCase));
        }

        /* Theme loaded callback */
        public static void OnThemeLoaded()
        {
            /* Here we now refresh all open chat windows ... */
            var c = (FrmClientWindow) MainForm;
            if (c == null)
            {
                return;
            }
            c.SwitchView.BackColor = ThemeManager.GetColor(ThemeColor.SwitchTreeBackColor);
            c.SwitchView.ForeColor = ThemeManager.GetColor(ThemeColor.SwitchTreeForeColor);
            foreach (var win in Windows.SelectMany(w => w.Value))
            {
                var fnt = ThemeManager.GetFont(win.WindowType);
                var bg = ThemeManager.GetBackground(win.WindowType);
                switch (win.WindowType)
                {
                    case ChildWindowType.Channel:
                        /* Nicklist */
                        win.Nicklist.Font = fnt;
                        win.Nicklist.BackColor = ThemeManager.GetColor(ThemeColor.NicklistBackColor);
                        win.Nicklist.ForeColor = ThemeManager.GetColor(ThemeColor.NicklistForeColor);
                        break;
                }
                /* Fonts */
                win.Output.Font = fnt;
                win.Input.Font = fnt;
                /* Backgrounds */
                if (!string.IsNullOrEmpty(bg.Path))
                {
                    var file = Functions.MainDir(bg.Path);
                    if (File.Exists(file))
                    {
                        win.Output.BackgroundImage = (Bitmap)Image.FromFile(file);
                        win.Output.BackgroundImageLayout = bg.LayoutStyle;
                    }
                }
                else
                {
                    win.Output.BackgroundImage = null;
                }
                /* Colors */
                win.Output.BackColor = ThemeManager.GetColor(ThemeColor.OutputWindowBackColor);
                win.Output.LineMarkerColor = ThemeManager.GetColor(ThemeColor.OutputWindowLineMarkerColor);
                win.Input.BackColor = ThemeManager.GetColor(ThemeColor.InputWindowBackColor);
                win.Input.ForeColor = ThemeManager.GetColor(ThemeColor.InputWindowForeColor);
                win.Refresh();
            }
        }

        /* Private methods */
        private static int GetHighestConnectionId()
        {
            /* This method checks the dictionary of ClientConnection.ConnectionId and returns the highest + 1 */
            return Windows.Count > 0 ? Windows.Max(t => t.Key.ConnectionId) + 1 : 1;
        }

        private static void AddConnectionHandlers(ClientConnection client)
        {
            /* Add the callback handlers to ConnectionCallbackManager */
            client.Parser.OnOther += Raw.OnOther;
            client.Parser.OnNetworkNameChanged += Misc.OnNetworkNameChanged;
            client.Parser.OnChannelModes += Modes.OnChannelModes;
            client.OnClientBeginConnect += SocketEvents.OnClientBeginConnect;
            client.OnClientCancelConnection += SocketEvents.OnClientCancelConnection;
            client.OnClientConnected += SocketEvents.OnClientConnected;
            client.OnClientDisconnected += SocketEvents.OnClientDisconnected;
            client.OnClientConnectionError += SocketEvents.OnClientConnectionError;
            client.OnClientConnectionClosed += CommandServer.OnClientWaitToReconnect;
            client.Parser.OnServerPingPong += Misc.OnServerPingPong;
            client.Parser.OnErrorLink += Misc.OnErrorLink;
            client.OnClientSslInvalidCertificate += SocketEvents.OnClientSslInvalidCertificate;
            client.OnClientLocalInfoResolved += LocalInfo.OnClientLocalInfoResolved;
            client.OnClientLocalInfoFailed += LocalInfo.OnClientLocalInfoFailed;
            client.OnClientDnsResolved += Dns.OnClientDnsResolved;
            client.OnClientDnsFailed += Dns.OnClientDnsFailed;
            client.OnClientIdentDaemonRequest += Misc.OnClientIdentDaemonRequest;
            client.Parser.OnMotd += Misc.OnMotd;
            client.Parser.OnLUsers += Misc.OnLUsers;
            client.Parser.OnWelcome += Misc.OnWelcome;
            client.Parser.OnTopicIs += Channel.OnTopicIs;
            client.Parser.OnTopicSetBy += Channel.OnTopicSetBy;
            client.Parser.OnTopicChanged += Channel.OnTopicChanged;
            client.Parser.OnJoinUser += Channel.OnJoinUser;
            client.Parser.OnJoinSelf += Channel.OnJoinSelf;
            client.Parser.OnPartUser += Channel.OnPartUser;
            client.Parser.OnPartSelf += Channel.OnPartSelf;
            client.Parser.OnNames += Channel.OnNames;
            client.Parser.OnWho += Channel.OnWho;
            client.Parser.OnTextChannel += Messages.OnTextChannel;
            client.Parser.OnTextSelf += Messages.OnTextSelf;
            client.Parser.OnActionChannel += Messages.OnActionChannel;
            client.Parser.OnActionSelf += Messages.OnActionSelf;            
            client.Parser.OnNotice += Messages.OnNotice;
            client.Parser.OnNick += Channel.OnNick;
            client.Parser.OnQuit += Channel.OnQuit;
            client.Parser.OnKickSelf += Channel.OnKickSelf;
            client.Parser.OnKickUser += Channel.OnKickUser;
            client.Parser.OnModeSelf += Modes.OnModeSelf;
            client.Parser.OnModeChannel += Modes.OnModeChannel;
            client.Parser.OnRaw += Raw.OnRaw;
            client.Parser.OnUserInfo += Misc.OnUserInfo;
            client.Parser.OnWallops += Messages.OnWallops;
            client.Parser.OnWhois += Misc.OnWhois;
            client.Parser.OnInvite += Channel.OnInvite;
            client.Parser.OnCtcp += Ctcp.OnCtcp;
            client.Parser.OnCtcpReply += Ctcp.OnCtcpReply;
            client.Parser.OnNotChannelOperator += Raw.OnNotChannelOperator;
            client.Parser.OnModeListData += Channel.OnModeListData;
            client.Parser.OnEndOfChannelProperties += Channel.OnEndOfChannelProperties;
            client.OnClientBeginQuit += Channel.OnBeginQuit;
            client.Parser.OnWatchOnline += Notify.OnWatchOnline;
            client.Parser.OnWatchOffline += Notify.OnWatchOffline;
            client.Parser.OnIson += Notify.OnIson;
            client.Parser.OnBeginChannelList += Channel.OnBeginChannelList;
            client.Parser.OnChannelListData += Channel.OnChannelListData;
            client.Parser.OnEndChannelList += Channel.OnEndChannelList;
            client.Parser.OnDccChat += DccManager.OnDccChat;
            client.Parser.OnDccSend += DccManager.OnDccSend;
            client.Parser.OnDccAcceptResume += DccManager.OnDccAcceptResume;
        }

        private static void RemoveConnectionHandlers(ClientConnection client)
        {
            /* Add the callback handlers to ConnectionCallbackManager */
            client.Parser.OnOther -= Raw.OnOther;
            client.Parser.OnNetworkNameChanged -= Misc.OnNetworkNameChanged;
            client.Parser.OnChannelModes -= Modes.OnChannelModes;
            client.OnClientBeginConnect -= SocketEvents.OnClientBeginConnect;
            client.OnClientCancelConnection -= SocketEvents.OnClientCancelConnection;
            client.OnClientConnected -= SocketEvents.OnClientConnected;
            client.OnClientDisconnected -= SocketEvents.OnClientDisconnected;
            client.OnClientConnectionError -= SocketEvents.OnClientConnectionError;
            client.OnClientConnectionClosed -= CommandServer.OnClientWaitToReconnect;
            client.OnClientSslInvalidCertificate -= SocketEvents.OnClientSslInvalidCertificate;
            client.OnClientLocalInfoResolved -= LocalInfo.OnClientLocalInfoResolved;
            client.OnClientLocalInfoFailed -= LocalInfo.OnClientLocalInfoFailed;
            client.OnClientDnsResolved -= Dns.OnClientDnsResolved;
            client.OnClientDnsFailed -= Dns.OnClientDnsFailed;
            client.OnClientIdentDaemonRequest -= Misc.OnClientIdentDaemonRequest;
            client.Parser.OnServerPingPong -= Misc.OnServerPingPong;
            client.Parser.OnErrorLink -= Misc.OnErrorLink;
            client.Parser.OnMotd -= Misc.OnMotd;
            client.Parser.OnLUsers -= Misc.OnLUsers;
            client.Parser.OnWelcome -= Misc.OnWelcome;
            client.Parser.OnTopicIs -= Channel.OnTopicIs;
            client.Parser.OnTopicSetBy -= Channel.OnTopicSetBy;
            client.Parser.OnJoinUser -= Channel.OnJoinUser;
            client.Parser.OnJoinSelf -= Channel.OnJoinSelf;
            client.Parser.OnPartUser -= Channel.OnPartUser;
            client.Parser.OnPartSelf -= Channel.OnPartSelf;
            client.Parser.OnNames -= Channel.OnNames;
            client.Parser.OnWho -= Channel.OnWho;
            client.Parser.OnTextChannel -= Messages.OnTextChannel;
            client.Parser.OnTextSelf -= Messages.OnTextSelf;
            client.Parser.OnActionChannel -= Messages.OnActionChannel;
            client.Parser.OnActionSelf -= Messages.OnActionSelf;            
            client.Parser.OnNotice -= Messages.OnNotice;
            client.Parser.OnNick -= Channel.OnNick;
            client.Parser.OnQuit -= Channel.OnQuit;
            client.Parser.OnKickSelf -= Channel.OnKickSelf;
            client.Parser.OnKickUser -= Channel.OnKickUser;
            client.Parser.OnModeSelf -= Modes.OnModeSelf;
            client.Parser.OnModeChannel -= Modes.OnModeChannel;
            client.Parser.OnRaw -= Raw.OnRaw;
            client.Parser.OnUserInfo -= Misc.OnUserInfo;
            client.Parser.OnWallops -= Messages.OnWallops;
            client.Parser.OnWhois -= Misc.OnWhois;
            client.Parser.OnInvite -= Channel.OnInvite;
            client.Parser.OnCtcp -= Ctcp.OnCtcp;
            client.Parser.OnCtcpReply -= Ctcp.OnCtcpReply;
            client.Parser.OnNotChannelOperator -= Raw.OnNotChannelOperator;
            client.Parser.OnModeListData -= Channel.OnModeListData;
            client.Parser.OnEndOfChannelProperties -= Channel.OnEndOfChannelProperties;
            client.OnClientBeginQuit -= Channel.OnBeginQuit;
            client.Parser.OnWatchOnline -= Notify.OnWatchOnline;
            client.Parser.OnWatchOffline -= Notify.OnWatchOffline;
            client.Parser.OnIson -= Notify.OnIson;
            client.Parser.OnBeginChannelList -= Channel.OnBeginChannelList;
            client.Parser.OnChannelListData -= Channel.OnChannelListData;
            client.Parser.OnEndChannelList -= Channel.OnEndChannelList;
            client.Parser.OnDccChat -= DccManager.OnDccChat;
            client.Parser.OnDccSend -= DccManager.OnDccSend;
            client.Parser.OnDccAcceptResume -= DccManager.OnDccAcceptResume;
        }
    }
}
