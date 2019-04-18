/* Syntax highlighter - by Uriel Guy
 * Original version 2005
 * This version 2019 - Jason James Newland
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;

namespace ircScript.Controls.SyntaxHightlight
{
    public sealed class AutoCompleteForm : Form
    {
        private readonly ColumnHeader _columnHeader;
        private readonly ListView _lstCompleteItems;
        private StringCollection _items = new StringCollection();

        public AutoCompleteForm()
        {
            _lstCompleteItems = new ListView
                                    {
                                        Dock = DockStyle.Fill,
                                        FullRowSelect = true,
                                        HeaderStyle = ColumnHeaderStyle.None,
                                        HideSelection = false,
                                        LabelWrap = false,
                                        Location = new Point(0, 0),
                                        MultiSelect = false,
                                        Name = "lstCompleteItems",
                                        Size = new Size(152, 136),
                                        Sorting = SortOrder.Ascending,
                                        TabIndex = 1,
                                        View = View.Details
                                    };

            _columnHeader = new ColumnHeader {Width = 148};
            _lstCompleteItems.Columns.AddRange(new[] { _columnHeader });

            AutoScaleBaseSize = new Size(5, 13);
            ClientSize = new Size(152, 136);
            ControlBox = false;
            Controls.Add(_lstCompleteItems);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MaximumSize = new Size(128, 176);
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = @"AutoCompleteForm";
            TopMost = true;
            VisibleChanged += AutoCompleteFormVisibleChanged;
            ResumeLayout(false);
        }

        public StringCollection Items
        {
            get { return _items; }
        }

        internal int ItemHeight
        {
            get { return 18; }
        }

        public string SelectedItem
        {
            get
            {
                return _lstCompleteItems.SelectedItems.Count == 0 ? null : _lstCompleteItems.SelectedItems[0].Text;
            }
        }

        internal int SelectedIndex
        {
            get { return _lstCompleteItems.SelectedIndices.Count == 0 ? -1 : _lstCompleteItems.SelectedIndices[0]; }
            set { _lstCompleteItems.Items[value].Selected = true; }
        }

        internal void UpdateView()
        {
            _lstCompleteItems.Items.Clear();
            foreach (string item in _items)
            {
                _lstCompleteItems.Items.Add(item);
            }
        }

        private void AutoCompleteFormVisibleChanged(object sender, EventArgs e)
        {
            var items = new ArrayList(_items);
            items.Sort(new CaseInsensitiveComparer());
            _items = new StringCollection();
            _items.AddRange((string[]) items.ToArray(typeof (string)));
            _columnHeader.Width = _lstCompleteItems.Width - 20;
        }
    }
}