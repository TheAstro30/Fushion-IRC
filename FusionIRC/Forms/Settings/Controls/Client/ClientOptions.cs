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
    public partial class ClientOptions : BaseControlRenderer, ISettings
    {
        public bool SettingsChanged { get; set; }

        public event Action<ISettings> OnSettingsChanged;

        public ClientOptions()
        {
            InitializeComponent();

            Header = "Client Options";

            /* Ugly code */
            lvOptions.Items[0].Checked = SettingsManager.Settings.Client.Appearance.ControlBars.Control[0].Visible;
            lvOptions.Items[1].Checked = SettingsManager.Settings.Client.Appearance.ControlBars.Control[1].Visible;

            lvOptions.Items[2].Checked = SettingsManager.Settings.Client.Channels.KeepChannelsOpen;
            lvOptions.Items[3].Checked = SettingsManager.Settings.Client.Channels.JoinOpenChannelsOnConnect;
            lvOptions.Items[4].Checked = SettingsManager.Settings.Client.Channels.ReJoinChannelsOnKick;
            lvOptions.Items[5].Checked = SettingsManager.Settings.Client.Channels.JoinChannelsOnInvite;
            lvOptions.Items[6].Checked = SettingsManager.Settings.Client.Channels.ShowFavoritesDialogOnConnect;

            lvOptions.Items[7].Checked = SettingsManager.Settings.Client.Show.PingPong;
            lvOptions.Items[8].Checked = SettingsManager.Settings.Client.Show.Motd;
            lvOptions.Items[9].Checked = SettingsManager.Settings.Client.Show.Notices;
            lvOptions.Items[10].Checked = SettingsManager.Settings.Client.Show.Ctcps;

            lvOptions.Items[11].Checked = SettingsManager.Settings.Client.Flash.Channel;
            lvOptions.Items[12].Checked = SettingsManager.Settings.Client.Flash.Private;
            lvOptions.Items[13].Checked = SettingsManager.Settings.Client.Flash.Chat;
        }

        protected override void OnLoad(EventArgs e)
        {
            lvOptions.ItemChecked += ListViewCheckChanged;
            base.OnLoad(e);
        }

        private void ListViewCheckChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }
               
        public void SaveSettings()
        {
            /* Ugly code... */
            SettingsManager.Settings.Client.Appearance.ControlBars.Control[0].Visible = lvOptions.Items[0].Checked;
            SettingsManager.Settings.Client.Appearance.ControlBars.Control[1].Visible = lvOptions.Items[1].Checked;

            SettingsManager.Settings.Client.Channels.KeepChannelsOpen = lvOptions.Items[2].Checked;
            SettingsManager.Settings.Client.Channels.JoinOpenChannelsOnConnect = lvOptions.Items[3].Checked;
            SettingsManager.Settings.Client.Channels.ReJoinChannelsOnKick = lvOptions.Items[4].Checked;
            SettingsManager.Settings.Client.Channels.JoinChannelsOnInvite = lvOptions.Items[5].Checked;
            SettingsManager.Settings.Client.Channels.ShowFavoritesDialogOnConnect = lvOptions.Items[6].Checked;

            SettingsManager.Settings.Client.Show.PingPong = lvOptions.Items[7].Checked;
            SettingsManager.Settings.Client.Show.Motd = lvOptions.Items[8].Checked;
            SettingsManager.Settings.Client.Show.Notices = lvOptions.Items[9].Checked;
            SettingsManager.Settings.Client.Show.Ctcps = lvOptions.Items[10].Checked;

            SettingsManager.Settings.Client.Flash.Channel = lvOptions.Items[11].Checked;
            SettingsManager.Settings.Client.Flash.Private = lvOptions.Items[12].Checked;
            SettingsManager.Settings.Client.Flash.Chat = lvOptions.Items[13].Checked;
        }
    }
}
