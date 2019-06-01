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
using FusionIRC.Helpers.Commands;
using ircClient;
using ircCore.Controls;
using ircCore.Controls.ChildWindows.Classes.Channels;
using ircCore.Controls.ChildWindows.Input.ColorBox;
using ircCore.Utils;

namespace FusionIRC.Forms.ChannelProperties
{
    public sealed class FrmChannelProperties : FormEx
    {
        private readonly ClientConnection _client;
        private readonly string _chan;

        private readonly Button _btnClose;
        private readonly Label _lblTopic;
        private readonly ColorTextBox _txtTopic;
        private readonly GroupBox _gbModes;                                       
        private readonly CheckBox _chkLimit;
        private readonly TextBox _txtLimit;
        private readonly Label _lblUsers;
        private readonly CheckBox _chkKey;
        private readonly ColorTextBox _txtKey;       
        private readonly CheckBox _chkPrivate;
        private readonly CheckBox _chkSecret;                               
        private readonly CheckBox _chkTopic;
        private readonly CheckBox _chkMessages;
        private readonly CheckBox _chkInvite;
        private readonly CheckBox _chkModerated;
        private readonly TabControl _tabUsers;
        private readonly TabPage _tabBans;
        private readonly TabPage _tabExcepts;
        private readonly TabPage _tabInvites;
        private readonly Button _btnOk;

        private readonly string _originalTopic;
        private readonly List<ChannelCurrentModes> _originalModes = new List<ChannelCurrentModes>();
        private readonly string _originalKey;

        private readonly ChannelPropertyBase _pbBans;
        private readonly ChannelPropertyBase _pbExcepts;
        private readonly ChannelPropertyBase _pbInvites;

        public List<ChannelPropertyData> Bans = new List<ChannelPropertyData>();

        public List<ChannelPropertyData> Excepts = new List<ChannelPropertyData>();

        public List<ChannelPropertyData> Invites = new List<ChannelPropertyData>();

        public FrmChannelProperties(ClientConnection client, string channel, bool isOperator, ChannelBase modes)
        {
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ClientSize = new Size(382, 420);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Channel properties:";

            _lblTopic = new Label
                            {
                                AutoSize = true,
                                BackColor = Color.Transparent,
                                Location = new Point(9, 9),
                                Size = new Size(39, 15),
                                Text = @"Topic:"
                            };

            _txtTopic = new ColorTextBox
                            {
                                Location = new Point(12, 27),
                                ProcessCodes = true, 
                                Size = new Size(358, 23),
                                TabIndex = 0
                            };

            _gbModes = new GroupBox
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(12, 56),
                               Size = new Size(358, 123),
                               TabIndex = 1,
                               TabStop = false,
                               Text = @"Modes:"
                           };

            _chkTopic = new CheckBox
                            {
                                AutoSize = true,
                                Location = new Point(6, 22),
                                Size = new Size(147, 19),
                                TabIndex = 0,
                                Text = @"Only OPS set topic (+t)",
                                UseVisualStyleBackColor = true
                            };

            _chkMessages = new CheckBox
                               {
                                   AutoSize = true,
                                   Location = new Point(6, 47),
                                   Size = new Size(166, 19),
                                   TabIndex = 1,
                                   Text = @"No external messages (+n)",
                                   UseVisualStyleBackColor = true
                               };

            _chkInvite = new CheckBox
                             {
                                 AutoSize = true,
                                 Location = new Point(6, 72),
                                 Size = new Size(124, 19),
                                 TabIndex = 2,
                                 Text = @"Invitation only (+i)",
                                 UseVisualStyleBackColor = true
                             };

            _chkModerated = new CheckBox
                                {
                                    AutoSize = true,
                                    Location = new Point(6, 97),
                                    Size = new Size(114, 19),
                                    TabIndex = 3,
                                    Text = @"Moderated (+m)",
                                    UseVisualStyleBackColor = true
                                };

            _chkLimit = new CheckBox
                            {
                                AutoSize = true,
                                Location = new Point(179, 22),
                                Size = new Size(89, 19),
                                TabIndex = 4,
                                Text = @"Limit to (+l)",
                                UseVisualStyleBackColor = true
                            };

            _txtLimit = new TextBox
                            {
                                Location = new Point(274, 20),
                                Size = new Size(37, 23),
                                TabIndex = 5,
                                TextAlign = HorizontalAlignment.Center
                            };

            _lblUsers = new Label
                            {
                                AutoSize = true, 
                                Location = new Point(317, 23), 
                                Size = new Size(34, 15), 
                                Text = @"users"
                            };

            _chkKey = new CheckBox
                          {
                              AutoSize = true,
                              Location = new Point(179, 47),
                              Size = new Size(88, 19),
                              TabIndex = 6,
                              Text = @"Key set (+k)",
                              UseVisualStyleBackColor = true
                          };

            _txtKey = new ColorTextBox
                          {
                              ProcessCodes = true,
                              Location = new Point(274, 45),
                              Size = new Size(77, 23),
                              TabIndex = 7
                          };

            _chkPrivate = new CheckBox
                              {
                                  AutoSize = true,
                                  Location = new Point(179, 72),
                                  Size = new Size(88, 19),
                                  TabIndex = 8,
                                  Text = @"Private (+p)",
                                  UseVisualStyleBackColor = true
                              };

            _chkSecret = new CheckBox
                             {
                                 AutoSize = true,
                                 Location = new Point(179, 97),
                                 Size = new Size(82, 19),
                                 TabIndex = 9,
                                 Text = @"Secret (+s)",
                                 UseVisualStyleBackColor = true
                             };

            _gbModes.Controls.AddRange(new Control[]
                                          {
                                              _chkTopic, _chkMessages, _chkInvite, _chkModerated, _chkLimit, _txtLimit, _lblUsers,
                                              _chkKey, _txtKey, _chkPrivate, _chkSecret
                                          });

            _tabUsers = new TabControl
                            {
                                Location = new Point(12, 185),
                                SelectedIndex = 0,
                                Size = new Size(358, 193),
                                TabIndex = 2
                            };

            _tabBans = new TabPage
                           {
                               Location = new Point(4, 24),
                               Padding = new Padding(3),
                               Size = new Size(350, 165),
                               TabIndex = 0,
                               Text = @"Bans:",
                               UseVisualStyleBackColor = true
                           };

            _tabExcepts = new TabPage
                              {
                                  Location = new Point(4, 24),
                                  Padding = new Padding(3),
                                  Size = new Size(350, 165),
                                  TabIndex = 1,
                                  Text = @"Excepts:",
                                  UseVisualStyleBackColor = true
                              };

            _tabInvites = new TabPage
                              {
                                  Location = new Point(4, 24),
                                  Padding = new Padding(3),
                                  Size = new Size(350, 165),
                                  TabIndex = 2,
                                  Text = @"Invites:",
                                  UseVisualStyleBackColor = true
                              };

            _tabUsers.Controls.AddRange(new Control[] {_tabBans, _tabExcepts, _tabInvites});

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(214, 386),
                             Size = new Size(75, 23),
                             TabIndex = 3,
                             Text = @"OK",
                             UseVisualStyleBackColor = true
                         };

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.Cancel,
                                Location = new Point(295, 386),
                                Size = new Size(75, 23),
                                TabIndex = 4,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_lblTopic, _txtTopic, _gbModes, _tabUsers, _btnOk, _btnClose});

            AcceptButton = _btnOk;

            _client = client;
            _chan = channel;
            Text = string.Format("Channel properties: {0}", _chan);

            _pbBans = new ChannelPropertyBase(client, _chan, isOperator, Bans, 'b')
                          {
                              Dock = DockStyle.Fill,
                              Location = new Point(0, 0),
                              Size = new Size(350, 165)
                          };
            _tabBans.Controls.Add(_pbBans);

            _pbExcepts = new ChannelPropertyBase(client, _chan, isOperator, Excepts, 'e')
                             {
                                 Dock = DockStyle.Fill,
                                 Location = new Point(0, 0),
                                 Size = new Size(350, 165)
                             };
            _tabExcepts.Controls.Add(_pbExcepts);

            _pbInvites = new ChannelPropertyBase(client, _chan, isOperator, Invites, 'I')
                             {
                                 Dock = DockStyle.Fill,
                                 Location = new Point(0, 0),
                                 Size = new Size(350, 165)
                             };
            _tabInvites.Controls.Add(_pbInvites);
            
            _originalTopic = modes.Topic;
            _txtTopic.Text = _originalTopic;            

            foreach (var m in modes.Modes)
            {
                ChannelCurrentModes c = null;
                switch (m)
                {
                    case 't':
                        _chkTopic.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Topic};
                        break;

                    case 'n':
                        _chkMessages.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.NoExternalMessages};
                        break;

                    case 'i':
                        _chkInvite.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Invite};
                        break;

                    case 'm':
                        _chkModerated.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Moderated};
                        break;

                    case 'l':
                        _chkLimit.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Limit};
                        break;

                    case 'p':
                        _chkPrivate.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Private};
                        break;

                    case 's':
                        _chkSecret.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Secret};
                        break;

                    case 'k':
                        _chkKey.Checked = true;
                        c = new ChannelCurrentModes {Mode = ModeType.Key};
                        break;
                }
                if (c != null)
                {
                    _originalModes.Add(c);
                }
            }
            /* Set controls enable if operator of channel */
            SetControls(isOperator);

            _txtLimit.Enabled = isOperator && _chkLimit.Checked;
            _txtKey.Enabled = isOperator && _chkKey.Checked;

            _txtLimit.Text = modes.Limit > 0 ? modes.Limit.ToString() : string.Empty;
            _originalKey = modes.Key;
            _txtKey.Text = _originalKey;

            _chkLimit.CheckedChanged += CheckedChanged;
            _chkKey.CheckedChanged += CheckedChanged;
            _chkPrivate.CheckedChanged += CheckedChanged;
            _chkSecret.CheckedChanged += CheckedChanged;

            _btnOk.Click += ButtonClickHandler;
        }

        private void CheckedChanged(object sender, EventArgs e)
        {
            _txtLimit.Enabled = _chkLimit.Checked;
            _txtKey.Enabled = _chkKey.Checked;
            /* Cannot have private and secret at the same time */
            var chk = (CheckBox) sender;
            if (chk == null)
            {
                return;
            }
            if (chk.Text == @"Private (+p)" && _chkPrivate.Checked && _chkSecret.Checked)
            {
                _chkSecret.Checked = false;
            }
            if (chk.Text == @"Secret (+s)" && _chkSecret.Checked && _chkPrivate.Checked)
            {
                _chkPrivate.Checked = false;
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
            AppendModeFlag(ModeType.Topic, _chkTopic, ref s, 't');
            AppendModeFlag(ModeType.NoExternalMessages, _chkMessages, ref s, 'n');
            AppendModeFlag(ModeType.Invite, _chkInvite, ref s, 'i');
            AppendModeFlag(ModeType.Moderated, _chkModerated, ref s, 'm');
            AppendModeFlag(ModeType.Private, _chkPrivate, ref s, 'p');
            AppendModeFlag(ModeType.Secret, _chkSecret, ref s, 's');
            /* Limit and key is a little more difficult, we have to send it as +lk <n> <key> in order */
            var secondary = new StringBuilder();
            var word = Functions.GetFirstWord(_txtLimit.Text);
            if (_chkLimit.Checked != GetOriginalMode(ModeType.Limit))
            {
                s.Append(_chkLimit.Checked && !string.IsNullOrEmpty(word) ? "+l" : "-l");
                if (_chkLimit.Checked && !string.IsNullOrEmpty(word) && Functions.IsNumeric(word))
                {
                    secondary.Append(string.Format("{0} ", word));
                }
            }
            /* Key */
            if (_chkKey.Checked != GetOriginalMode(ModeType.Key))
            {
                s.Append(_chkKey.Checked ? "+k" : "-k");
                word = Functions.GetFirstWord(_txtKey.Text);
                if (_chkKey.Checked && !string.IsNullOrEmpty(word))
                {
                    secondary.Append(string.Format("{0} ", word));
                }
                else
                {
                    secondary.Append(string.Format("{0} ", _originalKey));
                }
            }
            /* Send modes to channel */
            CommandMode.ParseOddModes(_client, _chan, s.ToString(), secondary.ToString().Trim());
            /* Set topic */
            if (_txtTopic.Text.Equals(_originalTopic))
            {
                return; /* Nothing to do */
            }
            _client.Send(string.Format("TOPIC {0} :{1}", _chan, _txtTopic.Text));
        }

        /* Helper methods */
        private void SetControls(bool isOperator)
        {
            foreach (var c in from Control c in Controls where c.GetType() == typeof(ColorTextBox) || c.GetType() == typeof(GroupBox) select c)
            {
                c.Enabled = isOperator;
            }
        }

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
