/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using FusionIRC.Forms;
using ircClient;

namespace FusionIRC.Helpers.Connection
{
    public static class Notify
    {
        public static void OnWatchOnline(ClientConnection client, string nick, string address)
        {
            /* User has come online, add to UI list */
            ((FrmClientWindow) WindowManager.MainForm).SwitchView.AddNotify(client, nick, address);
        }

        public static void OnWatchOffline(ClientConnection client, string nick)
        {
            /* User buggered off, so we now need to remove them from the notify list */
            ((FrmClientWindow) WindowManager.MainForm).SwitchView.RemoveNotify(client, nick);
        }
    }
}
