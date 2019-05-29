/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Controls.ChildWindows.Input.ColorBox;

namespace ircScript.Forms
{
    public sealed class FrmInput : FormEx
    {
        private readonly Label _lblPrompt;
        private readonly ColorTextBox _txtInput;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public string Prompt
        {
            set { _lblPrompt.Text = value; }
        }

        public string InputText
        {
            get { return _txtInput.Text; }
            set
            {
                _txtInput.Text = value;
                _txtInput.SelectionStart = _txtInput.TextLength;
            }
        }

        public FrmInput()
        {
            ClientSize = new Size(338, 112);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = @"FusionIRC - Input Request";

            _lblPrompt = new Label
                             {
                                 BackColor = Color.Transparent,
                                 Location = new Point(12, 9),
                                 Size = new Size(314, 30),
                                 Text = @"Prompt:"
                             };

            _txtInput = new ColorTextBox
                            {
                                Location = new Point(12, 42),
                                ProcessCodes = true,
                                Size = new Size(314, 23),
                                TabIndex = 0
                            };

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(170, 77),
                             Size = new Size(75, 23),
                             TabIndex = 1,
                             Text = @"OK",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(251, 77),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblPrompt, _txtInput, _btnOk, _btnCancel});

            AcceptButton = _btnOk;
        }
    }
}
