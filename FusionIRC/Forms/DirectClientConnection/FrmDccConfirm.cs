/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Properties;
using ircCore.Controls;
using ircCore.Dcc;
using ircCore.Settings;

namespace FusionIRC.Forms.DirectClientConnection
{
    public sealed class FrmDccConfirm : FormEx
    {
        private readonly Panel _pnlIcon;
        private readonly Label _lblTitle;
        private readonly Label _lblFilename;
        private readonly Label _lblNickname;
        private readonly Label _lblInfo;
        private readonly Button _btnReject;
        private readonly Button _btnAccept;

        private readonly Timer _timer;
        private int _timeOut;

        public string FileName
        {
            set { _lblFilename.Text = string.Format("Filename: {0}", value); }
        }

        public string NickName
        {
            set { _lblNickname.Text = string.Format("Nick: {0}", value); }
        }

        public FrmDccConfirm(DccType type)
        {
            ClientSize = new Size(399, 190);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            _pnlIcon = new Panel
                           {
                               BackColor = Color.Transparent,
                               BackgroundImageLayout = ImageLayout.Center,
                               Location = new Point(12, 12),
                               Size = new Size(68, 68)
                           };

            _lblTitle = new Label
                            {
                                BackColor = Color.Transparent,
                                Location = new Point(86, 12), 
                                Size = new Size(301, 15)
                            };

            _lblFilename = new Label
                               {
                                   BackColor = Color.Transparent,
                                   Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                                   Location = new Point(86, 39),
                                   Size = new Size(301, 15)
                               };

            _lblNickname = new Label
                               {
                                   BackColor = Color.Transparent,
                                   Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                                   Location = new Point(86, 65),
                                   Size = new Size(301, 15)
                               };

            _lblInfo = new Label
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(86, 95),
                               Size = new Size(301, 45),
                               Text = @"Do you want to accept the request? Note: only accept DCC requests from people you know and only if you were expecting the request."
                           };

            _btnReject = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(231, 159),
                                 Size = new Size(75, 23),
                                 TabIndex = 0,
                                 Text = @"Reject",
                                 UseVisualStyleBackColor = true
                             };

            _btnAccept = new Button
                             {
                                 DialogResult = DialogResult.OK,
                                 Location = new Point(312, 159),
                                 Size = new Size(75, 23),
                                 TabIndex = 1,
                                 Text = @"Accept",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_pnlIcon, _lblTitle, _lblFilename, _lblNickname, _lblInfo, _btnReject, _btnAccept});
            AcceptButton = _btnReject;

            switch (type)
            {
                case DccType.DccChat:
                    Text = @"FusionIRC - DCC Chat request";
                    _lblTitle.Text = @"The following person is attempting to start a chat:";
                    _pnlIcon.BackgroundImage = Resources.dccChatRequest.ToBitmap();
                    _lblFilename.Visible = false;
                    _lblNickname.Location = new Point(86, 55);
                    break;

                case DccType.DccFileTransfer:
                    Text = @"FusionIRC - DCC Get request";
                    _lblTitle.Text = @"The following person is attempting to send you a file:";
                    _pnlIcon.BackgroundImage = Resources.dccManager.ToBitmap();
                    break;
            }
            _timer = new Timer {Interval = 1000, Enabled = true};
            _timer.Tick += TimeOutTick;
        }

        /* Private helper */
        private void TimeOutTick(object sender, EventArgs e)
        {            
            if (_timeOut >= SettingsManager.Settings.Dcc.Options.Timeouts.GetSendRequest)
            {
                _timer.Enabled = false;
                Close();
                return;
            }
            _timeOut++;
        }
    }
}
