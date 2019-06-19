/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;

namespace FusionIRC.Forms.Settings.Controls.Client
{
    public class ClientOptions : BaseControlRenderer, ISettings
    {
        private readonly ListView _lvOptions;

        public bool SettingsChanged { get; set; }

        public event Action<ISettings> OnSettingsChanged;

        public ClientOptions()
        {
            Header = "Client Options";

            _lvOptions = new ListView
                             {
                                 Location = new System.Drawing.Point(3, 39),
                                 MultiSelect = false,
                                 Size = new System.Drawing.Size(421, 305),
                                 TabIndex = 0,
                                 UseCompatibleStateImageBehavior = false,
                                 View = View.Details,
                                 HeaderStyle = ColumnHeaderStyle.None,
                                 BorderStyle = BorderStyle.FixedSingle,
                                 CheckBoxes = true
                             };
            _lvOptions.Columns.Add(new ColumnHeader {Width = 300});

            _lvOptions.Groups.AddRange(new[]
                                          {
                                              new ListViewGroup("General", HorizontalAlignment.Left),
                                              new ListViewGroup("Channels", HorizontalAlignment.Left),
                                              new ListViewGroup("Show...", HorizontalAlignment.Left),
                                              new ListViewGroup("Flash main window on...", HorizontalAlignment.Left)
                                          });

            /* Ugly code - thought about different ways I could make this cleaner... I just can't be fucked */
            _lvOptions.Items.AddRange(new[]
                                          {
                                              new ListViewItem("Show menubar")
                                                  {
                                                      Group = _lvOptions.Groups[0],
                                                      Checked =
                                                          SettingsManager.Settings.Client.Appearance.ControlBars.Control
                                                          [0].Visible
                                                  },
                                              new ListViewItem("Show toolbar")
                                                  {
                                                      Group = _lvOptions.Groups[0],
                                                      Checked =
                                                          SettingsManager.Settings.Client.Appearance.ControlBars.Control
                                                          [1].Visible
                                                  },
                                              new ListViewItem("Always keep channel windows open")
                                                  {
                                                      Group = _lvOptions.Groups[1],
                                                      Checked =
                                                          SettingsManager.Settings.Client.Channels.KeepChannelsOpen
                                                  },
                                              new ListViewItem("Re-join open channels on connect")
                                                  {
                                                      Group = _lvOptions.Groups[1],
                                                      Checked =
                                                          SettingsManager.Settings.Client.Channels.
                                                          JoinOpenChannelsOnConnect
                                                  },
                                              new ListViewItem("Re-join channels on kick")
                                                  {
                                                      Group = _lvOptions.Groups[1],
                                                      Checked =
                                                          SettingsManager.Settings.Client.Channels.ReJoinChannelsOnKick
                                                  },
                                              new ListViewItem("Auto-join channel on invite (not recommended)")
                                                  {
                                                      Group = _lvOptions.Groups[1],
                                                      Checked =
                                                          SettingsManager.Settings.Client.Channels.JoinChannelsOnInvite
                                                  },
                                              new ListViewItem("Show favorites dialog on connect")
                                                  {
                                                      Group = _lvOptions.Groups[1],
                                                      Checked =
                                                          SettingsManager.Settings.Client.Channels.
                                                          ShowFavoritesDialogOnConnect
                                                  },
                                              new ListViewItem("Client ping/pong event in console")
                                                  {
                                                      Group = _lvOptions.Groups[2],
                                                      Checked = SettingsManager.Settings.Client.Show.PingPong
                                                  },
                                              new ListViewItem("MOTD on connect")
                                                  {
                                                      Group = _lvOptions.Groups[2],
                                                      Checked = SettingsManager.Settings.Client.Show.Motd
                                                  },
                                              new ListViewItem("Notices in active window")
                                                  {
                                                      Group = _lvOptions.Groups[2],
                                                      Checked = SettingsManager.Settings.Client.Show.Notices
                                                  },
                                              new ListViewItem("CTCPs in active window")
                                                  {
                                                      Group = _lvOptions.Groups[2],
                                                      Checked = SettingsManager.Settings.Client.Show.Ctcps
                                                  },
                                              new ListViewItem("Channel messages")
                                                  {
                                                      Group = _lvOptions.Groups[3],
                                                      Checked = SettingsManager.Settings.Client.Flash.Channel
                                                  },
                                              new ListViewItem("Private messages")
                                                  {
                                                      Group = _lvOptions.Groups[3],
                                                      Checked = SettingsManager.Settings.Client.Flash.Private
                                                  },
                                              new ListViewItem("DCC chat messages")
                                                  {
                                                      Group = _lvOptions.Groups[3],
                                                      Checked = SettingsManager.Settings.Client.Flash.Chat
                                                  }
                                          });
            Controls.Add(_lvOptions);
        }

        protected override void OnLoad(EventArgs e)
        {
            _lvOptions.ItemChecked += ListViewCheckChanged;
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
            SettingsManager.Settings.Client.Appearance.ControlBars.Control[0].Visible = _lvOptions.Items[0].Checked;
            SettingsManager.Settings.Client.Appearance.ControlBars.Control[1].Visible = _lvOptions.Items[1].Checked;

            SettingsManager.Settings.Client.Channels.KeepChannelsOpen = _lvOptions.Items[2].Checked;
            SettingsManager.Settings.Client.Channels.JoinOpenChannelsOnConnect = _lvOptions.Items[3].Checked;
            SettingsManager.Settings.Client.Channels.ReJoinChannelsOnKick = _lvOptions.Items[4].Checked;
            SettingsManager.Settings.Client.Channels.JoinChannelsOnInvite = _lvOptions.Items[5].Checked;
            SettingsManager.Settings.Client.Channels.ShowFavoritesDialogOnConnect = _lvOptions.Items[6].Checked;

            SettingsManager.Settings.Client.Show.PingPong = _lvOptions.Items[7].Checked;
            SettingsManager.Settings.Client.Show.Motd = _lvOptions.Items[8].Checked;
            SettingsManager.Settings.Client.Show.Notices = _lvOptions.Items[9].Checked;
            SettingsManager.Settings.Client.Show.Ctcps = _lvOptions.Items[10].Checked;

            SettingsManager.Settings.Client.Flash.Channel = _lvOptions.Items[11].Checked;
            SettingsManager.Settings.Client.Flash.Private = _lvOptions.Items[12].Checked;
            SettingsManager.Settings.Client.Flash.Chat = _lvOptions.Items[13].Checked;
        }
    }
}
