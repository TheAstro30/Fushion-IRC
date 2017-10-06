/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Settings;

namespace FusionIRC.Forms.Child
{
    public sealed class FrmChatFind : FormEx
    {
        private readonly FrmChildWindow _child;

        private readonly Label _lblHeader;
        private readonly ComboBox _cmbSearch;
        private readonly GroupBox _gbDirection;
        private readonly RadioButton _rbDown;
        private readonly RadioButton _rbUp;
        private readonly GroupBox _gbOption;
        private readonly CheckBox _chkCase;
        private readonly Button _btnClear;
        private readonly Button _btnFind;

        public FrmChatFind(FrmChildWindow child)
        {
            _child = child;
            /* Build our form */            
            _lblHeader = new Label
                             {
                                 BackColor = Color.Transparent,
                                 Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                 Location = new Point(12, 9),
                                 Size = new Size(339, 39),
                                 Text =
                                     @"Enter the word in the search box below you want to look for in the active chat window:"
                             };

            _cmbSearch = new ComboBox
                             {
                                 Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                 FormattingEnabled = true,
                                 Location = new Point(15, 51),
                                 Size = new Size(258, 23),
                                 TabIndex = 0
                             };

            _rbUp = new RadioButton
                        {
                            AutoSize = true,
                            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                            Location = new Point(9, 24),
                            Size = new Size(40, 19),
                            TabIndex = 2,
                            TabStop = true,
                            Text = @"Up",
                            UseVisualStyleBackColor = true
                        };

            _rbDown = new RadioButton
                          {
                              AutoSize = true,
                              Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                              Location = new Point(58, 24),
                              Size = new Size(56, 19),
                              TabIndex = 3,
                              TabStop = true,
                              Text = @"Down",
                              UseVisualStyleBackColor = true
                          };

            _gbDirection = new GroupBox
                               {
                                   BackColor = Color.Transparent,
                                   Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                   Location = new Point(15, 82),
                                   Size = new Size(165, 55),
                                   TabStop = false,
                                   Text = @"Direction:"
                               };
            _gbDirection.Controls.AddRange(new Control[]{ _rbUp, _rbDown});

            _chkCase = new CheckBox
                           {
                               AutoSize = true,
                               Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                               Location = new Point(10, 24),
                               Size = new Size(86, 19),
                               TabIndex = 4,
                               Text = @"Match case",
                               UseVisualStyleBackColor = true
                           };

            _gbOption = new GroupBox
                            {
                                BackColor = Color.Transparent,
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                Location = new Point(189, 82),
                                Size = new Size(165, 55),
                                TabIndex = 3,
                                TabStop = false,
                                Text = @"Options:"
                            };
            _gbOption.Controls.Add(_chkCase);

            _btnClear = new Button
                            {
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                Location = new Point(279, 51),
                                Size = new Size(75, 23),
                                TabIndex = 2,
                                Tag = "CLEAR",
                                Text = @"Clear",
                                UseVisualStyleBackColor = true
                            };

            _btnFind = new Button
                           {
                               Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                               Location = new Point(279, 151),
                               Size = new Size(75, 23),
                               TabIndex = 5,
                               Tag = "FIND",
                               Text = @"Find",
                               UseVisualStyleBackColor = true
                           };

            Controls.AddRange(new Control[]
                                  {
                                      _lblHeader, _cmbSearch, _btnClear, _gbDirection,_gbOption,_btnFind
                                  });
            AcceptButton = _btnFind;
            ClientSize = new Size(366, 186);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Find Text";
            /* Initialize settings for each control */
            _cmbSearch.Items.AddRange(SettingsManager.Settings.Windows.Search.History.ToArray());
            if (_cmbSearch.Items.Count > 0)
            {
                _cmbSearch.SelectedIndex = 0;
            }
            switch (SettingsManager.Settings.Windows.Search.Direction)
            {
                case SearchDirection.Up:
                    _rbUp.Checked = true;
                    break;

                default:
                    _rbDown.Checked = true;
                    break;
            }
            _chkCase.Checked = SettingsManager.Settings.Windows.Search.MatchCase;

            _btnClear.Click += ButtonClickHandler;
            _btnFind.Click += ButtonClickHandler;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            /* Update current settings */
            SettingsManager.Settings.Windows.Search.History = new List<String>();            
            foreach (var s in _cmbSearch.Items)
            {
                SettingsManager.Settings.Windows.Search.History.Add(s.ToString());
            }

            SettingsManager.Settings.Windows.Search.Direction = _rbUp.Checked ? SearchDirection.Up : SearchDirection.Down;
            SettingsManager.Settings.Windows.Search.MatchCase = _chkCase.Checked;
            base.OnFormClosing(e);
        }

        /* Button click handler */
        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Tag.ToString())
            {
                case "CLEAR":
                    _cmbSearch.Items.Clear();
                    _cmbSearch.Text = string.Empty;
                    break;

                case "FIND":
                    if (!string.IsNullOrEmpty(_cmbSearch.Text))
                    {                        
                        /* Search and insert the new search term at the TOP of the list */
                        var inserted = false;
                        var search = _cmbSearch.Text;                        
                        foreach (var s in from object s in _cmbSearch.Items where s.ToString() == search select s)
                        {                            
                            /* Move it to top of list */
                            _cmbSearch.Items.Remove(s);
                            _cmbSearch.Items.Insert(0, s);
                            inserted = true;
                            break;
                        }
                        if (!inserted)
                        {
                            _cmbSearch.Items.Insert(0, search);
                        }
                        if (string.IsNullOrEmpty(_cmbSearch.Text))
                        {
                            _cmbSearch.Text = search;
                            _cmbSearch.SelectionStart = search.Length;
                        }
                        /* Adjust the size of the list */
                        if (_cmbSearch.Items.Count > SettingsManager.Settings.Windows.Caching.ChatSearch)
                        {
                            /* Remove last item */
                            _cmbSearch.Items.RemoveAt(_cmbSearch.Items.Count - 1);
                        }
                        FindWord(_cmbSearch.Text, _rbDown.Checked, _chkCase.Checked);
                    }
                    break;
            }
        }

        private void FindWord(string matchString, bool directionDown, bool matchCase)
        {
            if (string.IsNullOrEmpty(matchString) || _child.Output == null || _child.Output.TextData.Wrapped.Count == 0)
            {
                return;
            }
            int currentPointer;
            if (!directionDown)
            {
                currentPointer = _child.Output.ScrollTo - 1;
                if (currentPointer < 0)
                {
                    currentPointer = 0;
                    SystemSounds.Beep.Play();
                }
            }
            else
            {
                currentPointer = _child.Output.ScrollTo + 1;
                var count = _child.Output.TextData.WrappedLinesCount - 1;
                if (currentPointer > count)
                {
                    currentPointer = count;
                    SystemSounds.Beep.Play();
                }
            }
            var regXMatch = !matchCase
                                ? new Regex(Regex.Escape(matchString.Replace("*", null)), RegexOptions.IgnoreCase)
                                : new Regex(Regex.Escape(matchString.Replace("*", null)));
            int x;
            int y;
            var pointer = 0;            
            if (!directionDown)
            {                
                pointer = _child.Output.TextData.WrappedLinesCount - 1;
                for (x = _child.Output.TextData.Wrapped.Count - 1; x >= 0; x-- )
                {
                    for (y = _child.Output.TextData.Wrapped[x].Lines.Count - 1; y >= 0; y--)
                    {
                        if (pointer > currentPointer)
                        {
                            pointer--;
                            continue;                            
                        }
                        if (!regXMatch.IsMatch(_child.Output.TextData.Wrapped[x].Lines[y].Text))
                        {
                            pointer--;
                            continue;
                        }
                        _child.Output.ScrollTo = pointer;             
                        return;                        
                    }
                }
            }
            else
            {                
                for (x = 0; x <= _child.Output.TextData.Wrapped.Count - 1; x++)
                {
                    for (y = 0; y <= _child.Output.TextData.Wrapped[x].Lines.Count - 1; y++)
                    {
                        if (pointer < currentPointer)
                        {
                            pointer++;
                            continue;
                        }
                        if (!regXMatch.IsMatch(_child.Output.TextData.Wrapped[x].Lines[y].Text))
                        {
                            pointer++;
                            continue;
                        }
                        _child.Output.ScrollTo = pointer;                        
                        return;
                    }
                }
            }
            /* No match */
            SystemSounds.Beep.Play();
        }
    }
}
