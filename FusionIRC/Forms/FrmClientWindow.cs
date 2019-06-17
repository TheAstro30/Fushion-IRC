/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Controls.ControlBars;
using FusionIRC.Controls.SwitchView;
using FusionIRC.Forms.Child;
using FusionIRC.Forms.Misc;
using FusionIRC.Helpers;
using FusionIRC.Helpers.Commands;
using FusionIRC.Properties;
using ircCore.Autos;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.Channels;
using ircCore.Settings.Networks;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.Theming;
using ircCore.Users;
using ircCore.Utils;
using ircScript;
using ircScript.Classes;
using ircScript.Classes.Structures;

namespace FusionIRC.Forms
{
    public sealed class FrmClientWindow : TrayIcon
    {
        private readonly bool _initialize;
        private readonly ImageList _images;
        private readonly MdiHelper _mdi;

        private readonly ContextMenuStrip _mdiMenu;

        private readonly ToolStripPanel _dockTop;
        private readonly ToolStripPanel _dockLeft;
        private readonly ToolStripPanel _dockBottom;
        private readonly ToolStripPanel _dockRight;

        private readonly Splitter _switchViewSplitter;

        private readonly Timer _timerConnect;

        public WindowTreeView SwitchView;        
        public ToolbarControl ToolBar { get; private set; }
        public MenubarControl MenuBar { get; private set; }

        /* Constructor */
        public FrmClientWindow()
        {
            _initialize = true;
            MinimumSize = new Size(400, 300);
            /* Set main application directory */
            Functions.SetMainDir();
            Functions.CheckFolders();
            /* Load client settings */
            SettingsManager.Load();
            /* Load servers */
            ServerManager.Load();
            /* Load client current theme */
            ThemeManager.ThemeLoaded += WindowManager.OnThemeLoaded;
            ThemeManager.Load(
                Functions.MainDir(
                    SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path));
            /* Load users list */
            UserManager.Load();
            /* Load automations */
            AutomationsManager.Load();
            /* Channel manager */
            ChannelManager.Load();
            /* Load scripts */
            ScriptManager.LoadMultipleScripts(SettingsManager.Settings.Scripts.Aliases, ScriptManager.AliasData);
            ScriptManager.LoadMultipleScripts(SettingsManager.Settings.Scripts.Events, ScriptManager.EventData);
            /* Build script data */
            ScriptManager.BuildScripts(ScriptType.Aliases, ScriptManager.AliasData, ScriptManager.Aliases);
            ScriptManager.BuildScripts(ScriptType.Events, ScriptManager.EventData, ScriptManager.Events);
            /* Load variables */
            ScriptManager.LoadVariables(Functions.MainDir(@"\scripts\variables.xml"));
            /* Load popups */
            if (SettingsManager.Settings.Scripts.Popups.Count < 6)
            {
                SettingsManager.CreatePopupList();
            }
            PopupManager.LoadMultiplePopups(SettingsManager.Settings.Scripts.Popups);
            PopupManager.OnPopupItemClicked += OnPopupItemClicked;
            /* Main form initialization */
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = @"FusionIRC";
            ClientSize = new Size(953, 554);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            IsMdiContainer = true;
            StartPosition = FormStartPosition.Manual;
            /* Toolbar docks */
            _dockTop = new ToolStripPanel
                           {
                               Dock = DockStyle.Top,
                               Orientation = Orientation.Horizontal,
                               RowMargin = new Padding(3, 0, 0, 0),
                               Tag = "TOP"
                           };
            _dockTop.ControlAdded += DockControlAdded;
            _dockLeft = new ToolStripPanel
                            {
                                Dock = DockStyle.Left,
                                Orientation = Orientation.Vertical,
                                RowMargin = new Padding(0, 3, 0, 0),
                                Tag = "LEFT"
                            };
            _dockLeft.ControlAdded += DockControlAdded;
            _dockBottom = new ToolStripPanel
                              {
                                  Dock = DockStyle.Bottom,
                                  Orientation = Orientation.Horizontal,
                                  RowMargin = new Padding(3, 0, 0, 0),
                                  Tag = "BOTTOM"
                              };
            _dockBottom.ControlAdded += DockControlAdded;
            _dockRight = new ToolStripPanel
                             {
                                 Dock = DockStyle.Right,
                                 Orientation = Orientation.Vertical,
                                 RowMargin = new Padding(0, 3, 0, 0),
                                 Tag = "RIGHT"
                             };
            _dockRight.ControlAdded += DockControlAdded;
            /* Treeview */
            SwitchView = new WindowTreeView
                             {
                                 BorderStyle = BorderStyle.Fixed3D,
                                 Dock = DockStyle.Left,
                                 DrawMode = TreeViewDrawMode.OwnerDrawText,
                                 Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                 HideSelection = false,
                                 Location = new Point(0, 0),
                                 ShowPlusMinus = false,
                                 ShowRootLines = false,
                                 Size = new Size(160, 554),
                                 BackColor = ThemeManager.GetColor(ThemeColor.SwitchTreeBackColor),
                                 ForeColor = ThemeManager.GetColor(ThemeColor.SwitchTreeForeColor),                                 
                                 TabIndex = 0
                             };
            /* Treeview icons */
            _images = new ImageList {ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit};
            _images.Images.AddRange(new[]
                                        {
                                            Resources.status.ToBitmap(),
                                            Resources.channel.ToBitmap(),
                                            Resources.query.ToBitmap(),
                                            Resources.dcc_chat.ToBitmap(),
                                            Resources.list.ToBitmap(),
                                            Resources.notifyGroup.ToBitmap(),
                                            Resources.notify.ToBitmap()
                                        });
            SwitchView.ImageList = _images;
            SwitchView.AfterSelect += SwitchViewAfterSelect;
            SwitchView.NodeMouseDoubleClick += SwitchViewNodeDoubleClick;
            /* Splitter */
            _switchViewSplitter = new Splitter
                                      {
                                          Location = new Point(160, 0),
                                          MinExtra = 60,
                                          MinSize = 80,
                                          Size = new Size(1, 554),
                                          TabIndex = 4,
                                          TabStop = false
                                      };
            _switchViewSplitter.SplitterMoving += SwitchViewSplitterMoving;
            _switchViewSplitter.SplitterMoved += SwitchViewSplitterMoved;
            /* Add controls */
            Controls.AddRange(new Control[]
                                  {
                                      _switchViewSplitter,
                                      SwitchView,
                                      _dockLeft,
                                      _dockRight, 
                                      _dockTop, 
                                      _dockBottom
                                  });
            /* Adjust splitter */
            _switchViewSplitter.SplitPosition = SettingsManager.Settings.Windows.SwitchTreeWidth;
            /* Setup toolbar */
            ToolBar = new ToolbarControl(this);
            SetDockControl(ToolBar);
            /* Setup menubar */
            MenuBar = new MenubarControl(this);            
            SetDockControl(MenuBar);
            ToolBar.MenuBar = MenuBar;
            MainMenuStrip = MenuBar;
            /* MDI helper class */
            _mdi = new MdiHelper(this);
            _mdi.MdiClientWnd.MouseDown += OnMdiMouseDown;
            WindowManager.MainForm = this;
            /* Set window position and size */
            var w = SettingsManager.GetWindowByName("application");
            Size = w.Size;
            Location = w.Position;
            WindowState = w.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
            /* Create MDI context menu */
            _mdiMenu = new ContextMenuStrip();
            _mdiMenu.Opening += MdiMenuOpening;
            BuildMdiMenu();
            /* Set this window background */
            SetNewMdiBackground();            
            /* Tray icon */
            TrayNotifyIcon.Text = @"FusionIRC IRC Client";
            var ico = Functions.MainDir(SettingsManager.Settings.Client.TrayIcon.Icon);
            TrayNotifyIcon.Icon = !string.IsNullOrEmpty(ico) && File.Exists(ico)
                                      ? Icon.ExtractAssociatedIcon(ico)
                                      : Icon;
            TrayHideOnMinimize = SettingsManager.Settings.Client.TrayIcon.HideMinimized;
            TrayShowNotifications = SettingsManager.Settings.Client.TrayIcon.ShowNotifications;
            if (SettingsManager.Settings.Client.TrayIcon.AlwaysShow)
            {
                TrayAlwaysShowIcon = true;
                TrayNotifyIcon.Visible = true;
            }
            /* Show connect dialog */
            _timerConnect = new Timer {Interval = 10};
            _timerConnect.Tick += ShowConnectDialog;
            /* UserManager */
            UserManager.NotifyChanged += SendWatchCommand;
            _initialize = false;
        }

        /* Overrides */
        protected override void OnLoad(EventArgs e)
        {            
            /* Need to call the loading of the main menu popups separately */
            MenuBar.MenuCommands.Visible = PopupManager.BuildPopups(PopupType.Commands, PopupManager.Popups[0],
                                                                    MenuBar.MenuCommands.DropDownItems);
            /* Create our first connection */
            var w = WindowManager.AddWindow(null, ChildWindowType.Console, this, "Console", "Console", true);
            if (w != null)
            {
                w.DisplayNode.Text = string.Format("{0}: {1}", "Console", w.Client.UserInfo.Nick);
            }
            _timerConnect.Enabled = true;
            base.OnLoad(e);            
        }

        protected override void OnActivated(EventArgs e)
        {            
            /* Activate the child window that's currently to front */
            var win = WindowManager.GetActiveWindow();
            if (win != null)
            {
                win.MyActivate();
            }
            base.OnActivated(e);
        }

        protected override void OnMove(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName("application");
                if (WindowState != FormWindowState.Maximized)
                {
                    w.Position = Location;
                }
            }
            base.OnMove(e);
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            if (WindowManager.LastActiveChild.WindowType != ChildWindowType.ChanList && SettingsManager.Settings.Windows.ChildrenMaximized)
            {
                WindowManager.LastActiveChild.Output.UserResize = true;
            }
            base.OnResizeBegin(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName("application");
                if (WindowState == FormWindowState.Normal && Visible)
                {
                    w.Size = Size;
                    w.Position = Location;
                }
                w.Maximized = WindowState == FormWindowState.Maximized;                
            }
            base.OnResize(e);
            Invalidate(true);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            if (WindowManager.LastActiveChild.WindowType != ChildWindowType.ChanList && SettingsManager.Settings.Windows.ChildrenMaximized)
            {
                WindowManager.LastActiveChild.Output.UserResize = false;
            }
            base.OnResizeEnd(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            /* First check close confirmation */            
            string msg = null;
            switch (SettingsManager.Settings.Client.Confirmation.ClientClose)
            {
                case CloseConfirmation.Connected:
                    if (WindowManager.Windows.Any(client => client.Key.IsConnected))
                    {
                        msg = "You are still connected to an IRC server. Are you sure you want to exit?";
                    }                    
                    break;

                case CloseConfirmation.Always:
                    msg = "Are you sure you want to exit?";
                    break;
            }
            if (!string.IsNullOrEmpty(msg) && MessageBox.Show(msg, @"Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                /* Abort closing */
                e.Cancel = true;
                return;
            }
            /* Save child forms window state */            
            if (MdiChildren.Where(w => w is FrmChildWindow).Any(w => w.WindowState == FormWindowState.Maximized))
            {
                SettingsManager.Settings.Windows.ChildrenMaximized = true;
            }
            /* Disconnect each connection */
            foreach (var client in WindowManager.Windows.Where(client => client.Key.IsConnected))
            {
                CommandChannel.ParseQuit(client.Key, string.Empty);
                client.Key.Disconnect();
            }
            /* Get the first console window's user info data and copy it back to settings */
            var c = WindowManager.GetActiveConnection();
            if (c != null)
            {                
                SettingsManager.Settings.UserInfo = new SettingsUserInfo(c.UserInfo);
            }
            /* In order for the client to load the toolbars in the correct order, we need to get their positions within each dock... */
            UpdateDockLayout();
            /* Save client settings */
            SettingsManager.Save();
            /* Save servers */
            ServerManager.Save();
            /* Save client current theme */                        
            ThemeManager.Save(Functions.MainDir(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path));            
            /* Save users list */
            UserManager.Save();
            /* Save automations */
            AutomationsManager.Save();
            /* Channel manager */
            ChannelManager.Save();
            /* Save variables */
            ScriptManager.SaveVariables(Functions.MainDir(@"\scripts\variables.xml"));
            base.OnFormClosing(e);
        }

        private void OnMdiMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _mdiMenu.Show(this, PointToClient(Cursor.Position));
            }
            OnMouseDown(e);
        }

        private void SwitchViewSplitterMoving(object sender, SplitterEventArgs e)
        {
            if (_initialize)
            {
                return;
            }
            /* Save the switch window "size" */
            SettingsManager.Settings.Windows.SwitchTreeWidth = e.SplitX;
        }

        private void SwitchViewSplitterMoved(object sender, SplitterEventArgs e)
        {
            Invalidate(true);
        }

        /* Dock panels */
        private void SetDockControl(ToolStrip ctl)
        {
            var cs = SettingsManager.GetControlStyle(ctl.Tag.ToString());
            switch (cs.Dock)
            {
                case DockStyle.Top:
                    _dockTop.Join(ctl, cs.Position);
                    break;

                case DockStyle.Left:
                    _dockLeft.Join(ctl, cs.Position);
                    break;

                case DockStyle.Bottom:
                    _dockBottom.Join(ctl, cs.Position);
                    break;

                case DockStyle.Right:
                    _dockRight.Join(ctl, cs.Position);
                    break;
            }    
            /* Set visibility */
            ctl.Visible = cs.Visible;
        }

        /* Private helpers */
        private void SetNewMdiBackground()
        {
            var bmp = Functions.MainDir(SettingsManager.Settings.Windows.MdiBackground.Path);
            if (string.IsNullOrEmpty(bmp) || !File.Exists(bmp))
            {
                return;
            }
            BackgroundImage = new Bitmap(bmp);
            BackgroundImageLayout = SettingsManager.Settings.Windows.MdiBackground.Layout;
        }

        private void UpdateDockLayout()
        {            
            foreach (var ctl in Controls)
            {
                if (ctl.GetType() != typeof (ToolStripPanel))
                {
                    continue;
                }
                /* This is kind of annoying */
                foreach (var row in ((ToolStripPanel)ctl).Rows)
                {
                    if (row.Controls[0] == null)
                    {
                        continue;
                    }
                    var cs = SettingsManager.GetControlStyle(row.Controls[0].Tag.ToString());
                    cs.Position = new Point(row.Bounds.X, row.Bounds.Y);
                }
            }
        }

        private static void DockControlAdded(object sender, ControlEventArgs e)
        {
            var dock = (ToolStripPanel)sender;
            if (dock == null)
            {
                return;
            }
            var cs = SettingsManager.GetControlStyle(e.Control.Tag.ToString());
            switch (dock.Tag.ToString())
            {
                case "TOP":
                    cs.Dock = DockStyle.Top;
                    break;

                case "LEFT":
                    cs.Dock = DockStyle.Left;
                    break;

                case "BOTTOM":
                    cs.Dock = DockStyle.Bottom;
                    break;

                case "RIGHT":
                    cs.Dock = DockStyle.Right;
                    break;
            }
        }

        /* Treeview selection */
        private void SwitchViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            /* Flicker-free form activation */
            if (e.Action != TreeViewAction.ByMouse)
            {
                return;
            }
            var t = SwitchView.SelectedNode;
            if (!(t.Tag is FrmChildWindow))
            {
                return;
            }
            var win = (FrmChildWindow)t.Tag;
            win.Restore();
            _mdi.ActivateChild(win);            
            win.MyActivate();
        }

        private void SwitchViewNodeDoubleClick(object sender, EventArgs e)
        {
            var t = SwitchView.SelectedNode;
            if (!(t.Tag is User))
            {
                return;
            }
            var root = t.Parent.Parent;
            if (root == null)
            {
                return;
            }
            var c = ((FrmChildWindow)root.Tag).Client;
            if (!c.IsConnected)
            {
                return;
            }
            var u = (User)t.Tag;
            /* See if we have an open query/private chat with this fucker */
            var w = WindowManager.GetWindow(c, u.Nick);
            if (w == null)
            {
                WindowManager.AddWindow(c, ChildWindowType.Private, this, u.Nick, u.Nick, true);
            }
        }

        /* MDI context menu */
        private void BuildMdiMenu()
        {
            _mdiMenu.Items.Clear();
            var enable = BackgroundImage != null;
            var m = new ToolStripMenuItem("Background");
            m.DropDownItems.AddRange(new ToolStripItem[]
                                         {
                                             new ToolStripMenuItem("Select", null, OnMdiMenuClick),
                                             new ToolStripMenuItem("None", null, OnMdiMenuClick),
                                             new ToolStripSeparator(),
                                             new ToolStripMenuItem("Tile", null, OnMdiMenuClick),
                                             new ToolStripMenuItem("Center", null, OnMdiMenuClick),
                                             new ToolStripMenuItem("Stretch", null, OnMdiMenuClick),
                                             new ToolStripMenuItem("Zoom", null, OnMdiMenuClick)                                             
                                         });
            //none = 0, tile = 1; center = 2; stretch = 3; zoom = 4;             
            for (var i = 3; i <= m.DropDownItems.Count - 1; i++)
            {
                m.DropDownItems[i].Enabled = enable;
                if (enable && ((int)BackgroundImageLayout == i - 2))
                {
                    ((ToolStripMenuItem) m.DropDownItems[i]).Checked = true;
                }
            }
            _mdiMenu.Items.Add(m);
        }

        private void MdiMenuOpening(object sender, EventArgs e)
        {
            BuildMdiMenu();
        }

        private void OnMdiMenuClick(object sender, EventArgs e)
        {
            var i = (ToolStripMenuItem) sender;
            if (i == null)
            {
                return;
            }
            switch (i.Text.ToUpper())
            {
                case "SELECT":
                    using (var ofd = new OpenFileDialog
                    {
                        Title = @"Select a background image to load",
                        Filter = @"Picture files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|" + @"PNG Images (*.png)|*.png|JPEG Images (*.jpg)|*.jpg|" + @"Bitmap Images (*.bmp)|*.bmp",
                    })
                    {
                        if (ofd.ShowDialog(this) == DialogResult.Cancel)
                        {
                            return;
                        }
                        SettingsManager.Settings.Windows.MdiBackground = new MdiBackground
                                                                             {
                                                                                 Path = Functions.MainDir(ofd.FileName),
                                                                                 Layout = ImageLayout.Center
                                                                             };
                    }
                    SetNewMdiBackground();
                    return;

                case "NONE":
                    BackgroundImage = null;
                    BackgroundImageLayout = ImageLayout.None;
                    SettingsManager.Settings.Windows.MdiBackground = new MdiBackground();
                    break;

                case "TILE":                    
                    BackgroundImageLayout = ImageLayout.Tile;
                    break;

                case "CENTER":
                    BackgroundImageLayout = ImageLayout.Center;
                    break;

                case "STRETCH":
                    BackgroundImageLayout = ImageLayout.Stretch;
                    break;

                case "ZOOM":
                    BackgroundImageLayout = ImageLayout.Zoom;
                    break;
            }
            SettingsManager.Settings.Windows.MdiBackground.Layout = BackgroundImageLayout;
        }

        private void ShowConnectDialog(object sender, EventArgs e)
        {
            _timerConnect.Enabled = false;
            var w = WindowManager.GetActiveWindow();
            if (w == null)
            {
                return;
            }
            /* Auto-connect */
            if (AutomationsManager.Automations.Connect.Enable)
            {
                var n = AutomationsManager.GetAutomationByNetwork(AutomationsManager.AutomationType.Connect, "All");
                if (n != null)
                {
                    for (var i = 0; i <= n.Data.Count - 1; i++)
                    {
                        CommandProcessor.Parse(w.Client, w,
                                               i == 0
                                                   ? string.Format("SERVER {0}:{1}", n.Data[i].Item, n.Data[i].Value)
                                                   : string.Format("SERVER -M {0}:{1}", n.Data[i].Item, n.Data[i].Value));
                    }
                }
            }
            if (!SettingsManager.Settings.Connection.ShowConnectDialog)
            {
                return;
            }
            using (var connect = new FrmConnectTo(this, w))
            {
                connect.ShowDialog(this);
            }
        }
        
        /* Main callback for all popup's used */
        private static void OnPopupItemClicked(Script s)
        {
            var c = WindowManager.GetActiveWindow();
            if (c == null)
            {
                return;
            }
            string[] args = null;
            var e = new ScriptArgs
                         {
                             ChildWindow = c,
                             ClientConnection = c.Client,
                         };
            switch (s.PopupType)
            {
                case PopupType.Channel:
                case PopupType.Nicklist:
                    e.Channel = c.Tag.ToString();
                    if (c.Nicklist != null && c.Nicklist.SelectedNicks.Count > 0)
                    {
                        e.Nick = c.Nicklist.SelectedNicks[0];
                        args = c.Nicklist.SelectedNicks.ToArray();
                    }
                    break;

                case PopupType.Private:
                case PopupType.DccChat:
                    var nick = c.Tag.ToString();
                    e.Nick = nick[0] == '=' ? nick.Substring(1) : nick;
                    break;
            }
            s.LineParsed += PopupLineParsed;
            s.ParseCompleted += PopupParseCompleted;
            s.Parse(e, args);
            c.Input.Focus();
        }

        private static void PopupLineParsed(Script s, ScriptArgs e, string lineData)
        {
            CommandProcessor.Parse(e.ClientConnection, (FrmChildWindow) e.ChildWindow, lineData);
        }

        private static void PopupParseCompleted(Script s)
        {
            s.LineParsed -= PopupLineParsed;
            s.ParseCompleted -= PopupParseCompleted;
        }

        /* UserManager callbacks */
        private static void SendWatchCommand(string data)
        {
            foreach (var c in WindowManager.Windows.Where(c => c.Key.IsConnected))
            {
                if (c.Key.Parser.AllowsWatch)
                {
                    c.Key.Send(string.Format("WATCH {0}", data));
                }
                else
                {
                    /* This is left open for using a ISON timer */
                    continue;
                }
            }
        }
    }
}
