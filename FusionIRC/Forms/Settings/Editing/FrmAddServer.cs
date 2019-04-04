/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Settings.Networks;
using ircCore.Utils;

namespace FusionIRC.Forms.Settings.Editing
{
    public sealed class FrmAddServer : FormEx
    {
        private NetworkData _network;
        private ServerData _server;

        private readonly Label _lblAddress;
        private readonly TextBox _txtAddress;
        private readonly Label _lblPorts;
        private readonly TextBox _txtPorts;
        private readonly Label _lblDescription;
        private readonly TextBox _txtDescription;
        private readonly Label _lblPassword;
        private readonly TextBox _txtPassword;
        private readonly Label _lblNetwork;
        private readonly TextBox _txtNetwork;        

        private readonly Button _btnAdd;
        private readonly Button _btnCancel;

        public NetworkData Network
        {
            get { return _network; }
            set
            {
                _network = value;
                _txtNetwork.Text = _network.NetworkName;
            }
        }

        public ServerData Server
        {
            get { return _server; }
            set
            {
                _server = value;
                _txtAddress.Text = _server.Address;
                _txtPorts.Text = _server.PortRange;
                _txtDescription.Text = _server.Description;
                _txtPassword.Text = _server.Password;
            }
        }

        public FrmAddServer(ServerEditType serverEditType)
        {
            ClientSize = new Size(337, 274);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            _lblAddress = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(12, 9),
                                  Size = new Size(85, 15),
                                  Text = @"Server address:"
                              };

            _txtAddress = new TextBox
                              {
                                  Location = new Point(15, 27),
                                  Size = new Size(310, 23),
                                  TabIndex = 0
                              };

            _lblPorts = new Label
                            {
                                AutoSize = true,
                                BackColor = Color.Transparent,
                                Location = new Point(12, 53),
                                Size = new Size(216, 15),
                                Text = @"Port range (eg: '6667-6669,7000,+6697':)"
                            };

            _txtPorts = new TextBox
                            {
                                Location = new Point(15, 71),
                                Size = new Size(310, 23),
                                TabIndex = 1
                            };

            _lblDescription = new Label
                                  {
                                      AutoSize = true,
                                      BackColor = Color.Transparent,
                                      Location = new Point(12, 97),
                                      Size = new Size(127, 15),
                                      Text = @"Description (Optional):"
                                  };

            _txtDescription = new TextBox
                                  {
                                      Location = new Point(15, 115),
                                      Size = new Size(310, 23),
                                      TabIndex = 2
                                  };

            _lblPassword = new Label
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(12, 141),
                                   Size = new Size(263, 15),
                                   Text = @"Password (Some IRC servers require a password):"
                               };

            _txtPassword = new TextBox
                               {
                                   Location = new Point(15, 159),
                                   Size = new Size(310, 23),
                                   TabIndex = 3
                               };

            _lblNetwork = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(12, 185),
                                  Size = new Size(90, 15),
                                  Text = @"Network group:"
                              };

            _txtNetwork = new TextBox
                              {
                                  Location = new Point(15, 203),
                                  Size = new Size(310, 23),
                                  TabIndex = 4
                              };

            _btnAdd = new Button
                          {
                              DialogResult = DialogResult.OK,
                              Location = new Point(169, 239),
                              Size = new Size(75, 23),
                              TabIndex = 5,
                              Tag = "ADD",
                              Text = serverEditType == ServerEditType.Add ? @"Add" : @"Edit",
                              UseVisualStyleBackColor = true
                          };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(250, 239),
                                 Size = new Size(75, 23),
                                 TabIndex = 6,
                                 Tag = "CANCEL",
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[]
                                  {
                                      _lblAddress, _txtAddress, _lblPorts, _txtPorts, _lblDescription, _txtDescription,
                                      _lblPassword, _txtPassword, _lblNetwork, _txtNetwork, _btnAdd, _btnCancel
                                  });

            AcceptButton = _btnAdd;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(_txtAddress.Text))
                {
                    e.Cancel = true;
                    return;
                }
                _server.Address = Functions.GetFirstWord(_txtAddress.Text);
                _server.PortRange = !string.IsNullOrEmpty(_txtPorts.Text) ? Functions.GetFirstWord(_txtPorts.Text) : "6667";
                _server.Description = !string.IsNullOrEmpty(_txtDescription.Text) ? _txtDescription.Text : null;
                _server.Password = !string.IsNullOrEmpty(_txtPassword.Text) ? Functions.GetFirstWord(_txtPassword.Text) : null;
                _network.NetworkName = !string.IsNullOrEmpty(_txtNetwork.Text) ? Functions.GetFirstWord(_txtNetwork.Text) : "Unknown";
            }
            base.OnFormClosing(e);
        }
    }
}
