/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;
using FusionIRC.Helpers;
using ircClient;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.Channels;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms.Misc
{
    public partial class FrmChannelList : FormEx
    {
        private readonly ClientConnection _client;

        public FrmChannelList(ClientConnection client)
        {
            InitializeComponent();

            _client = client;
            /* Match */
            var l = SettingsManager.Settings.Client.Channels.ChannelList.Match;
            cmbMatch.Items.AddRange(l.ToArray());
            if (cmbMatch.Items.Count > 0)
            {
                cmbMatch.SelectedIndex = 0;               
            }
            cmbMatch.DataSource = l;            
            /* Min/max */
            txtMinimum.Text = SettingsManager.Settings.Client.Channels.ChannelList.Minimum.ToString();
            txtMaximum.Text = SettingsManager.Settings.Client.Channels.ChannelList.Maximum.ToString();
            /* Cache */
            var net = !string.IsNullOrEmpty(_client.Network) ? _client.Network : _client.Server.Address;
            var n = ChannelManager.Channels.ChannelList;
            if (string.IsNullOrEmpty(net))
            {
                net = "Unknown";
            }            
            ChannelListBase d = null;
            if (n.Count > 0)
            {
                for (var i = 0; i <= n.Count - 1; i++)
                {
                    if (!n[i].Network.Equals(net, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }                    
                    d = n[i];
                    break;
                }
            }
            if (d == null)
            {
                d = new ChannelListBase {Network = net};
                n.Add(d);   
                n.Sort();
            }
            cmbCache.DataSource = n;
            cmbCache.SelectedItem = d;
            /* Handlers */
            btnClearMatch.Click += ButtonClickHandler;
            btnClearCache.Click += ButtonClickHandler;
            btnCache.Click += ButtonClickHandler;
            btnList.Click += ButtonClickHandler;            
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var l = SettingsManager.Settings.Client.Channels.ChannelList.Match;
            foreach (var m in l.Where(m => m.Equals(cmbMatch.Text, StringComparison.InvariantCultureIgnoreCase)))
            {
                l.Remove(m);
                break;
            }
            if (!string.IsNullOrEmpty(cmbMatch.Text))
            {
                l.Insert(0, cmbMatch.Text);
            }
            int i;
            if (!int.TryParse(txtMinimum.Text, out i))
            {
                i = 3;
            }
            SettingsManager.Settings.Client.Channels.ChannelList.Minimum = i;
            if (!int.TryParse(txtMaximum.Text, out i))
            {
                i = 1000;
            }
            SettingsManager.Settings.Client.Channels.ChannelList.Maximum = i;
            ChannelManager.Save();
            base.OnFormClosing(e);
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Tag.ToString())
            {
                case "CLEARMATCH":
                    cmbMatch.DataSource = null;
                    var l = SettingsManager.Settings.Client.Channels.ChannelList.Match;
                    SettingsManager.Settings.Client.Channels.ChannelList.Match.Clear();
                    cmbMatch.DataSource = l;
                    cmbMatch.Text = string.Empty;
                    break;

                case "CLEARCACHE":
                    var net = !string.IsNullOrEmpty(_client.Network) ? _client.Network : _client.Server.Address;
                    var n = ChannelManager.Channels.ChannelList;
                    if (string.IsNullOrEmpty(net))
                    {
                        net = "Unknown";
                    }
                    cmbCache.DataSource = null;
                    n.Clear();
                    n.Add(new ChannelListBase{Network = net});
                    cmbCache.DataSource = n;
                    cmbCache.SelectedIndex = 0;
                    break;

                case "CACHE":
                    /* Attempt to load list from cache */
                    var d = (ChannelListBase) cmbCache.SelectedItem;
                    if (d == null || d.List.Count == 0)
                    {
                        /* Do a list call instead */
                        GetList();
                        return;
                    }
                    var w = WindowManager.GetWindow(_client, "channel list") ??
                            WindowManager.AddWindow(_client, ChildWindowType.ChanList, WindowManager.MainForm,
                                                    string.Format("{0} - Channel List: {1} listed", cmbCache.Text,
                                                                  d.List.Count), "Channel List", true);
                    w.ChanList.Channels = d.List;
                    w.ChanList.SetObjects(d.List);
                    SystemSounds.Beep.Play();
                    break;

                case "LIST":
                    GetList();
                    break;
            }
        }

        private void GetList()
        {
            int min;
            if (!int.TryParse(txtMinimum.Text, out min))
            {
                min = 3;
            }           
            int max;
            if (!int.TryParse(txtMaximum.Text, out max))
            {
                max = 1000;
            }
            _client.Send(string.IsNullOrEmpty(cmbMatch.Text)
                             ? string.Format("LIST >{0},<{1}", min, max)
                             : string.Format("LIST >{0},<{1},{2}", min, max, cmbMatch.Text));
        }
    }
}
