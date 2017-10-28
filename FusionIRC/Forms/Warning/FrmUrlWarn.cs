/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Settings;

namespace FusionIRC.Forms.Warning
{
    public sealed class FrmUrlWarn : FormEx
    {
        private readonly Panel _pnlIcon;        
        private readonly Label _lblInfo1;
        private readonly TextBox _txtUrl;
        private readonly Label _lblInfo2;
        private readonly CheckBox _chkAlwaysShow;
        private readonly Button _btnOpen;
        private readonly Button _btnCancel;

        public string Url
        {
            set { _txtUrl.Text = value; }
            get { return _txtUrl.Text; }
        }

        public FrmUrlWarn()
        {                                    
            ClientSize = new Size(432, 210);            
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;            
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"URL Warning";

            _pnlIcon = new Panel
                          {
                              BackColor = Color.Transparent,
                              BackgroundImageLayout = ImageLayout.Center,
                              Location = new Point(12, 12),                              
                              Size = new Size(48, 48),                              
                              BackgroundImage = Properties.Resources.warning.ToBitmap()
                          };

            _lblInfo1 = new Label
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(66, 12),                               
                               Size = new Size(354, 56),                               
                               Text = @"Some URLs may contain viruses, trojans or other malicious content that may harm your computer. Only click on links from people you know and trust. The original link is shown below:"
                           };

            _txtUrl = new TextBox
                         {
                             Location = new Point(69, 71),
                             Multiline = true,
                             ScrollBars = ScrollBars.Vertical,
                             Size = new Size(351, 56),
                             TabIndex = 2
                         };

            _lblInfo2 = new Label
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(66, 140),
                               Size = new Size(354, 23),
                               Text = @"Are you sure you want to visit this website?"
                           };

            _chkAlwaysShow = new CheckBox
                                {
                                    AutoSize = true,
                                    Location = new Point(12, 185),
                                    Size = new Size(162, 19),
                                    TabIndex = 3,
                                    Text = @"Always show this warning",
                                    UseVisualStyleBackColor = true
                                };

            _btnOpen = new Button
                          {
                              DialogResult = DialogResult.OK,
                              Location = new Point(264, 182),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Text = @"Open",
                              UseVisualStyleBackColor = true
                          };

            _btnCancel = new Button
                            {
                                DialogResult = DialogResult.Cancel,
                                Location = new Point(345, 182),
                                Size = new Size(75, 23),
                                TabIndex = 0,
                                Text = @"Cancel",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] { _pnlIcon, _lblInfo1, _txtUrl, _lblInfo2, _chkAlwaysShow, _btnOpen, _btnCancel });

            AcceptButton = _btnCancel;
            _chkAlwaysShow.Checked = SettingsManager.Settings.Client.Confirmation.Url;
            _chkAlwaysShow.CheckedChanged += CheckAlwaysShow;
        }

        private void CheckAlwaysShow(object sender, EventArgs e)
        {
            SettingsManager.Settings.Client.Confirmation.Url = _chkAlwaysShow.Checked;
        }
    }
}
