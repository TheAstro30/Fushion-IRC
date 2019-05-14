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
using FusionIRC.Forms.Child;
using FusionIRC.Forms.Misc;
using FusionIRC.Helpers.Commands;
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

    public static class WindowManager
    {
        /* DCC file transfer manager window */
        public static FrmDccManager DccManagerWindow;

        /* Main child window list */
        public static Dictionary<ClientConnection, List<FrmChildWindow>> Windows = new Dictionary<ClientConnection, List<FrmChildWindow>>();

        public static FrmChildWindow AddWindow(ClientConnection client, ChildWindowType type, Form mdiOwner, string text, string tag, bool manualOpen)
        {
            FrmChildWindow win = null;            
            if (type == ChildWindowType.Console)
            {
                /* Create a new dictionary entry */
                var connection = new ClientConnection(mdiOwner, SettingsManager.Settings.UserInfo);
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
            if (window.DisplayNodeRoot != null)
            {
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

        public static FrmChildWindow GetWindow(ClientConnection client, string tag)
        {
            var c = Windows.ContainsKey(client) ? Windows[client] : null;
            return c != null ? c.FirstOrDefault(o => o.Tag.ToString().Equals(tag, StringComparison.InvariantCultureIgnoreCase)) : null;
        }

        public static FrmChildWindow GetActiveWindow(Form owner)
        {
            return (FrmChildWindow) owner.ActiveMdiChild;
        }

        public static FrmChildWindow GetConsoleWindow(ClientConnection client)
        {
            var c = Windows.ContainsKey(client) ? Windows[client] : null;
            return c == null ? null : c.FirstOrDefault(o => o.Tag.ToString().Equals("console", StringComparison.InvariantCultureIgnoreCase));
        }

        public static ClientConnection GetActiveConnection(Form owner)
        {
            var win = (FrmChildWindow) owner.ActiveMdiChild;
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
            var c = (FrmClientWindow) ConnectionCallbackManager.MainForm;
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
                win.Output.ForeColor = ThemeManager.GetColor(ThemeColor.OutputWindowForeColor);
                win.Input.BackColor = ThemeManager.GetColor(ThemeColor.InputWindowBackColor);
                win.Input.ForeColor = ThemeManager.GetColor(ThemeColor.InputWindowForeColor);
                win.Refresh();
            }
        }

        /* Private methods */
        private static void AddConnectionHandlers(ClientConnection client)
        {
            /* Add the callback handlers to ConnectionCallbackManager */
            client.Parser.OnOther += ConnectionCallbackManager.OnOther;
            client.Parser.OnNetworkNameChanged += ConnectionCallbackManager.OnNetworkNameChanged;
            client.Parser.OnChannelModes += ConnectionCallbackManager.OnChannelModes;
            client.OnClientBeginConnect += ConnectionCallbackManager.OnClientBeginConnect;
            client.OnClientCancelConnection += ConnectionCallbackManager.OnClientCancelConnection;
            client.OnClientConnected += ConnectionCallbackManager.OnClientConnected;
            client.OnClientDisconnected += ConnectionCallbackManager.OnClientDisconnected;
            client.OnClientConnectionError += ConnectionCallbackManager.OnClientConnectionError;
            client.OnClientConnectionClosed += CommandServer.OnClientWaitToReconnect;
            client.Parser.OnServerPingPong += ConnectionCallbackManager.OnServerPingPong;
            client.Parser.OnErrorLink += ConnectionCallbackManager.OnErrorLink;
            client.OnClientSslInvalidCertificate += ConnectionCallbackManager.OnClientSslInvalidCertificate;
            client.OnClientLocalInfoResolved += ConnectionCallbackManager.OnClientLocalInfoResolved;
            client.OnClientLocalInfoFailed += ConnectionCallbackManager.OnClientLocalInfoFailed;
            client.OnClientDnsResolved += ConnectionCallbackManager.OnClientDnsResolved;
            client.OnClientDnsFailed += ConnectionCallbackManager.OnClientDnsFailed;
            client.OnClientIdentDaemonRequest += ConnectionCallbackManager.OnClientIdentDaemonRequest;
            client.Parser.OnMotd += ConnectionCallbackManager.OnMotd;
            client.Parser.OnLUsers += ConnectionCallbackManager.OnLUsers;
            client.Parser.OnWelcome += ConnectionCallbackManager.OnWelcome;
            client.Parser.OnTopicIs += ConnectionCallbackManager.OnTopicIs;
            client.Parser.OnTopicSetBy += ConnectionCallbackManager.OnTopicSetBy;
            client.Parser.OnTopicChanged += ConnectionCallbackManager.OnTopicChanged;
            client.Parser.OnJoinUser += ConnectionCallbackManager.OnJoinUser;
            client.Parser.OnJoinSelf += ConnectionCallbackManager.OnJoinSelf;
            client.Parser.OnPartUser += ConnectionCallbackManager.OnPartUser;
            client.Parser.OnPartSelf += ConnectionCallbackManager.OnPartSelf;
            client.Parser.OnNames += ConnectionCallbackManager.OnNames;
            client.Parser.OnWho += ConnectionCallbackManager.OnWho;
            client.Parser.OnTextChannel += ConnectionCallbackManager.OnTextChannel;
            client.Parser.OnTextSelf += ConnectionCallbackManager.OnTextSelf;
            client.Parser.OnActionChannel += ConnectionCallbackManager.OnActionChannel;
            client.Parser.OnActionSelf += ConnectionCallbackManager.OnActionSelf;            
            client.Parser.OnNotice += ConnectionCallbackManager.OnNotice;
            client.Parser.OnNick += ConnectionCallbackManager.OnNick;
            client.Parser.OnQuit += ConnectionCallbackManager.OnQuit;
            client.Parser.OnKickSelf += ConnectionCallbackManager.OnKickSelf;
            client.Parser.OnKickUser += ConnectionCallbackManager.OnKickUser;
            client.Parser.OnModeSelf += ConnectionCallbackManager.OnModeSelf;
            client.Parser.OnModeChannel += ConnectionCallbackManager.OnModeChannel;
            client.Parser.OnRaw += ConnectionCallbackManager.OnRaw;
            client.Parser.OnUserInfo += ConnectionCallbackManager.OnUserInfo;
            client.Parser.OnWallops += ConnectionCallbackManager.OnWallops;
            client.Parser.OnWhois += ConnectionCallbackManager.OnWhois;
            client.Parser.OnInvite += ConnectionCallbackManager.OnInvite;
            client.Parser.OnCtcp += ConnectionCallbackManager.OnCtcp;
            client.Parser.OnCtcpReply += ConnectionCallbackManager.OnCtcpReply;
        }

        private static void RemoveConnectionHandlers(ClientConnection client)
        {
            /* Add the callback handlers to ConnectionCallbackManager */
            client.Parser.OnOther -= ConnectionCallbackManager.OnOther;
            client.Parser.OnNetworkNameChanged -= ConnectionCallbackManager.OnNetworkNameChanged;
            client.Parser.OnChannelModes -= ConnectionCallbackManager.OnChannelModes;
            client.OnClientBeginConnect -= ConnectionCallbackManager.OnClientBeginConnect;
            client.OnClientCancelConnection -= ConnectionCallbackManager.OnClientCancelConnection;
            client.OnClientConnected -= ConnectionCallbackManager.OnClientConnected;
            client.OnClientDisconnected -= ConnectionCallbackManager.OnClientDisconnected;
            client.OnClientConnectionError -= ConnectionCallbackManager.OnClientConnectionError;
            client.OnClientConnectionClosed -= CommandServer.OnClientWaitToReconnect;
            client.OnClientSslInvalidCertificate -= ConnectionCallbackManager.OnClientSslInvalidCertificate;
            client.OnClientLocalInfoResolved -= ConnectionCallbackManager.OnClientLocalInfoResolved;
            client.OnClientLocalInfoFailed -= ConnectionCallbackManager.OnClientLocalInfoFailed;
            client.OnClientDnsResolved -= ConnectionCallbackManager.OnClientDnsResolved;
            client.OnClientDnsFailed -= ConnectionCallbackManager.OnClientDnsFailed;
            client.OnClientIdentDaemonRequest -= ConnectionCallbackManager.OnClientIdentDaemonRequest;
            client.Parser.OnServerPingPong -= ConnectionCallbackManager.OnServerPingPong;
            client.Parser.OnErrorLink -= ConnectionCallbackManager.OnErrorLink;
            client.Parser.OnMotd -= ConnectionCallbackManager.OnMotd;
            client.Parser.OnLUsers -= ConnectionCallbackManager.OnLUsers;
            client.Parser.OnWelcome -= ConnectionCallbackManager.OnWelcome;
            client.Parser.OnTopicIs -= ConnectionCallbackManager.OnTopicIs;
            client.Parser.OnTopicSetBy -= ConnectionCallbackManager.OnTopicSetBy;
            client.Parser.OnJoinUser -= ConnectionCallbackManager.OnJoinUser;
            client.Parser.OnJoinSelf -= ConnectionCallbackManager.OnJoinSelf;
            client.Parser.OnPartUser -= ConnectionCallbackManager.OnPartUser;
            client.Parser.OnPartSelf -= ConnectionCallbackManager.OnPartSelf;
            client.Parser.OnNames -= ConnectionCallbackManager.OnNames;
            client.Parser.OnWho -= ConnectionCallbackManager.OnWho;
            client.Parser.OnTextChannel -= ConnectionCallbackManager.OnTextChannel;
            client.Parser.OnTextSelf -= ConnectionCallbackManager.OnTextSelf;
            client.Parser.OnActionChannel -= ConnectionCallbackManager.OnActionChannel;
            client.Parser.OnActionSelf -= ConnectionCallbackManager.OnActionSelf;            
            client.Parser.OnNotice -= ConnectionCallbackManager.OnNotice;
            client.Parser.OnNick -= ConnectionCallbackManager.OnNick;
            client.Parser.OnQuit -= ConnectionCallbackManager.OnQuit;
            client.Parser.OnKickSelf -= ConnectionCallbackManager.OnKickSelf;
            client.Parser.OnKickUser -= ConnectionCallbackManager.OnKickUser;
            client.Parser.OnModeSelf -= ConnectionCallbackManager.OnModeSelf;
            client.Parser.OnModeChannel -= ConnectionCallbackManager.OnModeChannel;
            client.Parser.OnRaw -= ConnectionCallbackManager.OnRaw;
            client.Parser.OnUserInfo -= ConnectionCallbackManager.OnUserInfo;
            client.Parser.OnWallops -= ConnectionCallbackManager.OnWallops;
            client.Parser.OnWhois -= ConnectionCallbackManager.OnWhois;
            client.Parser.OnInvite -= ConnectionCallbackManager.OnInvite;
            client.Parser.OnCtcp -= ConnectionCallbackManager.OnCtcp;
            client.Parser.OnCtcpReply -= ConnectionCallbackManager.OnCtcpReply;
        }
    }
}
