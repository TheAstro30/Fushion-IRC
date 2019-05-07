/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;

namespace FusionIRC.Forms.Settings.Controls.Connection
{
    public partial class ConnectionOptions : BaseControlRenderer, ISettings
    {
        public event Action OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ConnectionOptions()
        {
            InitializeComponent();

            Header = "Connection Options";

            txtPort.Text = SettingsManager.Settings.Connection.Options.DefaultPort.ToString();
            chkRecon.Checked = SettingsManager.Settings.Connection.Options.Reconnect;
            gbOptions.Enabled = chkRecon.Checked;
            chkRetry.Checked = SettingsManager.Settings.Connection.Options.RetryConnection;
            txtRetry.Text = SettingsManager.Settings.Connection.Options.RetryTimes.ToString();
            /* Disable/enable controls */
            txtRetry.Enabled = chkRetry.Checked;
            lblRetry.Enabled = chkRetry.Checked;
            lblDelay.Enabled = chkRetry.Checked;
            txtDelay.Enabled = chkRetry.Checked;
            lblSecs.Enabled = chkRetry.Checked;
            chkNext.Enabled = chkRetry.Checked;

            txtDelay.Text = SettingsManager.Settings.Connection.Options.RetryDelay.ToString();
            chkNext.Checked = SettingsManager.Settings.Connection.Options.NextServer;

            txtPort.TextChanged += ControlsChanged;
            chkRecon.CheckedChanged += ControlsChanged;
            chkRetry.CheckedChanged += ControlsChanged;
            txtRetry.TextChanged += ControlsChanged;
            txtDelay.TextChanged += ControlsChanged;
            chkNext.CheckedChanged += ControlsChanged;
        }

        public void SaveSettings()
        {
            int i;
            if (!int.TryParse(txtPort.Text, out i))
            {
                i = 6667;
            }
            if (i == 0)
            {
                i = 6667;
            }
            SettingsManager.Settings.Connection.Options.DefaultPort = i;
            SettingsManager.Settings.Connection.Options.Reconnect = chkRecon.Checked;
            SettingsManager.Settings.Connection.Options.RetryConnection = chkRetry.Checked;
            if (!int.TryParse(txtRetry.Text, out i))
            {
                i = 1;
            }
            if (i == 0)
            {
                i = 1;
            }
            SettingsManager.Settings.Connection.Options.RetryTimes = i;
            if (!int.TryParse(txtDelay.Text, out i))
            {
                i = 1;
            }
            SettingsManager.Settings.Connection.Options.RetryDelay = i;
            SettingsManager.Settings.Connection.Options.NextServer = chkNext.Checked;
        }

        /* Callback */
        private void ControlsChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged();
            }
            if (sender.GetType() != typeof (CheckBox))
            {
                return;
            }
            var c = (CheckBox) sender;
            if (c.Text == @"Reconnect on disconnect")
            {
                gbOptions.Enabled = c.Checked;
            }
            if (c.Text != @"Retry connection:")
            {
                return;
            }
            txtRetry.Enabled = c.Checked;
            lblRetry.Enabled = c.Checked;
            lblDelay.Enabled = c.Checked;
            txtDelay.Enabled = c.Checked;
            lblSecs.Enabled = c.Checked;
            chkNext.Enabled = c.Checked;
        }
    }
}
