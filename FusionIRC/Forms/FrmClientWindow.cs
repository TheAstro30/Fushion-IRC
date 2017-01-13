/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Windows.Forms;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms
{
    public partial class FrmClientWindow : Form
    {
        private readonly bool _initialize;

        public FrmClientWindow()
        {
            _initialize = true;
            InitializeComponent();
            /* Load client settings */
            SettingsManager.Load();
            /* Load servers */
            ServerManager.Load("servers.xml");
            /* Load client current theme */
            ThemeManager.ThemeLoaded += ThemeLoaded;
            ThemeManager.Load(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path);
            /* Set window position and size */
            var w = SettingsManager.GetWindowByName("application");
            Size = w.Size;
            Location = w.Position;
            WindowState = w.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
            _initialize = false;
            //remove
            var f = new FrmTest
                        {
                            MdiParent = this
                        };
            f.Show();
            //create a test server
            //ServerManager.AddServer("Abc", "irc.formatme.com");
            //System.Diagnostics.Debug.Print("Server - " + ServerManager.GetNextServer("Test"));
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
            /* Save client settings */
            SettingsManager.Save();
            /* Save servers */
            ServerManager.Save("servers.xml");
            /* Save client current theme */            
            ThemeManager.Save(SettingsManager.Settings.Themes.Theme[SettingsManager.Settings.Themes.CurrentTheme].Path);
            base.OnFormClosing(e);
        }

        private void ThemeLoaded()
        {
            System.Diagnostics.Debug.Print("Theme loaded " + ThemeManager.CurrentTheme.Name);
            /* Here we now refresh all open chat windows ... */
        }
    }
}
