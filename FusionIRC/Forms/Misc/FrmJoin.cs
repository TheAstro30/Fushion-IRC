/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;

namespace FusionIRC.Forms.Misc
{
    public sealed class FrmJoin : FormEx
    {
        private readonly Panel _pnlIcon;
        private readonly Label _lblInfo;
        private readonly TextBox _txtChannels;
        private readonly Button _btnJoin;
        private readonly Button _btnClose;
        
        public string Channels { get { return _txtChannels.Text; } }

        public FrmJoin()
        {
            ClientSize = new Size(413, 151);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Join Channel(s)";

            _pnlIcon = new Panel
                           {
                               BackColor = Color.Transparent,
                               BackgroundImageLayout = ImageLayout.Center,
                               Location = new Point(12, 12),
                               Size = new Size(64, 64),
                               BackgroundImage = Properties.Resources.joinChan.ToBitmap()
                           };

            _lblInfo = new Label
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(82, 12),
                               Size = new Size(319, 34),
                               Text = @"Enter the channel(s) you wish to join separated by commas (eg: #AllNiteCafe,#new2irc)"
                           };

            _txtChannels = new TextBox
                               {
                                   Location = new Point(85, 53),
                                   Multiline = true,
                                   ScrollBars = ScrollBars.Vertical,
                                   Size = new Size(316, 44),
                                   TabIndex = 0
                               };

            _btnJoin = new Button
                           {
                               DialogResult = DialogResult.OK,
                               Location = new Point(245, 116),
                               Size = new Size(75, 23),
                               TabIndex = 1,
                               Text = @"Join",
                               UseVisualStyleBackColor = true
                           };

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.Cancel,
                                Location = new Point(326, 116),
                                Size = new Size(75, 23),
                                TabIndex = 2,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_pnlIcon, _lblInfo, _txtChannels, _btnJoin, _btnClose});

            AcceptButton = _btnJoin;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK && string.IsNullOrEmpty(_txtChannels.Text))
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }
    }
}
