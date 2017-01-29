/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Helpers;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms
{
    public sealed class FrmConnectTo : FormEx
    {
        private readonly Label _lblHeader;
        private readonly Label _lblAddress;
        private readonly TextBox _txtAddress;
        private readonly Label _lblPort;        
        private readonly TextBox _txtPort;
        private readonly Label _lblChannels;
        private readonly TextBox _txtChannels;
        private readonly CheckBox _chkNewWindow;
        private readonly Button _btnConnect;
        private readonly Button _btnClose;

        private readonly Form _owner;
        private bool _isSsl;

        private string _address;
        private int _port;        

        public FrmConnectTo(Form owner)
        {
            ClientSize = new Size(411, 215);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Connect To Location";

            _lblHeader = new Label
                             {
                                 BackColor = Color.Transparent,
                                 Location = new Point(12, 9),
                                 Size = new Size(387, 35),
                                 Text = @"You can specify an IRC server to connect to directly here. Use '+' before the port to specify the server is SSL"
                             };

            _lblAddress = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(12, 53),
                                  Size = new Size(85, 15),
                                  Text = @"Server address:"
                              };

            _txtAddress = new TextBox
                              {
                                  Location = new Point(15, 71),
                                  MaxLength = 200,
                                  Size = new Size(215, 23),
                                  TabIndex = 0
                              };

            _lblPort = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(233, 53),
                               Size = new Size(32, 15),
                               Text = @"Port:"
                           };

            _txtPort = new TextBox
                           {
                               Location = new Point(236, 71),
                               MaxLength = 6,
                               Size = new Size(66, 23),
                               TabIndex = 1
                           };

            _lblChannels = new Label
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(12, 97),
                                   Size = new Size(249, 15),
                                   Text = @"Channels to join on connect (separated by ','):"
                               };

            _txtChannels = new TextBox
                               {
                                   Location = new Point(15, 115),
                                   Size = new Size(384, 23),
                                   TabIndex = 2
                               };

            _chkNewWindow = new CheckBox
                                {
                                    AutoSize = true,
                                    BackColor = Color.Transparent,
                                    Location = new Point(15, 144),
                                    Size = new Size(158, 19),
                                    TabIndex = 3,
                                    Text = @"New connection window",
                                    UseVisualStyleBackColor = false
                                };

            _btnConnect = new Button
                              {
                                  Location = new Point(243, 180),
                                  Size = new Size(75, 23),
                                  TabIndex = 4,
                                  Tag = "CONNECT",
                                  Text = @"Connect",
                                  UseVisualStyleBackColor = true
                              };

            _btnClose = new Button
                            {
                                Location = new Point(324, 180),
                                Size = new Size(75, 23),
                                TabIndex = 5,
                                Tag = "CLOSE",
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_lblHeader, _lblAddress, _txtAddress, _lblPort, _txtPort, _lblChannels, _txtChannels, _chkNewWindow, _btnConnect, _btnClose});            

            AcceptButton = _btnConnect;

            _owner = owner;
            /* Set up recent information */
            _isSsl = SettingsManager.Settings.Connection.IsSsl;
            var port = SettingsManager.Settings.Connection.Port;
            if (port == 0)
            {
                port = 6667;
            }
            _txtAddress.Text = SettingsManager.Settings.Connection.Address;
            _txtAddress.SelectionStart = _txtAddress.Text.Length;
            _txtPort.Text = _isSsl ? string.Format("+{0}", port) : port.ToString();
            _txtChannels.Text = SettingsManager.Settings.Connection.Channels;
            _chkNewWindow.Checked = SettingsManager.Settings.Connection.NewWindow;

            _btnConnect.Click += ButtonClickHandler;
            _btnClose.Click += ButtonClickHandler;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SettingsManager.Settings.Connection.Address = _address;
            SettingsManager.Settings.Connection.Port = _port;
            SettingsManager.Settings.Connection.IsSsl = _isSsl;
            SettingsManager.Settings.Connection.Channels = _txtChannels.Text;
            SettingsManager.Settings.Connection.NewWindow = _chkNewWindow.Checked;
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
            var address = _txtAddress.Text.Split(' ');
            if (address.Length == 0)
            {
                Close();
                return;
            }
            _address = address[0];
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
                    /* Either get the current connection or create a new one */
                    FrmChildWindow c;
                    if (_chkNewWindow.Checked)
                    {
                        c = WindowManager.AddWindow(null, ChildWindowType.Console, _owner, "Console", "Console", true);
                    }
                    else
                    {
                        var client = WindowManager.GetActiveConnection(_owner);
                        c = WindowManager.GetConsoleWindow(client);
                    }
                    if (c == null)
                    {
                        return;
                    }
                    var connection = !string.IsNullOrEmpty(_txtChannels.Text)
                                         ? string.Format("SERVER {0}:{1} -j {2}", _address, port[0], _txtChannels.Text)
                                         : string.Format("SERVER {0}:{1}", _address, port[0]);
                    CommandProcessor.Parse(c.Client, c, connection);
                    Close();
                    break;

                case "CLOSE":
                    Close();
                    break;
            }
        }
    }
}
