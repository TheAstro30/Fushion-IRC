/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using FusionIRC.Controls;
using FusionIRC.Helpers;
using ircCore.Settings;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms
{
    public partial class FrmConnectTo : FormEx
    {
        private readonly Form _owner;
        private bool _isSsl;

        private string _address;
        private int _port;        

        public FrmConnectTo(Form owner)
        {
            InitializeComponent();
            _owner = owner;
            /* Set up recent information */
            _isSsl = SettingsManager.Settings.Connection.IsSsl;
            var port = SettingsManager.Settings.Connection.Port;
            if (port == 0)
            {
                port = 6667;
            }
            txtAddress.Text = SettingsManager.Settings.Connection.Address;
            txtAddress.SelectionStart = txtAddress.Text.Length;
            txtPort.Text = _isSsl ? string.Format("+{0}", port) : port.ToString();
            txtChannels.Text = SettingsManager.Settings.Connection.Channels;
            chkNewWindow.Checked = SettingsManager.Settings.Connection.NewWindow;

            btnConnect.Click += ButtonClickHandler;
            btnClose.Click += ButtonClickHandler;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SettingsManager.Settings.Connection.Address = _address;
            SettingsManager.Settings.Connection.Port = _port;
            SettingsManager.Settings.Connection.IsSsl = _isSsl;
            SettingsManager.Settings.Connection.Channels = txtChannels.Text;
            SettingsManager.Settings.Connection.NewWindow = chkNewWindow.Checked;
            base.OnFormClosing(e);
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null || string.IsNullOrEmpty(txtAddress.Text))
            {
                Close();
                return;
            }
            /* Get address and port */
            var address = txtAddress.Text.Split(' ');
            if (address.Length == 0)
            {
                Close();
                return;
            }
            _address = address[0];
            string[] port = null;
            if (!string.IsNullOrEmpty(txtPort.Text))
            {
                port = txtPort.Text.Split(' ');
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
                    if (chkNewWindow.Checked)
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
                    var connection = !string.IsNullOrEmpty(txtChannels.Text)
                                         ? string.Format("SERVER {0}:{1} -j {2}", _address, port[0], txtChannels.Text)
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
