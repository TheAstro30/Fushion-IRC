/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms.Child;
using FusionIRC.Forms.Misc;
using FusionIRC.Helpers;
using ircClient;
using ircCore.Settings.Networks;

namespace FusionIRC.Controls.ControlBars
{
    /* Base class for both toolbar and menubar controls which avoids duplicate code */
    public abstract class ControlBar : MenuStrip
    {
        public virtual void ConnectToServer(ClientConnection client, FrmChildWindow console)
        {
            var server = !string.IsNullOrEmpty(client.Server.Address)
                             ? client.Server
                             : ServerManager.Servers.Recent.Server.Count > 0
                                   ? ServerManager.Servers.Recent.Server[0]
                                   : new Server
                                         {
                                             Address = "irc.dragonirc.com",
                                             Port = 6667
                                         };
            CommandProcessor.Parse(client, console,
                                   string.Format("SERVER {0}:{1}", server.Address,
                                                 server.IsSsl
                                                     ? string.Format("+{0}", server.Port)
                                                     : server.Port.ToString()));
        }

        public virtual void ConnectToLocation(Form owner, FrmChildWindow console)
        {
            using (var connect = new FrmConnectTo(owner, console))
            {
                connect.ShowDialog(owner);
            }
        }

        public virtual void DisconnectFromServer(ClientConnection client, FrmChildWindow console)
        {
            CommandProcessor.Parse(client, console, "DISCONNECT");
        }

        public virtual void ConnectToRecentServer(ClientConnection client, ToolStripMenuItem item)
        {
            if (item.Text.ToUpper() == "CLEAR")
            {
                ServerManager.Servers.Recent.Server.Clear();
                return;
            }
            var address = item.Text.Split(':');
            if (address.Length == 0)
            {
                return;
            }
            var server = ServerManager.Servers.Recent.Server.FirstOrDefault(s => s.Address.Equals(address[0], StringComparison.InvariantCultureIgnoreCase));
            if (server == null)
            {
                return;
            }
            var console = WindowManager.GetConsoleWindow(client);
            if (console == null)
            {
                return;
            }
            CommandProcessor.Parse(client, console,
                                   string.Format("SERVER {0}:{1}", server.Address,
                                                 server.IsSsl
                                                     ? string.Format("+{0}", server.Port)
                                                     : server.Port.ToString()));
        }
    }
}
