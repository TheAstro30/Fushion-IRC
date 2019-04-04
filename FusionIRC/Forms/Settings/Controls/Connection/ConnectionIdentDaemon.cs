/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;
using ircCore.Utils;

namespace FusionIRC.Forms.Settings.Controls.Connection
{
    public class ConnectionIdentDaemon : BaseControlRenderer, ISettings
    {
        private Label _lblInfo;
        private CheckBox _chkEnable;
        private CheckBox _chkShow;
        private Label _lblUser;
        private TextBox _txtUser;
        private Label _lblSystem;
        private TextBox _txtSystem;
        private Label _lblPort;
        private TextBox _txtPort;

        public event Action OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ConnectionIdentDaemon()
        {
            InitializeComponent();

            Header = "Ident Daemon";

            _chkEnable.Checked = SettingsManager.Settings.Connection.Identd.Enable;
            _chkShow.Checked = SettingsManager.Settings.Connection.Identd.ShowRequests;
            _txtUser.Text = SettingsManager.Settings.Connection.Identd.UserId;
            _txtSystem.Text = SettingsManager.Settings.Connection.Identd.System;
            _txtPort.Text = SettingsManager.Settings.Connection.Identd.Port.ToString();

            _chkEnable.CheckedChanged += ControlsChanged;
            _chkShow.CheckedChanged += ControlsChanged;
            _txtUser.TextChanged += ControlsChanged;
            _txtSystem.TextChanged += ControlsChanged;
            _txtPort.TextChanged += ControlsChanged;
        }

        /* Save settings */
        public void SaveSettings()
        {
            SettingsManager.Settings.Connection.Identd.Enable = _chkEnable.Checked;
            SettingsManager.Settings.Connection.Identd.ShowRequests = _chkShow.Checked;
            SettingsManager.Settings.Connection.Identd.UserId = Functions.GetFirstWord(_txtUser.Text);
            SettingsManager.Settings.Connection.Identd.System = Functions.GetFirstWord(_txtSystem.Text);
            var port = Functions.GetFirstWord(_txtPort.Text);
            int p;
            if (!Int32.TryParse(port, out p))
            {
                p = 113;
            }
            SettingsManager.Settings.Connection.Identd.Port = p;
            SettingsChanged = false;
        }

        /* Private methods */
        private void InitializeComponent()
        {
            _lblInfo = new Label
                           {
                               Location = new Point(42, 77),
                               Size = new Size(371, 33),
                               Text = @"Some IRC servers require an Identd service to be running before allowing connection. You can configure the settings below:"
                           };

            _chkEnable = new CheckBox
                             {
                                 AutoSize = true,
                                 Location = new Point(81, 137),
                                 Size = new Size(267, 19),
                                 TabIndex = 0,
                                 Text = @"Enable Ident daemon server when connecting",
                                 UseVisualStyleBackColor = true
                             };

            _chkShow = new CheckBox
                           {
                               AutoSize = true,
                               Location = new Point(81, 162),
                               Size = new Size(229, 19),
                               TabIndex = 1,
                               Text = @"Show Ident request in console window",
                               UseVisualStyleBackColor = true
                           };

            _lblUser = new Label
                           {
                               AutoSize = true,
                               Location = new Point(78, 194),
                               Size = new Size(47, 15),
                               Text = @"User ID:"
                           };

            _txtUser = new TextBox
                           {
                               Location = new Point(131, 191),
                               MaxLength = 50,
                               Size = new Size(179, 23),
                               TabIndex = 2
                           };

            _lblSystem = new Label
                             {
                                 AutoSize = true,
                                 Location = new Point(77, 223),
                                 Size = new Size(48, 15),
                                 Text = @"System:"
                             };

            _txtSystem = new TextBox
                             {
                                 Location = new Point(131, 220),
                                 MaxLength = 50,                                 
                                 Size = new Size(179, 23),
                                 TabIndex = 3
                             };

            _lblPort = new Label
                           {
                               AutoSize = true,
                               Location = new Point(93, 252),
                               Size = new Size(32, 15),
                               Text = @"Port:"
                           };

            _txtPort = new TextBox
                           {
                               Location = new Point(131, 249),
                               MaxLength = 5,
                               Size = new Size(53, 23),
                               TabIndex = 4
                           };

            Controls.AddRange(new Control[]
                                  {
                                      _lblInfo, _chkEnable, _chkShow, _lblUser, _txtUser, _lblSystem, _txtSystem, _lblPort, _txtPort
                                  });
        }

        /* Callback */
        private void ControlsChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged();
            }
        }
    }
}
