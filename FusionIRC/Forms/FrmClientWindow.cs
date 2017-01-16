/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Helpers;
using FusionIRC.Properties;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms
{
    public partial class FrmClientWindow : Form
    {
        private readonly bool _initialize;
        private ImageList _images;

        private MdiHelper _mdi;

        /* Constructor */
        public FrmClientWindow()
        {
            _initialize = true;
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            /* MDI helper class */
            _mdi = new MdiHelper(this);            
            ConnectionCallbackManager.MainForm = this;
            /* Load client settings */
            SettingsManager.Load();
            /* Load servers */
            ServerManager.Load("servers.xml");
            /* Load client current theme */
            ThemeManager.ThemeLoaded += WindowManager.OnThemeLoaded;
            ThemeManager.Load(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path);
            /* Set window position and size */
            var w = SettingsManager.GetWindowByName("application");
            Size = w.Size;
            Location = w.Position;
            WindowState = w.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
            /* Treeview icons */
            _images = new ImageList {ImageSize = new Size(16, 16), ColorDepth = ColorDepth.Depth32Bit};
            _images.Images.AddRange(new[]
                                        {
                                            Resources.status.ToBitmap(),
                                            Resources.channel.ToBitmap(),
                                            Resources.query.ToBitmap(),
                                            Resources.dcc_chat.ToBitmap()
                                        });
            switchTree.ImageList = _images;
            switchTree.AfterSelect += SwitchTreeAfterSelect;
            SwitchViewSplitter.SplitPosition = SettingsManager.Settings.SettingsWindows.SwitchTreeWidth;
            SwitchViewSplitter.SplitterMoving += SplitterMoving;            
            _initialize = false;
            //remove
            //var f = new FrmTest
            //            {
            //                MdiParent = this
            //            };
            //f.Show();
        }

        /* Overrides */
        protected override void OnLoad(EventArgs e)
        {
            /* Create our first connection */
            WindowManager.AddWindow(null, ChildWindowType.Console, this, "Console", "Console", true);
            base.OnLoad(e);
        }

        protected override void OnMove(System.EventArgs e)
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

        protected override void OnResize(System.EventArgs e)
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

        private static void SplitterMoving(object sender, SplitterEventArgs e)
        {
            /* Save the switch window "size" */
            SettingsManager.Settings.SettingsWindows.SwitchTreeWidth = e.SplitX;
        }

        /* Treeview selection */
        private void SwitchTreeAfterSelect(object sender, TreeViewEventArgs e)
        {
            /* Flicker-free form activation */
            if (e.Action != TreeViewAction.ByMouse)
            {
                return;
            }
            var t = switchTree.SelectedNode;                                   
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
