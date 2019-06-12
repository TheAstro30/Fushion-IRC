/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms;
using FusionIRC.Forms.Child;
using FusionIRC.Forms.Favorites;
using FusionIRC.Forms.Favorites.Editing;
using FusionIRC.Forms.Misc;
using FusionIRC.Helpers;
using ircCore.Controls.Rendering;
using ircCore.Forms;
using ircCore.Settings.Channels;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Controls.ControlBars
{    
    public sealed class MenubarControl : ControlBar
    {
        private readonly Form _owner;

        private readonly ToolStripMenuItem _mnuFile;
        private readonly ToolStripMenuItem _mnuFavorites;
        private readonly ToolStripMenuItem _mnuWindow;
        private readonly ToolStripMenuItem _mnuHelp;

        public ToolStripMenuItem MenuCommands = new ToolStripMenuItem("Commands");

        public MenubarControl(Form owner)
        {
            _owner = owner;
            var renderer = new CustomRenderer(new Renderer());
            RenderMode = ToolStripRenderMode.Professional;
            Renderer = renderer;
            GripStyle = ToolStripGripStyle.Visible;
            Font = new Font("Segoe UI", 9, FontStyle.Regular, GraphicsUnit.Point, 0);
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
            _mnuFavorites = new ToolStripMenuItem
                                {
                                    Text = @"&Favorites"
                                };                        
            _mnuWindow = new ToolStripMenuItem
                             {
                                 Text = @"&Window"
                             };

            _mnuHelp = new ToolStripMenuItem
                           {
                               Text = @"&Help"
                           };
            _mnuHelp.DropDownItems.AddRange(new ToolStripItem[]
                                                {
                                                    new ToolStripMenuItem("Show Help", null, OnMenuHelpClick, Keys.F1),
                                                    new ToolStripSeparator(),
                                                    new ToolStripMenuItem("About FusionIRC...", null, OnMenuFileClick)
                                                });
            BuildFavoritesMenu();
            _mnuFavorites.DropDownOpening += OnMenuFavoritesDropDownOpening;
            BuildWindowsMenu();
            _mnuWindow.DropDownOpening += OnMenuWindowDropDownOpening;
            /* Add all menus */
            Items.AddRange(new[] { _mnuFile, _mnuFavorites, MenuCommands, _mnuWindow, _mnuHelp });           
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

        private void OnMenuFavoritesDropDownOpening(object sender, EventArgs e)
        {
            _mnuFavorites.DropDownItems.Clear();
            BuildFavoritesMenu();
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
            var c = WindowManager.GetActiveConnection();
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
            var c = WindowManager.GetActiveConnection();
            if (c == null || item == null)
            {
                return;
            }
            ConnectToRecentServer(c, item);
        }

        private void BuildFavoritesMenu()
        {
            var r = ChannelManager.Channels.Recent.Channels;
            var channels = new ToolStripMenuItem("Recent channels...");
            if (r.Count > 0)
            {
                foreach (var n in r)
                {
                    var net = new ToolStripMenuItem(n.Network);
                    foreach (var c in n.Channel)
                    {
                        net.DropDownItems.Add(new ToolStripMenuItem(c.Name, null, OnMenuFavoriteClick, Keys.None));
                    }
                    channels.DropDownItems.Add(net);
                }
                channels.DropDownItems.AddRange(new ToolStripItem[]
                                                    {
                                                        new ToolStripSeparator(),
                                                        new ToolStripMenuItem("Clear history", null, OnMenuFavoriteClick, Keys.None)
                                                    });                    
            }
            else
            {
                channels.Enabled = false;                
            }            
            /* Add to menu */
            _mnuFavorites.DropDownItems.AddRange(new ToolStripItem[]
                                                     {
                                                         channels,
                                                         new ToolStripSeparator(),
                                                         new ToolStripMenuItem("Add to favorites", null,
                                                                               OnMenuFavoriteClick, Keys.None),
                                                         new ToolStripMenuItem("Organize favorites", null,
                                                                               OnMenuFavoriteClick, Keys.Alt | Keys.O)
                                                     });
            /* Append favorites to the end */
            var count = 0;
            foreach (var c in ChannelManager.Channels.Favorites.Favorite)
            {
                if (count == 0)
                {
                    _mnuFavorites.DropDownItems.Add(new ToolStripSeparator());
                }
                if (count > 20)
                {
                    break;
                }
                _mnuFavorites.DropDownItems.Add(new ToolStripMenuItem(c.Name, null, OnMenuFavoriteClick, Keys.None));
                count++;
            }
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
                foreach (var c in (from TreeNode w in n.Nodes from TreeNode c in w.Nodes select c).Where(c => (c.Tag is FrmChildWindow)))
                {
                    child = (FrmChildWindow) c.Tag;
                    _mnuWindow.DropDownItems.Add(new ToolStripMenuItem(
                                                     Functions.TruncateString(child.Text, 100), null,
                                                     OnMenuWindowClick)
                                                     {
                                                         Tag = child,
                                                         Checked = child == active
                                                     });
                }
            }
        }

        private void OnMenuFavoriteClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var c = WindowManager.GetActiveWindow();
            if (c == null || item == null)
            {
                return;
            }
            switch (item.Text.ToUpper())
            {
                case "CLEAR HISTORY":
                    ChannelManager.Channels.Recent.Channels.Clear();
                    break;

                case "ADD TO FAVORITES":
                    using (var d = new FrmChannelEdit(DialogEditType.Add) {Channel = c.WindowType == ChildWindowType.Channel ? c.Tag.ToString() : string.Empty})
                    {
                        if (d.ShowDialog(_owner) == DialogResult.Cancel)
                        {
                            return;
                        }
                        var cd = new ChannelFavorites.ChannelFavoriteData
                                     {
                                         Name = d.Channel,
                                         Password = d.Password,
                                         Description = d.Description
                                     };
                        ChannelManager.Channels.Favorites.Favorite.Add(cd);
                        ChannelManager.Channels.Favorites.Favorite.Sort();
                    }
                    break;

                case "ORGANIZE FAVORITES":
                    using (var d = new FrmFavorites(c.Client))
                    {
                        d.ShowDialog(_owner);
                    }
                    break;

                default:
                    /* Join the channel */
                    if (c.Client.IsConnected)
                    {
                        c.Client.Send(string.Format("JOIN {0}", item.Text));
                    }
                    break;
            }
        }

        private void OnMenuWindowClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var c = WindowManager.GetActiveWindow();
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

        private void OnMenuHelpClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            if (item == null)
            {
                return;
            }
            var hf = Functions.MainDir(@"\FusionIRC Help.chm", true);
            switch (item.Text.ToUpper())
            {
                case "SHOW HELP":
                    if (File.Exists(hf))
                    {
                        try
                        {
                            Process.Start(hf);
                        }
                        catch
                        {
                            Debug.Assert(true);
                        }                        
                    }
                    break;

                case "ABOUT FUSIONIRC...":
                    using (var d = new FrmAbout())
                    {
                        d.ShowDialog(this);
                    }
                    break;
            }
        }

        /* Private methods */
        private void ChangeFont(FrmChildWindow c)
        {
            if (c.WindowType == ChildWindowType.ChanList)
            {
                return;
            }
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
                    foreach (var win in WindowManager.Windows[c.Client].Where(win => win.WindowType == c.WindowType && win != c))
                    {
                        ChangeFont(win, font.SelectedFont);
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
