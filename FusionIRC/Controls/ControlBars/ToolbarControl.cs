/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Autos;
using FusionIRC.Forms.Favorites;
using FusionIRC.Forms.Misc;
using FusionIRC.Forms.ScriptEditor;
using FusionIRC.Forms.Settings;
using FusionIRC.Forms.Theming;
using FusionIRC.Forms.Users;
using FusionIRC.Helpers;
using FusionIRC.Properties;
using ircCore.Controls.Rendering;
using ircCore.Settings.Networks;

namespace FusionIRC.Controls.ControlBars
{
    public sealed class ToolbarControl : ControlBar
    {
        private readonly Form _owner;

        private bool _connect;
        private bool _disconnect;

        private readonly ToolStripButton _btnConnect;
        private readonly ToolStripDropDownButton _btnConnectDrop;
        private readonly ToolStripButton _btnConnectToLocation;
        private readonly ToolStripButton _btnChanList;
        private readonly ToolStripButton _btnAliases;
        private readonly ToolStripButton _btnFavorites;
        private readonly ToolStripButton _btnJoin;
        private readonly ToolStripButton _btnPart;
        private readonly ToolStripButton _btnSettings;
        private readonly ToolStripButton _btnTheme;
        private readonly ToolStripButton _btnDcc;
        private readonly ToolStripButton _btnUsers;
        private readonly ToolStripButton _btnAutos;
        private readonly ToolStripButton _btnAbout;

        private readonly Timer _tmrCheck;
        
        public MenubarControl MenuBar { private get; set; }

        public ToolbarControl(Form owner)
        {
            _owner = owner;
            Stretch = true;
            AutoSize = false;
            ImageScalingSize = new Size(32, 32);
            var renderer = new CustomRenderer(new Renderer());
            RenderMode = ToolStripRenderMode.Professional;
            Renderer = renderer;           
            GripStyle = ToolStripGripStyle.Visible;
            ShowItemToolTips = true;
            LayoutStyle = ToolStripLayoutStyle.StackWithOverflow;
            Padding = new Padding(3, 2, 0, 0);
            Tag = "TOOLBAR";
            /* Connect button */
            _btnConnect = new ToolStripButton
                              {
                                  Image = Resources.connect.ToBitmap(),
                                  ImageScaling = ToolStripItemImageScaling.None,
                                  Size = new Size(32, 32),
                                  Tag = "CONNECT",
                                  ToolTipText = @"Connect"
                              };
            _btnConnect.Click += ToolbarButtonClick;
            _btnConnectDrop = new ToolStripDropDownButton { Tag = "CONNECT" };
            _btnConnectDrop.DropDownOpening += DropDownOpening;
            /* Connect to location button */
            _btnConnectToLocation = new ToolStripButton
                                        {
                                            Image = Resources.connect_to_location.ToBitmap(),
                                            ImageScaling = ToolStripItemImageScaling.None,
                                            Size = new Size(32, 32),
                                            Tag = "CONNECTTO",
                                            ToolTipText = @"Connect to location"
                                        };
            _btnConnectToLocation.Click += ToolbarButtonClick;
            /* Channels list */
            _btnChanList = new ToolStripButton
                               {
                                   Image = Resources.chanlist.ToBitmap(),
                                   ImageScaling = ToolStripItemImageScaling.None,
                                   Size = new Size(32, 32),
                                   Tag = "CHANLIST",
                                   ToolTipText = @"Channels list"
                               };
            _btnChanList.Click += ToolbarButtonClick;
            /* Aliases */
            _btnAliases = new ToolStripButton
                              {
                                  Image = Resources.aliases.ToBitmap(),
                                  ImageScaling = ToolStripItemImageScaling.None,
                                  Size = new Size(32, 32),
                                  Tag = "ALIASES",
                                  ToolTipText = @"Script editor"
                              };
            _btnAliases.Click += ToolbarButtonClick;
            /* Favorites */
            _btnFavorites = new ToolStripButton
                                {
                                    Image = Resources.favorites.ToBitmap(),
                                    ImageScaling = ToolStripItemImageScaling.None,
                                    Size = new Size(32, 32),
                                    Tag = "FAVORITES",
                                    ToolTipText = @"Favorite channels"
                                };
            _btnFavorites.Click += ToolbarButtonClick;
            /* Join channel */
            _btnJoin = new ToolStripButton
                           {
                               Image = Resources.join.ToBitmap(),
                               ImageScaling = ToolStripItemImageScaling.None,
                               Size = new Size(32, 32),
                               Tag = "JOIN",
                               ToolTipText = @"Join a channel"
                           };
            _btnJoin.Click += ToolbarButtonClick;
            /* Part channel */
            _btnPart = new ToolStripButton
                           {
                               Image = Resources.part.ToBitmap(),
                               ImageScaling = ToolStripItemImageScaling.None,
                               Size = new Size(32, 32),
                               Tag = "PART",
                               ToolTipText = @"Part a channel"
                           };
            _btnPart.Click += ToolbarButtonClick;            
            /* Settings button */
            _btnSettings = new ToolStripButton
                               {
                                   Image = Resources.settings.ToBitmap(),
                                   ImageScaling = ToolStripItemImageScaling.None,
                                   Size = new Size(32, 32),
                                   Tag = "SETTINGS",
                                   ToolTipText = @"Settings"
                               };
            _btnSettings.Click += ToolbarButtonClick;
            /* Theme button */
            _btnTheme = new ToolStripButton
                            {
                                Image = Resources.theme.ToBitmap(),
                                ImageScaling = ToolStripItemImageScaling.None,
                                Size = new Size(32, 32),
                                Tag = "THEME",
                                ToolTipText = @"Theme manager"
                            };
            _btnTheme.Click += ToolbarButtonClick;
            /* DCC manager button */
            _btnDcc = new ToolStripButton
                          {
                              Image = Resources.dcc.ToBitmap(),
                              ImageScaling = ToolStripItemImageScaling.None,
                              Size = new Size(32, 32),
                              Tag = "DCC",
                              ToolTipText = @"DCC transfer manager"
                          };
            _btnDcc.Click += ToolbarButtonClick;
            /* Automations button */
            _btnAutos = new ToolStripButton
                            {
                                Image = Resources.autos.ToBitmap(),
                                ImageScaling = ToolStripItemImageScaling.None,
                                Size = new Size(32, 32),
                                Tag = "AUTOS",
                                ToolTipText = @"Automations"
                            };
            _btnAutos.Click += ToolbarButtonClick;
            /* Users button */
            _btnUsers = new ToolStripButton
                            {
                                Image = Resources.users.ToBitmap(),
                                ImageScaling = ToolStripItemImageScaling.None,
                                Size = new Size(32, 32),
                                Tag = "USERS",
                                ToolTipText = @"User list"
                            };
            _btnUsers.Click += ToolbarButtonClick;
            /* About button */
            _btnAbout = new ToolStripButton
                            {
                                Image = Resources.about.ToBitmap(),
                                ImageScaling = ToolStripItemImageScaling.None,
                                Size = new Size(32, 32),
                                Tag = "ABOUT",
                                ToolTipText = @"About FusionIRC"
                            };
            _btnAbout.Click += ToolbarButtonClick;
            /* Add the buttons to the toolbar */
            Items.AddRange(new ToolStripItem[]
                               {
                                   _btnConnect, _btnConnectDrop, _btnConnectToLocation, new ToolStripSeparator(), _btnSettings, _btnTheme,
                                   _btnAliases, new ToolStripSeparator(), _btnChanList, _btnFavorites, _btnJoin, _btnPart, 
                                   new ToolStripSeparator(), _btnDcc, _btnUsers, _btnAutos, new ToolStripSeparator(), _btnAbout
                               });

            _tmrCheck = new Timer
                            {
                                Interval = 250,
                                Enabled = true
                            };
            _tmrCheck.Tick += TimerCheckConnection;            
        }

        /* Toolbar callback */
        private void ToolbarButtonClick(object sender, EventArgs e)
        {
            var btn = (ToolStripButton)sender;
            var c = WindowManager.GetActiveConnection();            
            if (c == null || btn == null)
            {
                return;
            }
            var console = WindowManager.GetConsoleWindow(c);
            if (console == null)
            {
                return;
            }
            switch (btn.Tag.ToString())
            {
                case "CONNECT":
                    ConnectToServer(c, console);
                    break;

                case "DISCONNECT":                    
                    DisconnectFromServer(c, console);
                    break;

                case "CONNECTTO":
                    ConnectToLocation(_owner, console);
                    break;

                case "SETTINGS":
                    using (var settings = new FrmSettings())
                    {                        
                        settings.ShowDialog(_owner);
                    }
                    break;

                case "ALIASES":
                    var edit = WindowManager.FindClientWindow(@"FusionIRC - Script Editor");
                    if (edit != null)
                    {
                        edit.BringToFront();
                        return;
                    }
                    edit = new FrmScript();
                    edit.Show();
                    break;

                case "CHANLIST":
                    break;

                case "FAVORITES":
                    using (var fav = new FrmFavorites(console.Client))
                    {
                        fav.ShowDialog(_owner);
                    }
                    break;

                case "JOIN":
                    if (!c.IsConnected)
                    {
                        return;
                    }
                    using (var join = new FrmJoin())
                    {
                        if (join.ShowDialog(_owner) == DialogResult.OK)
                        {
                            c.Send(string.Format("JOIN {0}", join.Channels));
                        }                        
                    }
                    break;

                case "PART":
                    if (!c.IsConnected)
                    {
                        return;
                    }
                    using (var part = new FrmPart(c))
                    {
                        if (part.ShowDialog(_owner) == DialogResult.OK)
                        {
                            c.Send(part.Channels == "0" ? "JOIN 0" : string.Format("PART {0}", part.Channels));
                        }
                    }
                    break;

                case "DCC":
                    WindowManager.DccManagerWindow.Show(_owner);
                    break;

                case "USERS":
                    using (var users = new FrmUsers())
                    {
                        users.ShowDialog(_owner);
                    }
                    break;

                case "AUTOS":
                    using (var autos = new FrmAutos())
                    {
                        autos.ShowDialog(_owner);
                    }
                    break;

                case "THEME":
                    using (var theme = new FrmTheme())
                    {
                        theme.ShowDialog(_owner);
                    }
                    break;

                case "ABOUT":
                    using (var about = new FrmAbout())
                    {
                        about.ShowDialog(_owner);
                    }
                    break;
            }
        }

        private void DropDownOpening(object sender, EventArgs e)
        {
            var btn = (ToolStripDropDownButton) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Tag.ToString())
            {
                case "CONNECT":
                    btn.DropDownItems.Clear();
                    foreach (var s in ServerManager.Servers.Recent.Server)
                    {
                        btn.DropDownItems.Add(s.ToString(), null, OnRecentServerClick);
                    }
                    break;
            }
        }

        private void OnRecentServerClick(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var c = WindowManager.GetActiveConnection();
            if (c == null || item == null)
            {
                return;
            }
            ConnectToRecentServer(c, item);
        }

        private void TimerCheckConnection(object sender, EventArgs e)
        {
            var c = WindowManager.GetActiveConnection();
            if (c == null)
            {
                return;
            }
            if (c.IsConnected || c.IsConnecting)
            {
                if (_disconnect)
                {
                    return;
                }
                _btnConnect.Tag = "DISCONNECT";
                _btnConnect.ToolTipText = @"Disconnect";
                _btnConnect.Image = Resources.disconnect.ToBitmap();
                _connect = false;
                _disconnect = true;
                if (MenuBar != null)
                {
                    MenuBar.ConnectionUpdate(true);
                }
            }
            else
            {
                if (_connect)
                {                    
                    return;
                }
                _btnConnect.Tag = "CONNECT";
                _btnConnect.ToolTipText = @"Connect";
                _btnConnect.Image = Resources.connect.ToBitmap();
                _connect = true;
                _disconnect = false;
                if (MenuBar != null)
                {
                    MenuBar.ConnectionUpdate(false);
                }
            }
        }
    }
}
