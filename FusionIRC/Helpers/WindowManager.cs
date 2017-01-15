/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using FusionIRC.Forms;
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
                    win = c.FirstOrDefault(o => o.Tag.ToString().ToLower() == tag.ToLower());
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
            }
            return win;
        }

        public static void RemoveWindow(ClientConnection client, FrmChildWindow window)
        {
            /* Romoves a window from the dictionary object */
            if (!Windows.ContainsKey(client))
            {
                System.Diagnostics.Debug.Print("Remove Failed");
                return;
            }
            if (window.WindowType == ChildWindowType.Console)
            {
                RemoveConnectionHandlers(client);
                /* We should send disconnect command ... */
                Windows.Remove(client);
                System.Diagnostics.Debug.Print("Removed console window");
                return;
            }
            /* Else, we remove the child window from this current console connection */
            Windows[client].Remove(window);
            System.Diagnostics.Debug.Print("Removed child window");
        }

        public static void RemoveAllWindowsOfConsole(ClientConnection client)
        {
            if (!Windows.ContainsKey(client))
            {
                System.Diagnostics.Debug.Print("Remove Failed");
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
            return c != null ? c.FirstOrDefault(o => o.Tag.ToString().ToLower() == tag.ToLower()) : null;
        }

        public static FrmChildWindow GetConsoleWindow(ClientConnection client)
        {
            var c = Windows.ContainsKey(client) ? Windows[client] : null;
            if (c == null)
            {
                return null;
            }
            return c.FirstOrDefault(o => o.Tag.ToString().ToLower() == "console");
        }

        public static ClientConnection GetActiveConnection(Form owner)
        {
            var win = (FrmChildWindow)owner.ActiveMdiChild;
            return win != null ? win.Client : null;
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
            client.OnDebugOut += ConnectionCallbackManager.OnDebugOut;
            client.Parser.OnJoinUser += ConnectionCallbackManager.OnJoinUser;
            client.Parser.OnJoinSelf += ConnectionCallbackManager.OnJoinSelf;
            client.Parser.OnPartUser += ConnectionCallbackManager.OnPartUser;
            client.Parser.OnNames += ConnectionCallbackManager.OnNames;
            client.Parser.OnTextChannel += ConnectionCallbackManager.OnTextChannel;
            client.Parser.OnActionChannel += ConnectionCallbackManager.OnActionChannel;
            client.Parser.OnNick += ConnectionCallbackManager.OnNick;
        }

        private static void RemoveConnectionHandlers(ClientConnection client)
        {
            /* Add the callback handlers to ConnectionCallbackManager */
            client.OnDebugOut -= ConnectionCallbackManager.OnDebugOut;
            client.Parser.OnJoinUser -= ConnectionCallbackManager.OnJoinUser;
            client.Parser.OnJoinSelf -= ConnectionCallbackManager.OnJoinSelf;
            client.Parser.OnPartUser -= ConnectionCallbackManager.OnPartUser;
            client.Parser.OnNames -= ConnectionCallbackManager.OnNames;
            client.Parser.OnTextChannel -= ConnectionCallbackManager.OnTextChannel;
            client.Parser.OnActionChannel -= ConnectionCallbackManager.OnActionChannel;
            client.Parser.OnNick -= ConnectionCallbackManager.OnNick;
        }
    }
}
