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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircCore.Controls;
using ircScript.Controls.SyntaxHighlight.Commands;
using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Helpers.TextRange;

namespace ircScript.Controls.SyntaxHighlight.Forms
{
    public sealed class ReplaceForm : FormEx
    {
        private readonly FastColoredTextBox _tb;
        private readonly Button _btClose;
        private readonly Button _btFindNext;
        private readonly Button _btReplace;
        private readonly Button _btReplaceAll;
        private readonly CheckBox _cbMatchCase;
        private readonly CheckBox _cbRegex;
        private readonly CheckBox _cbWholeWord;
        private readonly Label _lblFind;
        private readonly Label _lbReplace;

        private bool _firstSearch = true;
        private Place _startPlace;

        public TextBox TextFind { get; set; }
        public TextBox TextReplace { get; set; }

        public ReplaceForm(FastColoredTextBox tb)
        {
            ClientSize = new Size(360, 176);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = @"Find/Replace";
            TopMost = true;

            _lblFind = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(23, 14),
                               Size = new Size(33, 13),
                               TabIndex = 5,
                               Text = @"Find:"
                           };

            TextFind = new TextBox
                           {
                               Location = new Point(62, 12),
                               Size = new Size(286, 20), 
                               TabIndex = 0
                           };

            _lbReplace = new Label
                             {
                                 AutoSize = true,
                                 BackColor = Color.Transparent,
                                 Location = new Point(6, 81),
                                 Size = new Size(50, 13),
                                 TabIndex = 9,
                                 Text = @"Replace:"
                             };

            TextReplace = new TextBox
                              {
                                  Location = new Point(62, 78), 
                                  Size = new Size(286, 20), 
                                  TabIndex = 0
                              };

            _cbWholeWord = new CheckBox
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(154, 38),
                                   Size = new Size(113, 17),
                                   TabIndex = 2,
                                   Text = @"Match whole word",
                                   UseVisualStyleBackColor = false
                               };

            _cbMatchCase = new CheckBox
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(66, 38),
                                   Size = new Size(82, 17),
                                   TabIndex = 1,
                                   Text = @"Match case",
                                   UseVisualStyleBackColor = false
                               };

            _cbRegex = new CheckBox
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(273, 38),
                               Size = new Size(57, 17),
                               TabIndex = 3,
                               Text = @"Regex",
                               UseVisualStyleBackColor = false
                           };

            _btFindNext = new Button
                              {
                                  Location = new Point(111, 104),
                                  Size = new Size(75, 23),
                                  TabIndex = 5,
                                  Text = @"Find Next",
                                  UseVisualStyleBackColor = true
                              };

            _btReplace = new Button
                             {
                                 Location = new Point(192, 104),
                                 Size = new Size(75, 23),
                                 TabIndex = 6,
                                 Text = @"Replace",
                                 UseVisualStyleBackColor = true
                             };

            _btReplaceAll = new Button
                                {
                                    Location = new Point(273, 104),
                                    Size = new Size(75, 23),
                                    TabIndex = 7,
                                    Text = @"Replace All",
                                    UseVisualStyleBackColor = true
                                };

            _btClose = new Button
                           {
                               Location = new Point(273, 141),
                               Size = new Size(75, 23),
                               TabIndex = 8,
                               Text = @"Close",
                               UseVisualStyleBackColor = true
                           };

            Controls.AddRange(new Control[]
                                  {
                                      _lblFind, TextFind, _lbReplace, TextReplace, _cbMatchCase, _cbWholeWord, _cbRegex, _btFindNext,
                                      _btReplace, _btReplaceAll, _btClose
                                  });

            AcceptButton = _btFindNext;
            _tb = tb;

            TextFind.TextChanged += OptionsChanged;
            TextFind.KeyPress += TextFindKeyPress;
            TextReplace.TextChanged += OptionsChanged;
            TextReplace.KeyPress += TextFindKeyPress;

            _cbWholeWord.CheckedChanged += OptionsChanged;
            _cbMatchCase.CheckedChanged += OptionsChanged;
            _cbRegex.CheckedChanged += OptionsChanged;

            _btFindNext.Click += ButtonClickHandler;
            _btReplace.Click += ButtonClickHandler;
            _btReplaceAll.Click += ButtonClickHandler;
            _btClose.Click += ButtonClickHandler;
        }

        /* Overrides */
        protected override void OnActivated(EventArgs e)
        {
            TextFind.Focus();
            ResetSerach();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) // David
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            _tb.Focus();
        }

        /* Handlers */
        private void TextFindKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                FindNext();
            }
            if (e.KeyChar == '\x1b')
            {
                Hide();
            }
        }

        private void OptionsChanged(object sender, EventArgs e)
        {
            ResetSerach();
        }

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
                    FindNext();
                    break;

                case "REPLACE":
                    try
                    {
                        if (_tb.SelectionLength != 0)
                            if (!_tb.Selection.ReadOnly)
                            {
                                _tb.InsertText(TextReplace.Text);
                            }
                        FindNext();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    break;

                case "REPLACE ALL":
                    try
                    {
                        _tb.Selection.BeginUpdate();
                        /* Search */
                        var ranges = FindAll(TextFind.Text);
                        /* check readonly */
                        bool ro = ranges.Any(r => r.ReadOnly);
                        /* replace */
                        if (!ro)
                        {
                            if (ranges.Count > 0)
                            {
                                _tb.TextSource.Manager.ExecuteCommand(new ReplaceTextCommand(_tb.TextSource, ranges,
                                                                                             TextReplace.Text));
                                _tb.Selection.Start = new Place(0, 0);
                            }
                        }
                        _tb.Invalidate();
                        MessageBox.Show(string.Format("{0} occurrence(s) replaced", ranges.Count), @"Find/Replace",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        Debug.Assert(true);
                    }
                    break;

                case "CLOSE":
                    Close();
                    break;
            }
        }

        /* Public methods */
        public List<Range> FindAll(string pattern)
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
            var range = _tb.Selection.IsEmpty ? _tb.Range.Clone() : _tb.Selection.Clone();
            return range.GetRangesByLines(pattern, opt).ToList();
        }

        public bool Find(string pattern)
        {
            var opt = _cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
            if (!_cbRegex.Checked)
            {
                pattern = Regex.Escape(pattern);
            }
            if (_cbWholeWord.Checked)
            {
                pattern = "\\b" + pattern + "\\b";
            }
            var range = _tb.Selection.Clone();
            range.Normalize();
            if (_firstSearch)
            {
                _startPlace = range.Start;
                _firstSearch = false;
            }
            range.Start = range.End;
            range.End = range.Start >= _startPlace
                            ? new Place(_tb.GetLineLength(_tb.LinesCount - 1), _tb.LinesCount - 1)
                            : _startPlace;
            foreach (var r in range.GetRangesByLines(pattern, opt))
            {
                _tb.Selection.Start = r.Start;
                _tb.Selection.End = r.End;
                _tb.DoSelectionVisible();
                _tb.Invalidate();
                return true;
            }
            if (range.Start >= _startPlace && _startPlace > Place.Empty)
            {
                _tb.Selection.Start = new Place(0, 0);
                return Find(pattern);
            }
            return false;
        }

        /* Private helper methods */
        private void FindNext()
        {
            try
            {
                if (!Find(TextFind.Text))
                {
                    MessageBox.Show(string.Format(@"Text ""{0}"" not found.", TextFind.Text), @"Text Not Found",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Exclamation);
                }
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        private void ResetSerach()
        {
            _firstSearch = true;
        }
    }
}