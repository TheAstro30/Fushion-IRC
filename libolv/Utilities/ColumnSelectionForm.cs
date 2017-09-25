/* Object List View
 * Copyright (C) 2006-2012 Phillip Piper
 * Refactored by Jason James Newland - 2014
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip_piper@bigfoot.com.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace libolv.Utilities
{
    public partial class ColumnSelectionForm : Form
    {
        private List<OlvColumn> _allColumns;
        private List<OlvColumn> _rearrangableColumns = new List<OlvColumn>();
        private readonly Dictionary<OlvColumn, bool> _mapColumnToVisible = new Dictionary<OlvColumn, bool>();

        public ColumnSelectionForm()
        {
            InitializeComponent();
        }

        public void OpenOn(ObjectListView listView)
        {
            OpenOn(listView, listView.View);
        }

        public void OpenOn(ObjectListView listView, View view)
        {
            if (view != View.Details && view != View.Tile)
            {
                return;
            }
            InitializeForm(listView, view);
            if (ShowDialog() == DialogResult.OK)
            {
                Apply(listView, view);
            }
        }

        protected void InitializeForm(ObjectListView listView, View view)
        {
            _allColumns = listView.AllColumns;
            _rearrangableColumns = new List<OlvColumn>(_allColumns);
            foreach (var col in _rearrangableColumns)
            {
                if (view == View.Details)
                {
                    _mapColumnToVisible[col] = col.IsVisible;
                }
                else
                {
                    _mapColumnToVisible[col] = col.IsTileViewColumn;
                }
            }
            _rearrangableColumns.Sort(new SortByDisplayOrder(this));
            listView.BooleanCheckStateGetter = rowObject => _mapColumnToVisible[(OlvColumn)rowObject];
            listView.BooleanCheckStatePutter = delegate(Object rowObject, bool newValue)
                                              {
                                                  /* Some columns should always be shown, so ignore attempts to hide them */
                                                  var column = (OlvColumn)rowObject;
                                                  if (!column.CanBeHidden)
                                                  {
                                                      return true;
                                                  }
                                                  _mapColumnToVisible[column] = newValue;
                                                  EnableControls();
                                                  return newValue;
                                              };
            listView.SetObjects(_rearrangableColumns);
            EnableControls();
        }

        protected void Apply(ObjectListView listView, View view)
        {
            listView.Freeze();
            /* Update the column definitions to reflect whether they have been hidden */
            if (view == View.Details)
            {
                foreach (var col in listView.AllColumns)
                {
                    col.IsVisible = _mapColumnToVisible[col];
                }
            }
            else
            {
                foreach (var col in listView.AllColumns)
                {
                    col.IsTileViewColumn = _mapColumnToVisible[col];
                }
            }
            /* Collect the columns are still visible */
            var visibleColumns = _rearrangableColumns.FindAll(x => _mapColumnToVisible[x]);
            /* Detail view and Tile view have to be handled in different ways. */
            if (view == View.Details)
            {
                /* Of the still visible columns, change DisplayIndex to reflect their position in the rearranged list */
                listView.ChangeToFilteredColumns(view);
                foreach (ColumnHeader col in listView.Columns)
                {
                    col.DisplayIndex = visibleColumns.IndexOf((OlvColumn)col);
                }
            }
            else
            {
                /* In Tile view, DisplayOrder does nothing. So to change the display order, we have to change the 
                 * order of the columns in the Columns property.
                 * Remember, the primary column is special and has to remain first! */
                var primaryColumn = _allColumns[0];
                visibleColumns.Remove(primaryColumn);

                listView.Columns.Clear();
                listView.Columns.Add(primaryColumn);
                foreach (var c in visibleColumns)
                {
                    listView.Columns.Add(c);
                }
                listView.CalculateReasonableTileSize();
            }

            listView.Unfreeze();
        }

        /* Event handlers */
        private void ButtonMoveUpClick(object sender, EventArgs e)
        {
            var selectedIndex = olv.SelectedIndices[0];
            var col = _rearrangableColumns[selectedIndex];
            _rearrangableColumns.RemoveAt(selectedIndex);
            _rearrangableColumns.Insert(selectedIndex - 1, col);
            olv.BuildList();
            EnableControls();
        }

        private void ButtonMoveDownClick(object sender, EventArgs e)
        {
            var selectedIndex = olv.SelectedIndices[0];
            var col = _rearrangableColumns[selectedIndex];
            _rearrangableColumns.RemoveAt(selectedIndex);
            _rearrangableColumns.Insert(selectedIndex + 1, col);
            olv.BuildList();
            EnableControls();
        }

        private void ButtonShowClick(object sender, EventArgs e)
        {
            olv.SelectedItem.Checked = true;
        }

        private void ButtonHideClick(object sender, EventArgs e)
        {
            olv.SelectedItem.Checked = false;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ObjectListView1SelectionChanged(object sender, EventArgs e)
        {
            EnableControls();
        }

        /* Control enabling */
        protected void EnableControls()
        {
            if (olv.SelectedIndices.Count == 0)
            {
                buttonMoveUp.Enabled = false;
                buttonMoveDown.Enabled = false;
                buttonShow.Enabled = false;
                buttonHide.Enabled = false;
            }
            else
            {
                /* Can't move the first row up or the last row down */
                buttonMoveUp.Enabled = (olv.SelectedIndices[0] != 0);
                buttonMoveDown.Enabled = (olv.SelectedIndices[0] < (olv.GetItemCount() - 1));
                var selectedColumn = (OlvColumn)olv.SelectedObject;
                /* Some columns cannot be hidden (and hence cannot be Shown) */
                buttonShow.Enabled = !_mapColumnToVisible[selectedColumn] && selectedColumn.CanBeHidden;
                buttonHide.Enabled = _mapColumnToVisible[selectedColumn] && selectedColumn.CanBeHidden;
            }
        }

        private class SortByDisplayOrder : IComparer<OlvColumn>
        {
            public SortByDisplayOrder(ColumnSelectionForm form)
            {
                _form = form;
            }

            private readonly ColumnSelectionForm _form;

            int IComparer<OlvColumn>.Compare(OlvColumn x, OlvColumn y)
            {
                if (_form._mapColumnToVisible[x] && !_form._mapColumnToVisible[y])
                {
                    return -1;
                }
                if (!_form._mapColumnToVisible[x] && _form._mapColumnToVisible[y])
                {
                    return 1;
                }
                return x.DisplayIndex == y.DisplayIndex
                           ? String.Compare(x.Text, y.Text, StringComparison.Ordinal)
                           : x.DisplayIndex - y.DisplayIndex;
            }
        }
    }
}
