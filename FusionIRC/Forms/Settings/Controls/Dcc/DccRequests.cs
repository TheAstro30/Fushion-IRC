/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Utils;

namespace FusionIRC.Forms.Settings.Controls.Dcc
{
    public partial class DccRequests : BaseControlRenderer, ISettings
    {
        public event Action<ISettings> OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public DccRequests()
        {
            InitializeComponent();

            Header = "DCC Requests";

            rbGetShow.Checked = SettingsManager.Settings.Dcc.Requests.GetRequest == DccRequestAction.Ask;
            rbGetAuto.Checked = SettingsManager.Settings.Dcc.Requests.GetRequest == DccRequestAction.AutoAccept;
            rbGetIgnore.Checked = SettingsManager.Settings.Dcc.Requests.GetRequest == DccRequestAction.Ignore;

            cmbExists.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(DccFileExistsAction)));
            cmbExists.SelectedIndex = (int) SettingsManager.Settings.Dcc.Requests.GetFileExists;

            rbChatShow.Checked = SettingsManager.Settings.Dcc.Requests.ChatRequest == DccRequestAction.Ask;
            rbChatAuto.Checked = SettingsManager.Settings.Dcc.Requests.ChatRequest == DccRequestAction.AutoAccept;
            rbChatIgnore.Checked = SettingsManager.Settings.Dcc.Requests.ChatRequest == DccRequestAction.Ignore;

            rbGetShow.CheckedChanged += ControlsChanged;
            rbGetAuto.CheckedChanged += ControlsChanged;
            rbGetIgnore.CheckedChanged += ControlsChanged;
            cmbExists.SelectedIndexChanged += ControlsChanged;

            rbChatShow.CheckedChanged += ControlsChanged;
            rbChatAuto.CheckedChanged += ControlsChanged;
            rbChatIgnore.CheckedChanged += ControlsChanged;
        }
                
        public void SaveSettings()
        {
            SettingsManager.Settings.Dcc.Requests.GetRequest = rbGetShow.Checked
                                                                   ? DccRequestAction.Ask
                                                                   : rbGetAuto.Checked
                                                                         ? DccRequestAction.AutoAccept
                                                                         : DccRequestAction.Ignore;

            SettingsManager.Settings.Dcc.Requests.GetFileExists = (DccFileExistsAction) cmbExists.SelectedIndex;

            SettingsManager.Settings.Dcc.Requests.ChatRequest = rbChatShow.Checked
                                                                    ? DccRequestAction.Ask
                                                                    : rbChatAuto.Checked
                                                                          ? DccRequestAction.AutoAccept
                                                                          : DccRequestAction.Ignore;
            SettingsChanged = false;
        }

        /* Callbacks */
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
