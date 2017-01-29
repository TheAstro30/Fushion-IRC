/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Classes;
using FusionIRC.Controls.SwitchView;
using FusionIRC.Helpers;
using FusionIRC.Properties;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms
{
    public sealed class FrmClientWindow : Form
    {
        private readonly bool _initialize;
        private readonly ImageList _images;
        private readonly MdiHelper _mdi;
        private readonly Splitter _switchViewSplitter;

        public WindowTreeView SwitchView;        
        public ToolbarControl ToolBar { get; private set; }
        public MenubarControl MenuBar { get; private set; }

        /* Constructor */
        public FrmClientWindow()
        {
            _initialize = true;
            /* Load client settings */
            SettingsManager.Load();
            /* Load servers */
            ServerManager.Load("servers.xml");
            /* Load client current theme */
            ThemeManager.ThemeLoaded += WindowManager.OnThemeLoaded;
            ThemeManager.Load(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path);
            /* Main form initialization */
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Text = @"FusionIRC";
            ClientSize = new Size(953, 554);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            IsMdiContainer = true;
            StartPosition = FormStartPosition.Manual;            
            /* Treeview */
            SwitchView = new WindowTreeView
                             {
                                 BorderStyle = BorderStyle.FixedSingle,
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
            Controls.AddRange(new Control[] {_switchViewSplitter, SwitchView});
            /* Adjust splitter */
            _switchViewSplitter.SplitPosition = SettingsManager.Settings.Windows.SwitchTreeWidth;
            /* Setup toolbar */
            ToolBar = new ToolbarControl(this);
            /* Setup menubar */
            MenuBar = new MenubarControl(this);              
            ToolBar.MenuBar = MenuBar;
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
                SettingsManager.Settings.UserInfo = c.UserInfo;
            }
            /* Save client settings */
            SettingsManager.Save();
            /* Save servers */
            ServerManager.Save("servers.xml");
            /* Save client current theme */            
            ThemeManager.Save(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path);
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
