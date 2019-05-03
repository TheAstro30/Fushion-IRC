/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms;
using FusionIRC.Forms.Child;
using FusionIRC.Helpers;
using ircCore.Controls;
using ircCore.Controls.Rendering;
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
            var renderer = new CustomRenderer(new Renderer());
            RenderMode = ToolStripRenderMode.Professional;
            Renderer = renderer;
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
            BuildWindowsMenu();
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
            /* This would seem strange to do, but as we already have a treeview displaying all open windows of the client,
             * it makes sense to iterate that and get the objects we need to display in the window list */
            _mnuWindow.DropDownItems.Clear();
            BuildWindowsMenu();
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
                    var w = WindowManager.AddWindow(null, ChildWindowType.Console, _owner, "Console", "Console", true);
                    if (w != null)
                    {
                        w.DisplayNode.Text = string.Format("{0}: {1}", "Console", w.Client.UserInfo.Nick);
                    }
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

        private void BuildWindowsMenu()
        {
            _mnuWindow.DropDownItems.AddRange(new ToolStripItem[]
                                                  {                                                       
                                                      new ToolStripMenuItem("Find Text...", null, OnMenuWindowClick, Keys.Control | Keys.F),
                                                      new ToolStripMenuItem("Font...", null, OnMenuWindowClick), 
                                                      new ToolStripSeparator(), 
                                                      new ToolStripMenuItem("Cascade", null, OnMenuWindowClick),
                                                      new ToolStripMenuItem("Tile Vertically", null, OnMenuWindowClick),
                                                      new ToolStripMenuItem("Tile Horizontally", null, OnMenuWindowClick)
                                                  });
            var count = 0;
            var active = (FrmChildWindow)_owner.ActiveMdiChild;
            foreach (TreeNode n in ((FrmClientWindow)_owner).SwitchView.Nodes)
            {
                count++;
                var child = (FrmChildWindow)n.Tag;
                if (child == null)
                {
                    continue;
                }
                if (count > 25)
                {
                    _mnuWindow.DropDownItems.AddRange(new ToolStripItem[]
                                                          {
                                                              new ToolStripSeparator(),
                                                              new ToolStripMenuItem("More windows...", null,
                                                                                    OnMenuWindowClick)
                                                          });
                    return;
                }
                _mnuWindow.DropDownItems.AddRange(new ToolStripItem[]
                                                      {
                                                          new ToolStripSeparator(),
                                                          new ToolStripMenuItem(Functions.TruncateString(child.Text, 100), null, OnMenuWindowClick)
                                                              {
                                                                  Tag = child, 
                                                                  Checked = child == active
                                                              }
                                                      });
                foreach (var c in from TreeNode w in n.Nodes from TreeNode c in w.Nodes select c)
                {
                    child = (FrmChildWindow)c.Tag;
                    if (child == null)
                    {
                        continue;
                    }
                    if (c.Tag is FrmChildWindow)
                    {
                        _mnuWindow.DropDownItems.Add(new ToolStripMenuItem(Functions.TruncateString(child.Text, 100), null, OnMenuWindowClick)
                        {
                            Tag = child,
                            Checked = child == active
                        });
                    }
                }
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

                case "MORE WINDOWS...":
                    break;

                default:
                    /* Open windows */
                    c = (FrmChildWindow) item.Tag;
                    if (c != null)
                    {
                        c.Activate();
                    }
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
