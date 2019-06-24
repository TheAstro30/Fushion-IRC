/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using FusionIRC.Forms;
using ircClient;
using ircCore.Settings.Theming;
using ircCore.Settings.Theming.Structures;

namespace FusionIRC.Helpers.Connection
{
    public static class Notify
    {
        public static void OnWatchOnline(ClientConnection client, string nick, string address)
        {
            /* User has come online, add to UI list */
            ((FrmClientWindow) WindowManager.MainForm).SwitchView.AddNotify(client, nick, address);
            /* Play sound */
            ThemeManager.PlaySound(ThemeSound.NotifyOnline);
        }

        public static void OnWatchOffline(ClientConnection client, string nick)
        {
            /* User buggered off, so we now need to remove them from the notify list */
            ((FrmClientWindow) WindowManager.MainForm).SwitchView.RemoveNotify(client, nick);
            /* Play sound */
            ThemeManager.PlaySound(ThemeSound.NotifyOffline);
        }

        public static void OnIson(ClientConnection client, string nick)
        {
            /* Used for servers that don't support the /WATCH command */
            System.Diagnostics.Debug.Print("SERVER ISON -> " + nick);
        }
    }
}
