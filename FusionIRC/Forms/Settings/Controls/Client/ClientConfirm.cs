/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.SettingsBase.Structures.Client;
using ircCore.Utils;

namespace FusionIRC.Forms.Settings.Controls.Client
{
    public partial class ClientConfirm : BaseControlRenderer, ISettings
    {
        public event Action<ISettings> OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ClientConfirm()
        {
            InitializeComponent();

            Header = "Confirmation";

            cmbConsole.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(CloseConfirmation)));
            cmbConsole.SelectedIndex = (int) SettingsManager.Settings.Client.Confirmation.ConsoleClose;

            cmbApp.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(CloseConfirmation)));
            cmbApp.SelectedIndex = (int) SettingsManager.Settings.Client.Confirmation.ClientClose;

            chkUrl.Checked = SettingsManager.Settings.Client.Confirmation.Url;
            chkPaste.Checked = SettingsManager.Settings.Client.Confirmation.ConfirmPaste;
            txtPaste.Text = SettingsManager.Settings.Client.Confirmation.PasteLines.ToString();

            cmbConsole.SelectedIndexChanged += ControlsChanged;
            cmbApp.SelectedIndexChanged += ControlsChanged;
            chkUrl.CheckedChanged += ControlsChanged;
            chkPaste.CheckedChanged += ControlsChanged;
            txtPaste.TextChanged += ControlsChanged;
        }        

        public void SaveSettings()
        {
            SettingsManager.Settings.Client.Confirmation.ConsoleClose = (CloseConfirmation) cmbConsole.SelectedIndex;
            SettingsManager.Settings.Client.Confirmation.ClientClose = (CloseConfirmation) cmbApp.SelectedIndex;
            SettingsManager.Settings.Client.Confirmation.Url = chkUrl.Checked;
            SettingsManager.Settings.Client.Confirmation.ConfirmPaste = chkPaste.Checked;
            int i;
            if (!int.TryParse(txtPaste.Text, out i))
            {
                i = 10;
            }
            SettingsManager.Settings.Client.Confirmation.PasteLines = i;
        }

        /* Callback */
        private void ControlsChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }
    }
}
