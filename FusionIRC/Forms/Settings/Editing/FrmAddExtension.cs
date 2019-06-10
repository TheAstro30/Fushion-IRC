/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;

namespace FusionIRC.Forms.Settings.Editing
{
    public sealed class FrmAddExtension : FormEx
    {
        private readonly Label _lblExtension;
        private readonly TextBox _txtExtension;
        private readonly Button _btnAdd;
        private readonly Button _btnCancel;

        public string Extension
        {
            get { return _txtExtension.Text; }
        }

        public FrmAddExtension()
        {
            ClientSize = new Size(286, 83);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Add Filter Extension";

            _lblExtension = new Label
                                {
                                    AutoSize = true,
                                    BackColor = Color.Transparent,
                                    Location = new Point(12, 15),
                                    Size = new Size(140, 15),
                                    Text = "New extension (eg: *.avi):"
                                };

            _txtExtension = new TextBox
                                {
                                    Location = new Point(158, 12),
                                    MaxLength = 12,
                                    Size = new Size(116, 23),
                                    TabIndex = 0
                                };

            _btnAdd = new Button
                          {
                              DialogResult = DialogResult.OK,
                              Location = new Point(118, 48),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Text = @"Add",
                              UseVisualStyleBackColor = true
                          };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(199, 48),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblExtension, _txtExtension, _btnAdd, _btnCancel});

            AcceptButton = _btnAdd;
        }
    }
}
