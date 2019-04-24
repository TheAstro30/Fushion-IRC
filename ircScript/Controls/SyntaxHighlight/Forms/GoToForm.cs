//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System;
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;

namespace ircScript.Controls.SyntaxHighlight.Forms
{
    public sealed class GoToForm : FormEx
    {
        private readonly int _totalLineCount;
        private readonly Button _btnCancel;
        private readonly Button _btnOk;
        private readonly Label _lblLine;
        private readonly TextBox _tbLineNumber;

        public GoToForm(int selected, int count)
        {
            SelectedLineNumber = selected;
            _totalLineCount = count;

            ClientSize = new Size(320, 106);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Go To Line";
            TopMost = true;

            _lblLine = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(12, 9),
                               Size = new Size(96, 13),
                               Text = string.Format("Line number (1 - {0}):", _totalLineCount)
                           };

            _tbLineNumber = new TextBox
                                {
                                    Anchor = ((((AnchorStyles.Top | AnchorStyles.Left)
                                                | AnchorStyles.Right))),
                                    Location = new Point(12, 29),
                                    Size = new Size(296, 20),
                                    TabIndex = 0,
                                    Text = SelectedLineNumber.ToString()
                                };

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(152, 71),
                             Size = new Size(75, 23),
                             TabIndex = 1,
                             Text = @"OK",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(233, 71),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblLine, _tbLineNumber, _btnOk, _btnCancel});

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;

            _btnOk.Click += ButtonClickHandler;
            _btnCancel.Click += ButtonClickHandler;
        }

        public int SelectedLineNumber { get; set; }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Text.ToUpper())
            {
                case "OK":
                    int enteredLine;
                    if (int.TryParse(_tbLineNumber.Text, out enteredLine))
                    {
                        enteredLine = Math.Min(enteredLine, _totalLineCount);
                        enteredLine = Math.Max(1, enteredLine);

                        SelectedLineNumber = enteredLine;
                    }

                    DialogResult = DialogResult.OK;
                    break;

                case "Close":
                    DialogResult = DialogResult.Cancel;
                    break;
            }
            Close();
        }
    }
}