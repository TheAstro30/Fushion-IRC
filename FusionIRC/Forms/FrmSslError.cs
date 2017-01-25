/* Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using FusionIRC.Controls;
using FusionIRC.Properties;
using ircCore.Controls;

namespace FusionIRC.Forms
{
    public sealed class FrmSslError : FormEx
    {
        private readonly Panel _pnlIcon;
        private readonly Label _lblServer;
        private readonly Label _lblInfo;
        private readonly Label _lblAccept;
        private readonly CheckBox _chkAutoAccept;
        private readonly Button _btnAccept;
        private readonly Button _btnCancel;
        
        private X509Certificate _certificate;
        private int _timeOut;

        private readonly Timer _tmrTimeOut;

        /* Properties */
        public X509Store CertificateStore { get; set; }

        public X509Certificate Certificate
        {
            set
            {
                _certificate = value;
            }
        }

        public string Server
        {
            set
            {
                _lblServer.Text = string.Format(@"Connecting: {0}", value);
            }
        }

        /* Constructor */
        public FrmSslError()
        {
            _pnlIcon = new Panel
                           {
                               BackColor = Color.Transparent,
                               BackgroundImage = Resources.security.ToBitmap(),
                               BackgroundImageLayout = ImageLayout.Center,
                               Location = new Point(12, 9),
                               Size = new Size(48, 48)
                           };

            _lblServer = new Label
                             {
                                 BackColor = Color.Transparent,
                                 Location = new Point(73, 9),
                                 Size = new Size(349, 34),
                                 Text = @"Connecting:"
                             };

            _lblInfo = new Label
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(73, 45),
                               Size = new Size(349, 55),
                               Text = @"Any data exchanged via this connection is encrypted cannot be viewed or changed by anyone else. Except, there is a problem with the certificate and the details cannot be verified."
                           };

            _lblAccept = new Label
                             {
                                 AutoSize = true,
                                 BackColor = Color.Transparent,
                                 Location = new Point(73, 109),
                                 Size = new Size(208, 15),
                                 Text = @"Do you want to accept this certificate?"
                             };

            _chkAutoAccept = new CheckBox
                                 {
                                     AutoSize = true,
                                     BackColor = Color.Transparent,
                                     Location = new Point(76, 142),
                                     Size = new Size(283, 19),
                                     TabIndex = 0,
                                     Text = @"Automatically accept this certificate in the future",
                                     UseVisualStyleBackColor = false
                                 };

            _btnAccept = new Button
                             {
                                 DialogResult = DialogResult.OK,
                                 Location = new Point(266, 176),
                                 Size = new Size(75, 23),
                                 TabIndex = 1,
                                 Text = @"Accept",
                                 UseVisualStyleBackColor = true
                             };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(347, 176),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };
            /* Add controls */
            Controls.AddRange(new Control[] {_pnlIcon, _lblServer, _lblInfo, _lblAccept, _chkAutoAccept, _btnAccept, _btnCancel});
            /* Form initialization */
            AcceptButton = this._btnAccept;
            ClientSize = new Size(434, 211);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;            
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"SSL Certificate Error";
            
            _tmrTimeOut = new Timer {Interval = 1000, Enabled = true};
            _tmrTimeOut.Tick += TimerTimeOut;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_chkAutoAccept.Checked)
            {
                return;
            }
            CertificateStore.Add((X509Certificate2)_certificate);
            base.OnFormClosing(e);
        }

        private void TimerTimeOut(object sender, EventArgs e)
        {
            _timeOut++;
            if (_timeOut < 30)
            {
                return;
            }
            _tmrTimeOut.Enabled = false;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
