/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Utils;

namespace FusionIRC.Forms.Settings.Controls.Connection
{
    public partial class ClientLogging : BaseControlRenderer, ISettings
    {
        public event Action OnSettingsChanged;

        public bool SettingsChanged { get; set; }
        
        public ClientLogging()
        {
            InitializeComponent();

            Header = "Logging";

            txtPath.Text = SettingsManager.Settings.Client.Logging.LogPath;

            cmbKeep.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(LoggingType)));
            cmbKeep.SelectedIndex = (int) SettingsManager.Settings.Client.Logging.KeepLogsType;

            cmbReload.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(LoggingType)));
            cmbReload.SelectedIndex = (int)SettingsManager.Settings.Client.Logging.ReloadLogsType;

            chkFolder.Checked = SettingsManager.Settings.Client.Logging.CreateFolder;
            chkDate.Checked = SettingsManager.Settings.Client.Logging.DateByDay;
            chkStrip.Checked = SettingsManager.Settings.Client.Logging.StripCodes;

            txtPath.TextChanged += ControlsChanged;
            btnPath.Click += ButtonClickHandler;
            cmbKeep.SelectedIndexChanged += ControlsChanged;
            cmbReload.SelectedIndexChanged += ControlsChanged;
            chkFolder.CheckedChanged += ControlsChanged;
            chkDate.CheckStateChanged += ControlsChanged;
            chkStrip.CheckedChanged += ControlsChanged;
        }

        public void SaveSettings()
        {
            SettingsManager.Settings.Client.Logging.LogPath = txtPath.Text;
            SettingsManager.Settings.Client.Logging.KeepLogsType = (LoggingType) cmbKeep.SelectedIndex;
            SettingsManager.Settings.Client.Logging.ReloadLogsType = (LoggingType) cmbReload.SelectedIndex;
            SettingsManager.Settings.Client.Logging.CreateFolder = chkFolder.Checked;
            SettingsManager.Settings.Client.Logging.DateByDay = chkDate.Checked;
            SettingsManager.Settings.Client.Logging.StripCodes = chkStrip.Checked;
            SettingsChanged = false;
        }

        /* Callbacks */
        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var path = Functions.MainDir(txtPath.Text, false);
            using (var f = new FolderBrowserDialog())
            {
                f.Description = @"Select the folder path to save log files to:";
                f.SelectedPath = path;
                if (f.ShowDialog(this) == DialogResult.Cancel)
                {
                    return;
                }
                txtPath.Text = Functions.MainDir(f.SelectedPath, false);
            }
        }

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
