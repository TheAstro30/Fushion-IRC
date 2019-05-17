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

namespace FusionIRC.Forms.Misc
{
    public sealed class FrmException : FormEx
    {                
        private readonly Panel _pnlIcon;
        private readonly Label _lblHeader;
        private readonly TextBox _txtException;
        private readonly Button _btnClose;

        public FrmException(Exception e)
        {
            ClientSize = new Size(477, 203);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = @"FusionIRC - Internal Error";

            _pnlIcon = new Panel
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(12, 12),
                               Size = new Size(48, 48),
                               BackgroundImageLayout = ImageLayout.Center,
                               BackgroundImage = Resources.error.ToBitmap()
                           };

            _lblHeader = new Label
                             {
                                 AutoSize = true,
                                 BackColor = Color.Transparent,
                                 Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, ((0))),
                                 Location = new Point(66, 12),
                                 Size = new Size(272, 25),
                                 Text = @"Oops! Something went wrong!"
                             };

            _txtException = new TextBox
                                {
                                    BackColor = Color.White,
                                    Location = new Point(71, 40),
                                    Multiline = true,
                                    ReadOnly = true,
                                    ScrollBars = ScrollBars.Vertical,
                                    Size = new Size(394, 117),
                                    TabIndex = 0
                                };

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Location = new Point(390, 168),
                                Size = new Size(75, 23),
                                TabIndex = 1,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_pnlIcon, _lblHeader, _txtException, _btnClose});

            AcceptButton = _btnClose;

            _txtException.Text = string.Format("{0}{1}{2}", e.Message, Environment.NewLine, e.StackTrace);
            _txtException.SelectionStart = 0;
            _txtException.SelectionLength = 0;
        }
    }
}