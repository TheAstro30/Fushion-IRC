/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Controls;
using FusionIRC.Controls.SwitchView;
using FusionIRC.Forms.Child;
using FusionIRC.Helpers;
using FusionIRC.Properties;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;
using ircCore.Users;
using ircCore.Utils;

namespace FusionIRC.Forms
{
    public sealed class FrmClientWindow : Form
    {
        private readonly bool _initialize;
        private readonly ImageList _images;
        private readonly MdiHelper _mdi;

        private readonly ToolStripPanel _dockTop;
        private readonly ToolStripPanel _dockLeft;
        private readonly ToolStripPanel _dockBottom;
        private readonly ToolStripPanel _dockRight;

        private readonly Splitter _switchViewSplitter;

        public WindowTreeView SwitchView;        
        public ToolbarControl ToolBar { get; private set; }
        public MenubarControl MenuBar { get; private set; }

        /* Constructor */
        public FrmClientWindow()
        {
            _initialize = true;
            /* Set main application directory */
            Functions.SetMainDir();
            Functions.CheckFolders();
            /* Load client settings */
            SettingsManager.Load();
            /* Load servers */
            ServerManager.Load();
            /* Load client current theme */
            ThemeManager.ThemeLoaded += WindowManager.OnThemeLoaded;
            ThemeManager.Load(Functions.MainDir(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path, false));
            /* Load users list */
            UserManager.Load();
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
            _images = new ImageList { ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit };
            _images.Images.AddRange(new[]
                                        {
                                            Resources.status.ToBitmap(),
                                            Resources.channel.ToBitmap(),
                                            Resources.query.ToBitmap(),
                                            Resources.dcc_chat.ToBitmap()
                                        });
            SwitchView.ImageList = _images;
            SwitchView.AfterSelect += SwitchViewAfterSelect;
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
            /* Add controls */
            Controls.AddRange(new Control[] { _switchViewSplitter, SwitchView, _dockLeft, _dockRight, _dockTop, _dockBottom });
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
            ConnectionCallbackManager.MainForm = this;            
            /* Set window position and size */
            var w = SettingsManager.GetWindowByName("application");
            Size = w.Size;
            Location = w.Position;
            WindowState = w.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
            _initialize = false;
        }

        /* Overrides */
        protected override void OnLoad(EventArgs e)
        {
            /* Create our first connection */
            WindowManager.AddWindow(null, ChildWindowType.Console, this, "Console", "Console", true);
            base.OnLoad(e);
        }

        protected override void OnActivated(EventArgs e)
        {            
            /* Activate the child window that's currently to front */
            var win = WindowManager.GetActiveWindow(this);
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

        protected override void OnResize(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName("application");
                if (WindowState != FormWindowState.Maximized)
                {
                    w.Size = Size;
                    w.Position = Location;
                }
                w.Maximized = WindowState == FormWindowState.Maximized;
            }
            base.OnResize(e);
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
                client.Key.Send("QUIT :Leaving.");
                client.Key.Disconnect();
            }
            /* Get the first console window's user info data and copy it back to settings */
            var c = WindowManager.GetActiveConnection(this);
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
            ThemeManager.Save(Functions.MainDir(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path, false));            
            /* Save users list */
            UserManager.Save();
            base.OnFormClosing(e);
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
            var win = (FrmChildWindow)t.Tag;
            if (win == null)
            {               
                return;
            }
            win.Restore();
            _mdi.ActivateChild(win);
            win.MyActivate();            
        }
    }
}
