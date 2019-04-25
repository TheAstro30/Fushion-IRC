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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ircScript.Controls.SyntaxHighlight.Forms.Hotkeys
{
    public sealed class HotkeysEditorForm : Form
    {
        private readonly BindingList<HotkeyWrapper> _wrappers = new BindingList<HotkeyWrapper>();
                         
        private readonly Label _lblInfo;
        private readonly DataGridView _dgv;
        private readonly DataGridViewComboBoxColumn _cbModifiers;
        private readonly DataGridViewComboBoxColumn _cbKey;
        private readonly DataGridViewComboBoxColumn _cbAction;
        private readonly Button _btAdd;
        private readonly Button _btRemove;
        private readonly Button _btResore;
        private readonly Button _btOk;
        private readonly Button _btCancel;  

        public HotkeysEditorForm(SortedDictionary<Keys, FctbAction> hotkeys)
        {
            ClientSize = new Size(549, 357);
            MaximumSize = new Size(565, 700);
            MinimumSize = new Size(565, 395);
            ShowIcon = false;
            Text = @"Hotkeys Editor";

            var dataGridViewCellStyle1 = new DataGridViewCellStyle();

            _lblInfo = new Label
                           {
                               AutoSize = true,
                               Font =
                                   new Font("Microsoft Sans Serif", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 204),
                               Location = new Point(12, 9),
                               Size = new Size(114, 16),
                               TabIndex = 5,
                               Text = @"Hotkeys mapping"
                           };

            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Window;
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
            dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = Color.LightSteelBlue;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;

            _dgv = new DataGridView
                       {
                           AllowUserToAddRows = false,
                           AllowUserToDeleteRows = false,
                           AllowUserToResizeColumns = false,
                           AllowUserToResizeRows = false,
                           Anchor = (((((AnchorStyles.Top | AnchorStyles.Bottom)
                                        | AnchorStyles.Left)
                                       | AnchorStyles.Right))),
                           BackgroundColor = SystemColors.Control,
                           ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                           DefaultCellStyle = dataGridViewCellStyle1,
                           GridColor = SystemColors.Control,
                           Location = new Point(12, 28),
                           RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                           RowHeadersVisible = false,
                           RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing,
                           SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                           Size = new Size(525, 278),
                           TabIndex = 0
                       };

            _cbModifiers = new DataGridViewComboBoxColumn
                               {
                                   DataPropertyName = "Modifiers",
                                   DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                                   FlatStyle = FlatStyle.Flat,
                                   HeaderText = @"Modifiers",
                               };

            _cbKey = new DataGridViewComboBoxColumn
                         {
                             DataPropertyName = "Key",
                             DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                             FlatStyle = FlatStyle.Flat,
                             HeaderText = @"Key",
                             Resizable = DataGridViewTriState.True,
                             Width = 120
                         };

            _cbAction = new DataGridViewComboBoxColumn
                            {
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                                DataPropertyName = "Action",
                                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
                                FlatStyle = FlatStyle.Flat,
                                HeaderText = @"Action",
                            };

            _dgv.Columns.AddRange(new DataGridViewColumn[]
                                     {
                                         _cbModifiers,
                                         _cbKey,
                                         _cbAction
                                     });

            _btAdd = new Button
                         {
                             Anchor = (((AnchorStyles.Bottom | AnchorStyles.Left))),
                             Location = new Point(13, 322),
                             Size = new Size(75, 23),
                             TabIndex = 1,
                             Text = @"Add",
                             UseVisualStyleBackColor = true
                         };

            _btRemove = new Button
                            {
                                Anchor = (((AnchorStyles.Bottom | AnchorStyles.Left))),
                                Location = new Point(103, 322),
                                Size = new Size(75, 23),
                                TabIndex = 2,
                                Text = @"Remove",
                                UseVisualStyleBackColor = true
                            };

            _btResore = new Button
                            {
                                Anchor = (((AnchorStyles.Bottom | AnchorStyles.Left))),
                                Location = new Point(194, 322),
                                Size = new Size(105, 23),
                                TabIndex = 6,
                                Text = @"Restore Default",
                                UseVisualStyleBackColor = true
                            };

            _btOk = new Button
                        {
                            Anchor = (((AnchorStyles.Bottom | AnchorStyles.Right))),
                            DialogResult = DialogResult.OK,
                            Location = new Point(379, 322),
                            Size = new Size(75, 23),
                            TabIndex = 3,
                            Text = @"OK",
                            UseVisualStyleBackColor = true
                        };

            _btCancel = new Button
                            {
                                Anchor = (((AnchorStyles.Bottom | AnchorStyles.Right))),
                                DialogResult = DialogResult.Cancel,
                                Location = new Point(460, 322),
                                Size = new Size(75, 23),
                                TabIndex = 4,
                                Text = @"Cancel",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_lblInfo, _dgv, _btAdd, _btRemove, _btResore, _btOk, _btCancel});

            CancelButton = _btCancel;

            _dgv.RowsAdded += DataGridViewRowsAdded;

            BuildWrappers(hotkeys);
            _dgv.DataSource = _wrappers;

            _btAdd.Click += ButtonClickHandler;
            _btRemove.Click += ButtonClickHandler;
            _btResore.Click += ButtonClickHandler;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                return;
            }
            string actions = GetUnAssignedActions();
            if (string.IsNullOrEmpty(actions))
            {
                return;
            }
            if (
                MessageBox.Show(
                    @"Some actions are not assigned!\r\nActions: " + actions +
                    @"\r\nPress Yes to save and exit, press No to continue editing", @"Some actions are not assigned",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

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
                case "ADD":
                    _wrappers.Add(new HotkeyWrapper(Keys.None, FctbAction.None));
                    break;

                case "REMOVE":
                    for (int i = _dgv.RowCount - 1; i >= 0; i--)
                    {
                        if (_dgv.Rows[i].Selected) _dgv.Rows.RemoveAt(i);
                    }
                    break;

                case "RESTORE DEFAULT":
                    var h = new HotkeysMapping();
                    h.InitDefault();
                    BuildWrappers(h);
                    break;
            }
        }

        private void DataGridViewRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            var cell = (_dgv[0, e.RowIndex] as DataGridViewComboBoxCell);
            if (cell == null)
            {
                return;
            }
            if (cell.Items.Count == 0)
            {
                foreach (
                    string item in
                        new[]
                            {
                                "", "Ctrl", "Ctrl + Shift", "Ctrl + Alt", "Shift", "Shift + Alt", "Alt",
                                "Ctrl + Shift + Alt"
                            })
                {
                    cell.Items.Add(item);
                }
            }
            cell = (_dgv[1, e.RowIndex] as DataGridViewComboBoxCell);
            if (cell == null)
            {
                return;
            }
            if (cell.Items.Count == 0)
            {
                foreach (object item in Enum.GetValues(typeof (Keys)))
                {
                    cell.Items.Add(item);
                }
            }
            cell = (_dgv[2, e.RowIndex] as DataGridViewComboBoxCell);
            if (cell == null || cell.Items.Count != 0)
            {
                return;
            }
            foreach (object item in Enum.GetValues(typeof (FctbAction)))
            {
                cell.Items.Add(item);
            }
        }

        /* Methods */
        private static int CompareKeys(Keys key1, Keys key2)
        {
            var res = ((int) key1 & 0xff).CompareTo((int) key2 & 0xff);
            if (res == 0)
            {
                res = key1.CompareTo(key2);
            }
            return res;
        }

        private void BuildWrappers(SortedDictionary<Keys, FctbAction> hotkeys)
        {
            var keys = new List<Keys>(hotkeys.Keys);
            keys.Sort(CompareKeys);
            _wrappers.Clear();
            foreach (Keys k in keys)
            {
                _wrappers.Add(new HotkeyWrapper(k, hotkeys[k]));
            }
        }

        public HotkeysMapping GetHotkeys()
        {
            var result = new HotkeysMapping();
            foreach (var w in _wrappers)
            {
                result[w.ToKeyData()] = w.Action;
            }
            return result;
        }

        private string GetUnAssignedActions()
        {
            var sb = new StringBuilder();
            var dic = new Dictionary<FctbAction, FctbAction>();
            foreach (var w in _wrappers)
            {
                dic[w.Action] = w.Action;
            }
            foreach (var item in from object item in Enum.GetValues(typeof(FctbAction))
                                 where (FctbAction)item != FctbAction.None
                                 where !((FctbAction)item).ToString().StartsWith("CustomAction")
                                 where !dic.ContainsKey((FctbAction)item)
                                 select item)
            {
                sb.Append(item + ", ");
            }
            return sb.ToString().TrimEnd(' ', ',');
        }
    }
}