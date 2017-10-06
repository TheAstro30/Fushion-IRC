/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms.Child;
using FusionIRC.Forms.Misc;
using FusionIRC.Helpers;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;

namespace FusionIRC.Controls
{    
    public sealed class MenubarControl : MenuStrip
    {
        private readonly Form _owner;

        private readonly ToolStripMenuItem _mnuFile;
        private readonly ToolStripMenuItem _mnuWindow;

        public MenubarControl(Form owner)
        {
            _owner = owner;
            RenderMode = ToolStripRenderMode.Professional;
            GripStyle = ToolStripGripStyle.Visible;
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Tag = "MENUBAR";
            /* Menus */
            _mnuFile = new ToolStripMenuItem
                           {
                               Text = @"&File"
                           };
            _mnuFile.DropDownItems.AddRange(new ToolStripItem[]
                                                {
                                                    new ToolStripMenuItem("New window", null, OnMenuFileClick, Keys.Control | Keys.M),
                                                    new ToolStripMenuItem("Recent"), 
                                                    new ToolStripSeparator(),
                                                    new ToolStripMenuItem("Connect to location...", null, OnMenuFileClick, Keys.F2),
                                                    new ToolStripSeparator(), 
                                                    new ToolStripMenuItem("Connect", null, OnMenuFileClick, Keys.F3),
                                                    new ToolStripMenuItem("Disconnect", null, OnMenuFileClick, Keys.F4),                                                    
                                                    new ToolStripSeparator(),
                                                    new ToolStripMenuItem("Exit", null, OnMenuFileClick, Keys.Alt | Keys.F4)
                                                });
            _mnuFile.DropDownOpening += OnMenuFileDropDownOpening;            
            _mnuWindow = new ToolStripMenuItem
                             {
                                 Text = @"&Window"
                             };
            _mnuWindow.DropDownItems.AddRange(new ToolStripItem[]
                                                  {
                                                      new ToolStripMenuItem("Find Text...", null, OnMenuWindowClick, Keys.Control | Keys.F)
                                                  });            
            Items.AddRange(new[] { _mnuFile, _mnuWindow });                        
        }

        public void ConnectionUpdate(bool connected)
        {            
            _mnuFile.DropDownItems[5].Enabled = !connected;
            _mnuFile.DropDownItems[6].Enabled = connected;
        }

        /* Menu callbacks */
        private void OnMenuFileDropDownOpening(object sender, EventArgs e)
        {
            if (ServerManager.Servers.Recent.Server.Count == 0)
            {
                _mnuFile.DropDownItems[1].Visible = false;
                return;
            }
            /* Populate the recent list */
            _mnuFile.DropDownItems[1].Visible = true;
            var itm = (ToolStripMenuItem)_mnuFile.DropDownItems[1];
            if (itm == null)
            {
                return;
            }
            itm.DropDownItems.Clear();
            foreach (var s in ServerManager.Servers.Recent.Server)
            {                
                itm.DropDownItems.Add(s.ToString(), null, OnMenuRecentServerClick);
            }
            itm.DropDownItems.AddRange(new ToolStripItem[]
                                           {
                                               new ToolStripSeparator(),
                                               new ToolStripMenuItem("Clear", null, OnMenuRecentServerClick)
                                           });
        }

        private void OnMenuFileClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem) sender;
            var c = WindowManager.GetActiveConnection(_owner);
            if (c == null || item == null)
            {
                return;
            }
            FrmChildWindow console;            
            switch (item.Text.ToUpper())
            {
                case "NEW WINDOW":
                    WindowManager.AddWindow(null, ChildWindowType.Console, _owner, "Console", "Console", true);
                    break;

                case "CONNECT TO LOCATION...":
                    using (var connect = new FrmConnectTo(_owner))
                    {
                        connect.ShowDialog(_owner);
                    }
                    break;    

                case "CONNECT":
                    var server = ServerManager.Servers.Recent.Server.Count > 0
                                     ? ServerManager.Servers.Recent.Server[0]
                                     : new Server
                                           {
                                               Address = "irc.dragonirc.com",
                                               Port = 6667
                                           };
                    console = WindowManager.GetConsoleWindow(c);
                    if (console == null)
                    {
                        return;
                    }
                    CommandProcessor.Parse(c, console,
                                           string.Format("SERVER {0}:{1}", server.Address,
                                                         server.IsSsl
                                                             ? string.Format("+{0}", server.Port)
                                                             : server.Port.ToString()));
                    break;

                case "DISCONNECT":
                    console = WindowManager.GetConsoleWindow(c);
                    if (console == null)
                    {
                        return;
                    }
                    CommandProcessor.Parse(c, console, "DISCONNECT");
                    break;

                case "EXIT":
                    Application.Exit();
                    break;
            }
        }

        private void OnMenuRecentServerClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem) sender;
            var c = WindowManager.GetActiveConnection(_owner);
            if (c == null || item == null)
            {
                return;
            }
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
            var console = WindowManager.GetConsoleWindow(c);
            if (console == null)
            {
                return;
            }
            CommandProcessor.Parse(c, console,
                                   string.Format("SERVER {0}:{1}", server.Address,
                                                 server.IsSsl
                                                     ? string.Format("+{0}", server.Port)
                                                     : server.Port.ToString()));
        }

        private void OnMenuWindowClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var c = WindowManager.GetActiveWindow(_owner);
            if (c == null || item == null)
            {
                return;
            }
            switch (item.Text.ToUpper())
            {
                case "FIND TEXT...":
                    using (var f = new FrmChatFind(c))
                    {                                              
                        f.ShowDialog(_owner);
                    }
                    break;
            }
        }
    }
}
