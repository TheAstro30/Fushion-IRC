/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using System.Drawing;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;
using ircCore.Utils;

namespace FusionIRC.Forms.Settings.Controls.Connection
{
    public class ConnectionLocalInfo : BaseControlRenderer, ISettings
    {
        private Label _lblInfo;
        private Label _lblLookup;
        private ComboBox _cmbLookup;
        private Label _lblAddress;
        private TextBox _txtAddress;
        private Label _lblHost;
        private TextBox _txtHost;
        
        public event Action OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ConnectionLocalInfo()
        {
            InitializeComponent();

            Header = "Local Information";

            _cmbLookup.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(LocalInfoLookupMethod)));
            _cmbLookup.SelectedIndex = (int) SettingsManager.Settings.Connection.LocalInfo.LookupMethod;

            _txtAddress.Text = SettingsManager.Settings.Connection.LocalInfo.HostInfo.Address;
            _txtHost.Text = SettingsManager.Settings.Connection.LocalInfo.HostInfo.HostName;

            _cmbLookup.SelectedIndexChanged += ControlsChanged;
            _txtAddress.TextChanged += ControlsChanged;
            _txtHost.TextChanged += ControlsChanged;
        }

        public void SaveSettings()
        {
            SettingsManager.Settings.Connection.LocalInfo.LookupMethod = (LocalInfoLookupMethod) _cmbLookup.SelectedIndex;
            SettingsManager.Settings.Connection.LocalInfo.HostInfo.Address = Functions.GetFirstWord(_txtAddress.Text);
            SettingsManager.Settings.Connection.LocalInfo.HostInfo.HostName = Functions.GetFirstWord(_txtHost.Text);
            SettingsManager.Save();
            SettingsChanged = false;
        }

        private void InitializeComponent()
        {
            _lblInfo = new Label
                           {
                               Location = new Point(28, 77),
                               Size = new Size(371, 42),
                               Text = @"FusionIRC will attempt to gather this information on connection to an IRC server, so for now it can be left blank"
                           };

            _lblLookup = new Label
                             {
                                 AutoSize = true,
                                 Location = new Point(28, 129),
                                 Text = @"Look-up method:"
                             };

            _cmbLookup = new ComboBox
                             {
                                 DropDownStyle = ComboBoxStyle.DropDownList,
                                 FormattingEnabled = true,
                                 Location = new Point(31, 147),
                                 Size = new Size(245, 23),
                                 TabIndex = 1
                             };

            _lblAddress = new Label
                              {
                                  AutoSize = true,
                                  Location = new Point(28, 188),
                                  Size = new Size(65, 15),
                                  Text = @"IP Address:"
                              };

            _txtAddress = new TextBox
                              {
                                  Location = new Point(31, 206),
                                  Size = new Size(245, 23),
                                  TabIndex = 0
                              };

            _lblHost = new Label
                           {
                               AutoSize = true,
                               Location = new Point(28, 246),
                               Size = new Size(68, 15),
                               Text = @"Host name:"
                           };

            _txtHost = new TextBox
                           {
                               Location = new Point(31, 264),
                               Size = new Size(368, 23),
                               TabIndex = 2
                           };

            Controls.AddRange(new Control[] {_lblInfo, _lblLookup, _cmbLookup, _lblAddress, _txtAddress, _lblHost, _txtHost});
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
