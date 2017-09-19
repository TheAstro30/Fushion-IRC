/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using FusionIRC.Forms;
using FusionIRC.Forms.Child;
using ircClient;
using ircCore.Settings;
using ircCore.Settings.Theming;

namespace FusionIRC.Helpers
{
    public static class WindowManager
    {
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
                              Text = text,
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

        /* Theme loaded callback */
        public static void OnThemeLoaded()
        {
            System.Diagnostics.Debug.Print("Theme loaded " + ThemeManager.CurrentTheme.Name);
            /* Here we now refresh all open chat windows ... */
        }

        /* Private methods */
        private static void AddConnectionHandlers(ClientConnection client)
        {
            /* Add the callback handlers to ConnectionCallbackManager */
            client.Parser.OnOther += ConnectionCallbackManager.OnOther;
            client.OnClientBeginConnect += ConnectionCallbackManager.OnClientBeginConnect;
            client.OnClientCancelConnection += ConnectionCallbackManager.OnClientCancelConnection;
            client.OnClientConnected += ConnectionCallbackManager.OnClientConnected;
            client.OnClientDisconnected += ConnectionCallbackManager.OnClientDisconnected;
            client.OnClientConnectionError += ConnectionCallbackManager.OnClientConnectionError;
            client.OnClientConnectionClosed += CommandProcessor.OnClientWaitToReconnect;
            client.Parser.OnServerPingPong += ConnectionCallbackManager.OnServerPingPong;
            client.OnClientSslInvalidCertificate += ConnectionCallbackManager.OnClientSslInvalidCertificate;
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
            client.OnClientBeginConnect -= ConnectionCallbackManager.OnClientBeginConnect;
            client.OnClientCancelConnection -= ConnectionCallbackManager.OnClientCancelConnection;
            client.OnClientConnected -= ConnectionCallbackManager.OnClientConnected;
            client.OnClientDisconnected -= ConnectionCallbackManager.OnClientDisconnected;
            client.OnClientConnectionError -= ConnectionCallbackManager.OnClientConnectionError;
            client.OnClientConnectionClosed -= CommandProcessor.OnClientWaitToReconnect;
            client.OnClientSslInvalidCertificate -= ConnectionCallbackManager.OnClientSslInvalidCertificate;
            client.Parser.OnServerPingPong -= ConnectionCallbackManager.OnServerPingPong;
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
            client.Parser.OnWallops -= ConnectionCallbackManager.OnWallops;
            client.Parser.OnWhois -= ConnectionCallbackManager.OnWhois;
            client.Parser.OnInvite -= ConnectionCallbackManager.OnInvite;
            client.Parser.OnCtcp -= ConnectionCallbackManager.OnCtcp;
            client.Parser.OnCtcpReply -= ConnectionCallbackManager.OnCtcpReply;
        }
    }
}
