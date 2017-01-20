/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms;
using FusionIRC.Helpers;
using ircCore.Settings.Theming;

namespace FusionIRC.Classes
{    
    public class MenubarControl
    {
        private readonly Form _owner;

        private readonly MenuStrip _menuBar;

        private readonly ToolStripMenuItem _mnuFile;
        private readonly ToolStripMenuItem _mnuWindow;

        public MenubarControl(Form owner)
        {
            _owner = owner;
            _menuBar = new MenuStrip
                           {
                               Dock = DockStyle.Top,
                               Size = new Size(657, 28),
                               RenderMode = ToolStripRenderMode.Professional,
                               GripStyle = ToolStripGripStyle.Hidden,
                               Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0)
                           };
            owner.Controls.Add(_menuBar);
            /* Menus */
            _mnuFile = new ToolStripMenuItem
                           {
                               Text = @"&File"
                           };
            _mnuFile.DropDownItems.AddRange(new ToolStripItem[]
                                                {
                                                    new ToolStripMenuItem("New window", null, OnMenuFileClick, Keys.Control | Keys.M),
                                                    new ToolStripSeparator(),
                                                    new ToolStripMenuItem("Connect to location...", null, OnMenuFileClick, Keys.F2),
                                                    new ToolStripSeparator(), 
                                                    new ToolStripMenuItem("Connect", null, OnMenuFileClick, Keys.F3),
                                                    new ToolStripMenuItem("Disconnect", null, OnMenuFileClick, Keys.F4),                                                    
                                                    new ToolStripSeparator(),
                                                    new ToolStripMenuItem("Exit", null, OnMenuFileClick, Keys.Alt | Keys.F4), 
                                                });
            _mnuWindow = new ToolStripMenuItem
                             {
                                 Text = @"&Window"
                             };
            _mnuWindow.DropDownItems.AddRange(new ToolStripItem[]
                                                  {
                                                      new ToolStripMenuItem("Find Text...", null, OnMenuWindowClick, Keys.Control | Keys.F)
                                                  });
            _menuBar.Items.AddRange(new[] {_mnuFile, _mnuWindow});
        }

        public void ConnectionUpdate(bool connected)
        {            
            _mnuFile.DropDownItems[4].Enabled = !connected;
            _mnuFile.DropDownItems[5].Enabled = connected;
        }

        /* Menu callbacks */
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
                    if (string.IsNullOrEmpty(c.Server.Address))
                    {
                        c.Server.Address = "irc.dragonirc.com"; //change this
                        c.Server.Port = 6667;
                        c.Server.IsSsl = false;
                    }
                    console = WindowManager.GetConsoleWindow(c);
                    if (console == null)
                    {
                        return;
                    }
                    CommandProcessor.Parse(c, console,
                                           string.Format("SERVER {0}:{1}", c.Server.Address,
                                                         c.Server.IsSsl
                                                             ? string.Format("+{0}", c.Server.Port.ToString())
                                                             : c.Server.Port.ToString()));
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
