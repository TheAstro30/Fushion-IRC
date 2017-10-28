/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Child;
using FusionIRC.Helpers;
using ircCore.Forms;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Controls.ControlBars
{    
    public sealed class MenubarControl : ControlBar
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
                                                      new ToolStripMenuItem("Find Text...", null, OnMenuWindowClick, Keys.Control | Keys.F),
                                                      new ToolStripMenuItem("Font...", null, OnMenuWindowClick), 
                                                      new ToolStripSeparator(), 
                                                      new ToolStripMenuItem("Cascade", null, OnMenuWindowClick),
                                                      new ToolStripMenuItem("Tile Vertically", null, OnMenuWindowClick),
                                                      new ToolStripMenuItem("Tile Horizontally", null, OnMenuWindowClick)
                                                  });
            _mnuWindow.DropDownOpening += OnMenuWindowDropDownOpening;
            /* Add all menus */
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

        private void OnMenuWindowDropDownOpening(object sender, EventArgs e)
        {
            /* We would list all open windows here (up to a certain amount), then provide a separate dialog showing
             * all the rest */
        }

        private void OnMenuFileClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem) sender;
            var c = WindowManager.GetActiveConnection(_owner);
            if (c == null || item == null)
            {
                return;
            }
            var console = WindowManager.GetConsoleWindow(c);
            if (console == null)
            {
                return;
            }
            switch (item.Text.ToUpper())
            {
                case "NEW WINDOW":
                    WindowManager.AddWindow(null, ChildWindowType.Console, _owner, "Console", "Console", true);
                    break;

                case "CONNECT TO LOCATION...":
                    ConnectToLocation(_owner, console);
                    break;    

                case "CONNECT":                    
                    ConnectToServer(c, console);
                    break;

                case "DISCONNECT":
                    DisconnectFromServer(c, console);
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
            ConnectToRecentServer(c, item);
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

                case "FONT...":
                    ChangeFont(c);
                    break;

                case "CASCADE":
                    _owner.LayoutMdi(MdiLayout.Cascade);
                    break;

                case "TILE VERTICALLY":
                    _owner.LayoutMdi(MdiLayout.TileVertical);
                    break;

                case "TILE HORIZONTALLY":
                    _owner.LayoutMdi(MdiLayout.TileHorizontal);
                    break;
            }
        }

        /* Private methods */
        private void ChangeFont(FrmChildWindow c)
        {
            var def = string.Format("Set as default {0} window font",Functions.EnumUtils.GetDescriptionFromEnumValue(c.WindowType).ToLower());
            using (var font = new FrmFont { SelectedFont = c.Output.Font, SelectedFontDefaultText = def })
            {
                if (font.ShowDialog(_owner) == DialogResult.Cancel)
                {
                    return;
                }
                /* Let's change the font for the current window */
                ChangeFont(c, font.SelectedFont);
                if (font.SelectedFontDefault)
                {
                    /* We now iterate all open windows and change the font of the same type */
                    foreach (var win in WindowManager.Windows[c.Client])
                    {
                        if (win.WindowType == c.WindowType && win != c)
                        {
                            ChangeFont(win, font.SelectedFont);
                        }
                    }
                }
                /* Update theme settings */
                ThemeManager.CurrentTheme.ThemeFonts[c.WindowType] = font.SelectedFont;
                ThemeManager.CurrentTheme.ThemeChanged = true;
            }
        }

        private static void ChangeFont(FrmChildWindow c, Font font)
        {
            c.Output.Font = font;
            c.Input.Font = font;
            if (c.WindowType == ChildWindowType.Channel)
            {
                c.Nicklist.Font = font;
            }
            c.Refresh();
        }
    }
}
