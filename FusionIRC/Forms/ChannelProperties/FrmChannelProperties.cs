/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FusionIRC.Forms.ChannelProperties.Controls;
using FusionIRC.Helpers;
using ircClient;
using ircCore.Controls;
using ircCore.Controls.ChildWindows.Classes.Channels;
using ircCore.Utils;

namespace FusionIRC.Forms.ChannelProperties
{
    public sealed partial class FrmChannelProperties : FormEx
    {
        private readonly ClientConnection _client;
        private readonly string _chan;

        private readonly string _originalTopic;
        private readonly List<ChannelCurrentModes> _originalModes = new List<ChannelCurrentModes>();
        private readonly string _originalKey;

        private readonly ChannelPropertyBase _pbBans;
        private readonly ChannelPropertyBase _pbExcepts;
        private readonly ChannelPropertyBase _pbInvites;

        public List<ChannelPropertyData> Bans = new List<ChannelPropertyData>();

        public List<ChannelPropertyData> Excepts = new List<ChannelPropertyData>();

        public List<ChannelPropertyData> Invites = new List<ChannelPropertyData>();

        public FrmChannelProperties(ClientConnection client, string channel, ChannelBase modes)
        {
            InitializeComponent();

            _client = client;
            _chan = channel;
            Text = string.Format("Channel properties: {0}", _chan);            

            _pbBans = new ChannelPropertyBase(ChannelPropertyType.Bans, Bans)
                          {
                              Dock = DockStyle.Fill,
                              Location = new Point(0, 0),
                              Size = new Size(350, 165)
                          };
            tabBans.Controls.Add(_pbBans);

            _pbExcepts = new ChannelPropertyBase(ChannelPropertyType.Excepts, Excepts)
                             {
                                 Dock = DockStyle.Fill,
                                 Location = new Point(0, 0),
                                 Size = new Size(350, 165)
                             };
            tabExcepts.Controls.Add(_pbExcepts);

            _pbInvites = new ChannelPropertyBase(ChannelPropertyType.Invites, Invites)
                             {
                                 Dock = DockStyle.Fill,
                                 Location = new Point(0, 0),
                                 Size = new Size(350, 165)
                             };
            tabInvites.Controls.Add(_pbInvites);
            
            _originalTopic = modes.Topic;
            txtTopic.Text = _originalTopic;            

            foreach (var m in modes.Modes)
            {
                ChannelCurrentModes c = null;
                switch (m)
                {
                    case 't':
                        chkTopic.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Topic};
                        break;

                    case 'n':
                        chkMessages.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.NoExternalMessages};
                        break;

                    case 'i':
                        chkInvite.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Invite};
                        break;

                    case 'm':
                        chkModerated.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Moderated};
                        break;

                    case 'l':
                        chkLimit.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Limit};
                        break;

                    case 'p':
                        chkPrivate.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Private};
                        break;

                    case 's':
                        chkSecret.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Secret};
                        break;

                    case 'k':
                        chkKey.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Key};
                        break;
                }
                if (c != null)
                {
                    _originalModes.Add(c);
                }
            }

            txtLimit.Enabled = chkLimit.Checked;
            txtKey.Enabled = chkKey.Checked;

            txtLimit.Text = modes.Limit > 0 ? modes.Limit.ToString() : string.Empty;
            _originalKey = modes.Key;
            txtKey.Text = _originalKey;

            chkLimit.CheckedChanged += CheckedChanged;
            chkKey.CheckedChanged += CheckedChanged;
            chkPrivate.CheckedChanged += CheckedChanged;
            chkSecret.CheckedChanged += CheckedChanged;

            btnOk.Click += ButtonClickHandler;
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            txtLimit.Enabled = chkLimit.Checked;
            txtKey.Enabled = chkKey.Checked;
            /* Cannot have private and secret at the same time */
            var chk = (CheckBox) sender;
            if (chk == null)
            {
                return;
            }
            if (chk.Text == @"Private (+p)" && chkPrivate.Checked && chkSecret.Checked)
            {
                chkSecret.Checked = false;
            }
            if (chk.Text == @"Secret (+s)" && chkSecret.Checked && chkPrivate.Checked)
            {
                chkPrivate.Checked = false;
            }
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            if (!_client.IsConnected)
            {
                return;
            }
            /* We need to build a list of modes to add/remove and send them to the server */
            var s = new StringBuilder();
            AppendModeFlag(ModeType.Topic, chkTopic, ref s, 't');
            AppendModeFlag(ModeType.NoExternalMessages, chkMessages, ref s, 'n');
            AppendModeFlag(ModeType.Invite, chkInvite, ref s, 'i');
            AppendModeFlag(ModeType.Moderated, chkModerated, ref s, 'm');
            AppendModeFlag(ModeType.Private, chkPrivate, ref s, 'p');
            AppendModeFlag(ModeType.Secret, chkSecret, ref s, 's');
            /* Limit and key is a little more difficult, we have to send it as +lk <n> <key> in order */
            var secondary = new StringBuilder();
            var word = Functions.GetFirstWord(txtLimit.Text);
            if (chkLimit.Checked != GetOriginalMode(ModeType.Limit))
            {
                s.Append(chkLimit.Checked && !string.IsNullOrEmpty(word) ? "+l" : "-l");
                if (chkLimit.Checked && !string.IsNullOrEmpty(word) && Functions.IsNumeric(word))
                {
                    secondary.Append(string.Format("{0} ", word));
                }
            }
            /* Key */
            if (chkKey.Checked != GetOriginalMode(ModeType.Key))
            {
                System.Diagnostics.Debug.Print("key");
                s.Append(chkKey.Checked ? "+k" : "-k");
                word = Functions.GetFirstWord(txtKey.Text);
                if (chkKey.Checked && !string.IsNullOrEmpty(word))
                {
                    System.Diagnostics.Debug.Print("new key " + word);
                    secondary.Append(string.Format("{0} ", word));
                }
                else
                {
                    System.Diagnostics.Debug.Print("old key");
                    secondary.Append(string.Format("{0} ", _originalKey));
                }
            }
            /* Send modes to channel */
            _client.Send(string.Format("MODE {0} {1} {2}", _chan, s, secondary.ToString().Trim()));
            /* Set topic */
            if (txtTopic.Text.Equals(_originalTopic))
            {
                return; /* Nothing to do */
            }
            _client.Send(string.Format("TOPIC {0} :{1}", _chan, txtTopic.Text));
        }

        /* Helper methods */
        private bool GetOriginalMode(ModeType mode)
        {
            return _originalModes.Any(m => m.Mode == mode);
        }

        private void AppendModeFlag(ModeType mode, CheckBox chk, ref StringBuilder s, char modeChar)
        {
            if (chk.Checked != GetOriginalMode(mode))
            {
                s.Append(chk.Checked ? string.Format("+{0}", modeChar) : string.Format("-{0}", modeChar));
            }
        }
    }
}
