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

namespace FusionIRC.Classes
{    
    public class MenubarControl
    {
        private readonly Form _owner;

        private readonly MenuStrip _menuBar;

        private readonly ToolStripMenuItem _mnuFile;

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
                                                    new ToolStripMenuItem("Connect", null, OnMenuFileClick),
                                                    new ToolStripMenuItem("Disconnect", null, OnMenuFileClick),                                                    
                                                    new ToolStripSeparator(), new ToolStripMenuItem("Exit",null,OnMenuFileClick, Keys.Alt | Keys.F4), 
                                                });
            _menuBar.Items.Add(_mnuFile);
        }

        public void ConnectionUpdate(bool connected)
        {
            _mnuFile.DropDownItems[0].Enabled = !connected;
            _mnuFile.DropDownItems[1].Enabled = connected;
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
                case "CONNECT":
                    if (string.IsNullOrEmpty(c.Server))
                    {
                        c.Server = "irc.dragonirc.com"; //change this
                        c.Port = 6667;
                    }
                    console = WindowManager.GetConsoleWindow(c);
                    if (console == null)
                    {
                        return;
                    }
                    CommandProcessor.Parse(c, console, string.Format("SERVER {0}:{1}", c.Server, c.Port));
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
    }
}
