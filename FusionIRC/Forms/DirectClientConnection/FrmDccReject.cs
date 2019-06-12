/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Properties;
using ircCore.Controls;
using ircCore.Settings;

namespace FusionIRC.Forms.DirectClientConnection
{
    public sealed class FrmDccReject : FormEx
    {
        private readonly Panel _pnlIcon;        
        private readonly Label _lblInfo;
        private readonly Label _lblFilename;
        private readonly Label _lblNickname;
        private readonly Label _lblSize;
        private readonly CheckBox _chkShow;
        private readonly Button _btnClose;

        public string FileName
        {
            set { _lblFilename.Text = string.Format("Filename: {0}", value); }
        }

        public string NickName
        {
            set { _lblNickname.Text = string.Format("Nick: {0}", value); }
        }

        public string FileSize
        {
            set { _lblSize.Text = string.Format("Size: {0}", value); }
        }

        public FrmDccReject()
        {
            ClientSize = new Size(347, 155);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"FusionIRC - DCC file rejected ";

            _pnlIcon = new Panel
                           {
                               BackColor = Color.Transparent,
                               BackgroundImageLayout = ImageLayout.Center,
                               Location = new Point(12, 12),
                               Size = new Size(64, 64),
                               TabIndex = 0
                           };

            _lblInfo = new Label
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(82, 12),
                               Size = new Size(253, 34),
                               Text = @"Because of your DCC filter settings, the following file has been rejected:"
                           };

            _lblFilename = new Label
                               {
                                   BackColor = Color.Transparent,
                                   Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                                   Location = new Point(82, 46),
                                   Size = new Size(253, 15)
                               };

            _lblNickname = new Label
                               {
                                   BackColor = Color.Transparent,
                                   Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                                   Location = new Point(82, 70),
                                   Size = new Size(253, 15)
                               };

            _lblSize = new Label
                           {
                               BackColor = Color.Transparent,
                               Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                               Location = new Point(82, 94),
                               Size = new Size(253, 15)
                           };

            _chkShow = new CheckBox
                           {
                               AutoSize = true,
                               Location = new Point(12, 123),
                               Size = new Size(152, 19),
                               TabIndex = 0,
                               Text = @"Always show this dialog",
                               UseVisualStyleBackColor = true
                           };

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Location = new Point(260, 120),
                                Size = new Size(75, 23),
                                TabIndex = 1,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_pnlIcon, _lblInfo, _lblFilename, _lblNickname, _lblSize, _chkShow, _btnClose});

            AcceptButton = _btnClose;

            _pnlIcon.BackgroundImage = Resources.dccManager.ToBitmap();
            _chkShow.Checked = SettingsManager.Settings.Dcc.Options.Filter.ShowRejectionDialog;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SettingsManager.Settings.Dcc.Options.Filter.ShowRejectionDialog = _chkShow.Checked;
            base.OnFormClosing(e);
        }
    }
}
