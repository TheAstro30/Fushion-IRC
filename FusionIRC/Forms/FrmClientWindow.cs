/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Helpers;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms
{
    public partial class FrmClientWindow : Form
    {
        private readonly bool _initialize;

        /* Constructor */
        public FrmClientWindow()
        {
            _initialize = true;
            InitializeComponent();
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
            /* Create our first connection */
            WindowManager.AddWindow(null, ChildWindowType.Console, this, "Console", "Console", true);
            _initialize = false;
            //remove
            //var f = new FrmTest
            //            {
            //                MdiParent = this
            //            };
            //f.Show();
        }

        /* Overrides */
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
            /* Save client settings */
            SettingsManager.Save();
            /* Save servers */
            ServerManager.Save("servers.xml");
            /* Save client current theme */            
            ThemeManager.Save(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path);
            base.OnFormClosing(e);
        }
    }
}
