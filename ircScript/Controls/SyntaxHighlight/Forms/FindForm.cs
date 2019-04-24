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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircCore.Controls;
using ircScript.Controls.SyntaxHighlight.Helpers;

namespace ircScript.Controls.SyntaxHighlight.Forms
{
    public sealed class FindForm : FormEx
    {
        private readonly Button _btClose;
        private readonly Button _btFindNext;
        private readonly CheckBox _cbMatchCase;
        private readonly CheckBox _cbRegex;
        private readonly CheckBox _cbWholeWord;
        private readonly Label _lblFind;

        private bool _firstSearch = true;
        private Place _startPlace;

        public TextBox FindText { get; set; }

        public FindForm(FastColoredTextBox tb)
        {
            ClientSize = new Size(360, 108);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = @"Find";
            TopMost = true;
           
            _lblFind = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(6, 15),
                               Size = new Size(33, 13),
                               Text = @"Find:"
                           };

            FindText = new TextBox
                           {
                               Location = new Point(42, 12),
                               Size = new Size(306, 20),
                               TabIndex = 0
                           };

            _cbRegex = new CheckBox
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(249, 38),
                               Size = new Size(57, 17),
                               TabIndex = 1,
                               Text = @"Regex",
                               UseVisualStyleBackColor = false
                           };            

            _cbMatchCase = new CheckBox
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(42, 38),
                                   Size = new Size(82, 17),
                                   TabIndex = 2,
                                   Text = @"Match case",
                                   UseVisualStyleBackColor = false
                               };           

            _cbWholeWord = new CheckBox
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(130, 38),
                                   Size = new Size(113, 17),
                                   TabIndex = 3,
                                   Text = @"Match whole word",
                                   UseVisualStyleBackColor = false
                               };

            _btFindNext = new Button
                              {
                                  Location = new Point(192, 73),
                                  Size = new Size(75, 23),
                                  TabIndex = 4,
                                  Text = @"Find Next",
                                  UseVisualStyleBackColor = true
                              };

            _btClose = new Button
                           {
                               Location = new Point(273, 73),
                               Size = new Size(75, 23),
                               TabIndex = 5,
                               Text = @"Close",
                               UseVisualStyleBackColor = true
                           };
            
            Controls.AddRange(new Control[] {_lblFind, FindText, _cbWholeWord, _cbMatchCase, _cbRegex, _btFindNext, _btClose});

            AcceptButton = _btFindNext;

            TextBox = tb;

            FindText.TextChanged += FindOptionsChanged;
            _cbMatchCase.CheckedChanged += FindOptionsChanged;
            _cbWholeWord.CheckedChanged += FindOptionsChanged;
            _cbRegex.CheckedChanged += FindOptionsChanged;

            _btFindNext.Click += ButtonClickHandler;
            _btClose.Click += ButtonClickHandler;
        }

        public FastColoredTextBox TextBox { get; set; }

        /* Handlers */
        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Text.ToUpper())
            {
                case "FIND NEXT":
                    FindNext(FindText.Text);
                    break;

                case "CLOSE":
                    Close();
                    break;
            }
        }

        private void FindOptionsChanged(object sender, EventArgs e)
        {
            ResetSearch();
        }

        /* Overrides */
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                _btFindNext.PerformClick();
                e.Handled = true;
                return;
            }
            switch (e.KeyChar)
            {
                case '\x1b':
                    Hide();
                    e.Handled = true;
                    return;
            }
            base.OnKeyPress(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            TextBox.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnActivated(EventArgs e)
        {
            FindText.Focus();
            ResetSearch();
        }

        /* Find text method */
        public void FindNext(string pattern)
        {
            try
            {
                RegexOptions opt = _cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
                if (!_cbRegex.Checked)
                {
                    pattern = Regex.Escape(pattern);
                }
                if (_cbWholeWord.Checked)
                {
                    pattern = "\\b" + pattern + "\\b";
                }
                var range = TextBox.Selection.Clone();
                range.Normalize();
                if (_firstSearch)
                {
                    _startPlace = range.Start;
                    _firstSearch = false;
                }
                range.Start = range.End;
                range.End = range.Start >= _startPlace
                                ? new Place(TextBox.GetLineLength(TextBox.LinesCount - 1), TextBox.LinesCount - 1)
                                : _startPlace;
                foreach (var r in range.GetRangesByLines(pattern, opt))
                {
                    TextBox.Selection = r;
                    TextBox.DoSelectionVisible();
                    TextBox.Invalidate();
                    return;
                }
                if (range.Start >= _startPlace && _startPlace > Place.Empty)
                {
                    TextBox.Selection.Start = new Place(0, 0);
                    FindNext(pattern);
                    return;
                }
                MessageBox.Show(string.Format(@"Text ""{0}"" not found.", pattern), @"Text Not Found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ResetSearch()
        {
            _firstSearch = true;
        }
    }
}