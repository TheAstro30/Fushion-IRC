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
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ircScript.Controls.SyntaxHighlight.Forms.Hotkeys
{
    public partial class HotkeysEditorForm : Form
    {
        private readonly BindingList<HotkeyWrapper> _wrappers = new BindingList<HotkeyWrapper>();

        public HotkeysEditorForm(HotkeysMapping hotkeys)
        {
            InitializeComponent();
            BuildWrappers(hotkeys);
            dgv.DataSource = _wrappers;
        }

        private static int CompareKeys(Keys key1, Keys key2)
        {
            var res = ((int)key1 & 0xff).CompareTo((int)key2 & 0xff);
            if (res == 0)
                res = key1.CompareTo(key2);

            return res;
        }

        private void BuildWrappers(SortedDictionary<Keys, FctbAction> hotkeys)
        {
            var keys = new List<Keys>(hotkeys.Keys);
            keys.Sort(CompareKeys);
            _wrappers.Clear();
            foreach (var k in keys)
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

        private void btAdd_Click(object sender, EventArgs e)
        {
            _wrappers.Add(new HotkeyWrapper(Keys.None, FctbAction.None));
        }

        private void dgv_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            var cell = (dgv[0, e.RowIndex] as DataGridViewComboBoxCell);
            if (cell == null)
            {
                return;
            }
            if(cell.Items.Count == 0)
            {
                foreach(var item in new[]{"", "Ctrl", "Ctrl + Shift", "Ctrl + Alt", "Shift", "Shift + Alt", "Alt", "Ctrl + Shift + Alt"})
                cell.Items.Add(item);
            }
            cell = (dgv[1, e.RowIndex] as DataGridViewComboBoxCell);
            if (cell == null)
            {
                return;
            }
            if (cell.Items.Count == 0)
            {
                foreach (var item in Enum.GetValues(typeof (Keys)))
                {
                    cell.Items.Add(item);
                }
            }
            cell = (dgv[2, e.RowIndex] as DataGridViewComboBoxCell);
            if (cell == null || cell.Items.Count != 0)
            {
                return;
            }
            foreach (var item in Enum.GetValues(typeof (FctbAction)))
            {
                cell.Items.Add(item);
            }
        }

        private void btResore_Click(object sender, EventArgs e)
        {
            var h = new HotkeysMapping();
            h.InitDefault();
            BuildWrappers(h);
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            for (var i = dgv.RowCount - 1; i >= 0; i--)
            {
                if (dgv.Rows[i].Selected) dgv.Rows.RemoveAt(i);
            }
        }

        private void HotkeysEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                return;
            }
            var actions = GetUnAssignedActions();
            if (string.IsNullOrEmpty(actions))
            {
                return;
            }
            if (MessageBox.Show(@"Some actions are not assigned!\r\nActions: " + actions + @"\r\nPress Yes to save and exit, press No to continue editing", @"Some actions are not assigned", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private string GetUnAssignedActions()
        {
            var sb = new StringBuilder();
            var dic = new Dictionary<FctbAction, FctbAction>();
            foreach (var w in _wrappers)
            {
                dic[w.Action] = w.Action;
            }
            foreach (var item in from object item in Enum.GetValues(typeof(FctbAction)) where (FctbAction)item != FctbAction.None where !((FctbAction)item).ToString().StartsWith("CustomAction") where !dic.ContainsKey((FctbAction)item) select item)
            {
                sb.Append(item+", ");
            }
            return sb.ToString().TrimEnd(' ', ',');
        }
    }

    internal class HotkeyWrapper
    {
        private bool _ctrl;
        private bool _shift;
        private bool _alt;

        public Keys Key { get; set; }
        public FctbAction Action { get; set; }

        public HotkeyWrapper(Keys keyData, FctbAction action)
        {
            var a = new KeyEventArgs(keyData);
            _ctrl = a.Control;
            _shift = a.Shift;
            _alt = a.Alt;
            Key = a.KeyCode;
            Action = action;
        }

        public Keys ToKeyData()
        {
            var res = Key;
            if (_ctrl) res |= Keys.Control;
            if (_alt) res |= Keys.Alt;
            if (_shift) res |= Keys.Shift;
            return res;
        }
        
        public string Modifiers
        {
            get
            {
                var res = "";
                if (_ctrl) res += "Ctrl + ";
                if (_shift) res += "Shift + ";
                if (_alt) res += "Alt + ";
                return res.Trim(' ', '+');
            }
            set
            {
                if (value == null)
                {
                    _ctrl = _alt = _shift = false;
                }
                else
                {
                    _ctrl = value.Contains("Ctrl");
                    _shift = value.Contains("Shift");
                    _alt = value.Contains("Alt");
                }
            }
        }        
    }
}
