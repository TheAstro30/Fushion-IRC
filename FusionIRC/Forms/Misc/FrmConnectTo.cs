/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Child;
using FusionIRC.Helpers;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms.Misc
{
    public sealed class FrmConnectTo : FormEx
    {
        private readonly GroupBox _gbCredentials;
        private readonly Label _lblUserInfo;
        private readonly Label _lblNick;
        private readonly TextBox _txtNick;
        private readonly Label _lblAlternative;
        private readonly TextBox _txtAlternate;
        private readonly Label _lblIdent;
        private readonly TextBox _txtIdent;
        private readonly Label _lblRealName;
        private readonly TextBox _txtRealName;
        private readonly CheckBox _chkInvisible;                                                                      

        private readonly GroupBox _gbServer;
        private readonly Label _lblServerInfo;
        private readonly Label _lblAddress;
        private readonly TextBox _txtAddress;
        private readonly Label _lblPort;
        private readonly TextBox _txtPort;
        private readonly Label _lblChannels;
        private readonly TextBox _txtChannels;
        private readonly CheckBox _chkNewWindow;

        private readonly Button _btnConnect;
        private readonly Button _btnClose;
        
        private FrmChildWindow _console;

        private string _address;
        private int _port;
        private readonly Form _owner;        
        private bool _isSsl;

        public FrmConnectTo(Form owner, FrmChildWindow console)
        {
            ClientSize = new Size(436, 414);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Connect To Location";

            _gbCredentials = new GroupBox
                                 {
                                     BackColor = Color.Transparent,
                                     Location = new Point(12, 12),
                                     Size = new Size(412, 164),
                                     TabIndex = 2,
                                     TabStop = false,
                                     Text = @"User information:"
                                 };

            _lblUserInfo = new Label
                               {
                                   AutoSize = true,
                                   Location = new Point(6, 19),
                                   Size = new Size(262, 15),
                                   TabIndex = 3,
                                   Text = @"The following fields identify you to an IRC server"
                               };


            _lblNick = new Label
                           {
                               AutoSize = true,
                               Location = new Point(6, 48),
                               Size = new Size(64, 15),
                               TabIndex = 4,
                               Text = @"Nickname:"
                           };

            _txtNick = new TextBox
                           {
                               Enabled = false,
                               Location = new Point(9, 66),
                               Size = new Size(195, 23),
                               TabIndex = 5
                           };

            _lblAlternative = new Label
                                  {
                                      AutoSize = true,
                                      Location = new Point(206, 48),
                                      Size = new Size(185, 15),
                                      TabIndex = 6,
                                      Text = @"Alternative (if nickname is in use):"
                                  };

            _txtAlternate = new TextBox
                                {
                                    Enabled = false, 
                                    Location = new Point(209, 66), 
                                    Size = new Size(195, 23), 
                                    TabIndex = 7};

            _lblIdent = new Label
                            {
                                AutoSize = true,
                                Location = new Point(6, 92),
                                Size = new Size(77, 15),
                                TabIndex = 8,
                                Text = @"Ident (email):"
                            };

            _txtIdent = new TextBox
                            {
                                Enabled = false, 
                                Location = new Point(9, 110), 
                                Size = new Size(195, 23), 
                                TabIndex = 9
                            };

            _lblRealName = new Label
                               {
                                   AutoSize = true,
                                   Location = new Point(207, 92),
                                   Size = new Size(65, 15),
                                   TabIndex = 10,
                                   Text = @"Real name:"
                               };

            _txtRealName = new TextBox
                               {
                                   Enabled = false,
                                   Location = new Point(210, 110),
                                   Size = new Size(194, 23),
                                   TabIndex = 11
                               };

            _chkInvisible = new CheckBox
                                {
                                    AutoSize = true,
                                    Enabled = false,
                                    Location = new Point(9, 139),
                                    Size = new Size(125, 19),
                                    TabIndex = 12,
                                    Text = @"Invisible mode (+i)",
                                    UseVisualStyleBackColor = true
                                };

            _gbCredentials.Controls.AddRange(new Control[]
                                                 {
                                                     _lblUserInfo, _lblNick, _txtNick, _lblAlternative, _txtAlternate,
                                                     _lblIdent, _txtIdent, _lblRealName, _txtRealName, _chkInvisible
                                                 });

            _gbServer = new GroupBox
                            {
                                BackColor = Color.Transparent,
                                Location = new Point(12, 182),
                                Size = new Size(412, 180),
                                TabIndex = 13,
                                TabStop = false,
                                Text = @"Server:"
                            };

            _lblServerInfo = new Label
                                 {
                                     Location = new Point(6, 19),
                                     Size = new Size(398, 36),
                                     TabIndex = 14,
                                     Text = @"You can specify an IRC server to connect to here. Use '+' before the port to specify the server is SSL"
                                 };

            _lblAddress = new Label
                              {
                                  AutoSize = true,
                                  Location = new Point(6, 64),
                                  Size = new Size(85, 15),
                                  TabIndex = 15,
                                  Text = @"Server address:"
                              };

            _txtAddress = new TextBox
                              {
                                  Location = new Point(9, 82),
                                  Size = new Size(216, 23),
                                  TabIndex = 16
                              };

            _lblPort = new Label
                           {
                               AutoSize = true,
                               Location = new Point(228, 64),
                               Size = new Size(32, 15),
                               TabIndex = 17,
                               Text = @"Port:"
                           };

            _txtPort = new TextBox
                           {
                               Location = new Point(231, 82),
                               MaxLength = 5,
                               Size = new Size(70, 23),
                               TabIndex = 18
                           };

            _lblChannels = new Label
                               {
                                   AutoSize = true,
                                   Location = new Point(6, 108),
                                   Size = new Size(249, 15),
                                   TabIndex = 19,
                                   Text = @"Channels to join on connect (separated by ','):"
                               };

            _txtChannels = new TextBox
                               {
                                   Location = new Point(9, 126), 
                                   Size = new Size(395, 23), 
                                   TabIndex = 20
                               };


            _chkNewWindow = new CheckBox
                                {
                                    AutoSize = true,
                                    Location = new Point(9, 155),
                                    Size = new Size(158, 19),
                                    TabIndex = 21,
                                    Text = @"New connection window",
                                    UseVisualStyleBackColor = true
                                };

            _gbServer.Controls.AddRange(new Control[]
                                            {
                                                _lblServerInfo, _lblAddress, _txtAddress, _lblPort, _txtPort, _lblChannels,
                                                _txtChannels, _chkNewWindow
                                           });

            _btnConnect = new Button
                              {
                                  Location = new Point(268, 379),                                  
                                  Size = new Size(75, 23),
                                  TabIndex = 0,
                                  Tag = "CONNECT",
                                  Text = @"Connect",
                                  UseVisualStyleBackColor = true
                              };

            _btnClose = new Button
                            {
                                Location = new Point(349, 379),                                
                                Size = new Size(75, 23),
                                TabIndex = 1,
                                Tag = "CLOSE",
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_gbCredentials, _gbServer, _btnConnect, _btnClose});            

            AcceptButton = _btnConnect;
            _owner = owner;
            /* Set up recent information */
            _isSsl = SettingsManager.Settings.Connection.IsSsl;
            var port = SettingsManager.Settings.Connection.Port;
            if (port == 0)
            {
                port = 6667;
            }
            /* Current client connection */
            _console = console;           
            /* User information */
            _txtNick.Text = _console.Client.UserInfo.Nick;
            _txtAlternate.Text = _console.Client.UserInfo.Alternative;
            _txtIdent.Text = _console.Client.UserInfo.Ident;
            _txtRealName.Text = _console.Client.UserInfo.RealName;
            _chkInvisible.Checked = _console.Client.UserInfo.Invisible;
            /* Server information */
            _txtAddress.Text = SettingsManager.Settings.Connection.Address;
            _txtAddress.SelectionStart = _txtAddress.Text.Length;
            _txtPort.Text = _isSsl ? string.Format("+{0}", port) : port.ToString();
            _txtChannels.Text = SettingsManager.Settings.Connection.Channels;
            _chkNewWindow.Checked = SettingsManager.Settings.Connection.NewWindow;

            if ((!_console.Client.IsConnecting && !_console.Client.IsConnected) || _chkNewWindow.Checked)
            {
                _txtNick.Enabled = true;
                _txtAlternate.Enabled = true;
                _txtIdent.Enabled = true;
                _txtRealName.Enabled = true;
                _chkInvisible.Enabled = true;
            }
            else
            {
                _btnConnect.Text = @"Disconnect";
                _btnConnect.Tag = "DISCONNECT";
            }

            _chkNewWindow.CheckedChanged += CheckNewWindowHandler;
            _btnConnect.Click += ButtonClickHandler;
            _btnClose.Click += ButtonClickHandler;            
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                /* User information */
                _console.Client.UserInfo.Nick = Functions.GetFirstWord(_txtNick.Text);
                _console.Client.UserInfo.Alternative = Functions.GetFirstWord(_txtAlternate.Text);
                _console.Client.UserInfo.Ident = Functions.GetFirstWord(_txtIdent.Text);
                _console.Client.UserInfo.RealName = _txtRealName.Text.Trim();
                _console.Client.UserInfo.Invisible = _chkInvisible.Checked;
                SettingsManager.Settings.UserInfo = new SettingsUserInfo(_console.Client.UserInfo);
                /* Server information */
                SettingsManager.Settings.Connection.Address = _address;
                SettingsManager.Settings.Connection.Port = _port;
                SettingsManager.Settings.Connection.IsSsl = _isSsl;
                SettingsManager.Settings.Connection.Channels = _txtChannels.Text;
                SettingsManager.Settings.Connection.NewWindow = _chkNewWindow.Checked;
            }
            base.OnFormClosing(e);
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null || string.IsNullOrEmpty(_txtAddress.Text))
            {
                Close();
                return;
            }
            /* Get address and port */
            var address = Functions.GetFirstWord(_txtAddress.Text);
            if (string.IsNullOrEmpty(address))
            {
                Close();
                return;
            }
            _isSsl = false;
            _address = address;
            string[] port = null;
            if (!string.IsNullOrEmpty(_txtPort.Text))
            {
                port = _txtPort.Text.Split(' ');
                if (port.Length == 0)
                {
                    Close();
                    return;
                }
                var p = port[0];
                if (p[0] == '+')
                {
                    _isSsl = true;
                    p = p.Substring(1);
                }
                int t;
                _port = !int.TryParse(p, out t) ? 6667 : t;
            }
            else
            {
                _port = 6667;
            }
            if (port == null)
            {
                Close();
                return;
            }
            switch (btn.Tag.ToString())
            {
                case "CONNECT":
                    /* Cannot connect if user info is blank ... */
                    if (string.IsNullOrEmpty(_txtNick.Text) || string.IsNullOrEmpty(_txtIdent.Text) || string.IsNullOrEmpty(_txtRealName.Text))
                    {
                        return;
                    }
                    /* Either get the current connection or create a new one */
                    if (_chkNewWindow.Checked)
                    {
                        _console = WindowManager.AddWindow(null, ChildWindowType.Console, _owner, "Console", "Console", true);
                    }
                    var connection = !string.IsNullOrEmpty(_txtChannels.Text)
                                         ? string.Format("SERVER {0}:{1} -j {2}", _address, port[0], _txtChannels.Text)
                                         : string.Format("SERVER {0}:{1}", _address, port[0]);
                    CommandProcessor.Parse(_console.Client, _console, connection);
                    DialogResult = DialogResult.OK;
                    break;

                case "DISCONNECT":
                    if (_console == null)
                    {
                        return;
                    }
                    _console.Client.Send("QUIT :Leaving.");
                    _console.Client.Disconnect();
                    _txtNick.Enabled = true;
                    _txtAlternate.Enabled = true;
                    _txtIdent.Enabled = true;
                    _txtRealName.Enabled = true;
                    _chkInvisible.Enabled = true;
                    _btnConnect.Text = @"Connect";
                    _btnConnect.Tag = "CONNECT";
                    return;
            }
            Close();
        }

        private void CheckNewWindowHandler(object sender, EventArgs e)
        {
            if (!_console.Client.IsConnecting && !_console.Client.IsConnected)
            {
                return;
            }
            var c = _chkNewWindow.Checked;
            _txtNick.Enabled = c;
            _txtAlternate.Enabled = c;
            _txtIdent.Enabled = c;
            _txtRealName.Enabled = c;
            _chkInvisible.Enabled = c;
            _btnConnect.Text = c ? "Connect" : "Disconnect";
            _btnConnect.Tag = c ? "CONNECT" : "DISCONNECT";
        }
    }
}
