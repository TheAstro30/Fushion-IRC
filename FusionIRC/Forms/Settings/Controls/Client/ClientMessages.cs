/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;

namespace FusionIRC.Forms.Settings.Controls.Client
{
    public partial class ClientMessages : BaseControlRenderer, ISettings
    {
        public event Action OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ClientMessages()
        {
            InitializeComponent();

            Header = "Messages";

            chkStrip.Checked = SettingsManager.Settings.Client.Messages.StripCodes;
            txtQuit.Text = SettingsManager.Settings.Client.Messages.QuitMessage;
            txtPart.Text = SettingsManager.Settings.Client.Messages.PartMessage;
            txtFinger.Text = SettingsManager.Settings.Client.Messages.FingerReply;
            txtCommand.Text = SettingsManager.Settings.Client.Messages.CommandCharacter;

            chkStrip.CheckedChanged += ControlsChanged;
            txtQuit.TextChanged += ControlsChanged;
            txtPart.TextChanged += ControlsChanged;
            txtFinger.TextChanged += ControlsChanged;
            txtCommand.TextChanged += ControlsChanged;

        }
       
        public void SaveSettings()
        {
            SettingsManager.Settings.Client.Messages.StripCodes = chkStrip.Checked;
            SettingsManager.Settings.Client.Messages.QuitMessage = txtQuit.Text;
            SettingsManager.Settings.Client.Messages.PartMessage = txtPart.Text;
            SettingsManager.Settings.Client.Messages.FingerReply = txtFinger.Text;
            SettingsManager.Settings.Client.Messages.CommandCharacter = txtCommand.Text;
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
