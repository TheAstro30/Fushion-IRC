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
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using libolv.Implementation;
using libolv.Implementation.Events;
using libolv.Implementation.Munger;

namespace libolv
{
    public partial class ObjectListView
    {
        internal CellEditEventArgs CellEditEventArgs;

        private int _lastMouseDownClickCount;

        protected virtual bool ShouldStartCellEdit(MouseEventArgs e)
        {
            if (IsCellEditing)
            {
                return false;
            }
            if (e.Button != MouseButtons.Left)
            {
                return false;
            }
            if ((ModifierKeys & (Keys.Shift | Keys.Control | Keys.Alt)) != 0)
            {
                return false;
            }
            if (_lastMouseDownClickCount == 1 && CellEditActivation == CellEditActivateMode.SingleClick)
            {
                return true;
            }
            return (_lastMouseDownClickCount == 2 && CellEditActivation == CellEditActivateMode.DoubleClick);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (IsCellEditing)
            {
                return CellEditKeyEngine.HandleKey(this, keyData);
            }
            /* Treat F2 as a request to edit the primary column */
            if (keyData == Keys.F2)
            {
                EditSubItem((OlvListItem)FocusedItem, 0);
                return base.ProcessDialogKey(keyData);
            }
            /* Treat Ctrl-C as Copy To Clipboard. */
            if (CopySelectionOnControlC && keyData == (Keys.C | Keys.Control))
            {
                CopySelectionToClipboard();
                return true;
            }
            /* Treat Ctrl-A as Select All. */
            if (SelectAllOnControlA && keyData == (Keys.A | Keys.Control))
            {
                SelectAll();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        public virtual void EditModel(object rowModel)
        {
            var olvItem = ModelToItem(rowModel);
            if (olvItem == null)
            {
                return;
            }
            for (var i = 0; i < olvItem.SubItems.Count; i++)
            {
                if (!GetColumn(i).IsEditable) { continue; }
                StartCellEdit(olvItem, i);
                return;
            }
        }

        public virtual void EditSubItem(OlvListItem item, int subItemIndex)
        {
            if (item == null)
            {
                return;
            }
            if (subItemIndex < 0 && subItemIndex >= item.SubItems.Count)
            {
                return;
            }
            if (CellEditActivation == CellEditActivateMode.None)
            {
                return;
            }
            if (!GetColumn(subItemIndex).IsEditable)
            {
                return;
            }
            StartCellEdit(item, subItemIndex);
        }

        public virtual void StartCellEdit(OlvListItem item, int subItemIndex)
        {
            var column = GetColumn(subItemIndex);
            var c = GetCellEditor(item, subItemIndex);
            var r = CalculateCellEditorBounds(item, subItemIndex, c.PreferredSize);
            c.Bounds = r;
            /* Try to align the control as the column is aligned. Not all controls support this property */
            Munger.PutProperty(c, "TextAlign", column.TextAlign);
            /* Give the control the value from the model */
            SetControlValue(c, column.GetValue(item.RowObject), column.GetStringValue(item.RowObject));
            /* Give the outside world the chance to munge with the process */
            CellEditEventArgs = new CellEditEventArgs(column, c, r, item, subItemIndex);
            OnCellEditStarting(CellEditEventArgs);
            if (CellEditEventArgs.Cancel)
            {
                return;
            }
            /* The event handler may have completely changed the control, so we need to remember it */
            CellEditor = CellEditEventArgs.Control;
            /* If the control isn't the height of the cell, centre it vertically. 
             * We don't do this in OwnerDrawn mode since the renderer already aligns the control correctly.
             * We also dont need to do this when in Tile view. */
            if (View != View.Tile && !OwnerDraw && CellEditor.Height != r.Height)
            {
                CellEditor.Top += (r.Height - CellEditor.Height) / 2;
            }
            Invalidate();
            Controls.Add(CellEditor);
            ConfigureControl();
            PauseAnimations(true);
        }
        
        public Rectangle CalculateCellEditorBounds(OlvListItem item, int subItemIndex, Size preferredSize)
        {
            var r = View == View.Details ? item.GetSubItemBounds(subItemIndex) : GetItemRect(item.Index, ItemBoundsPortion.Label);
            return OwnerDraw ? CalculateCellEditorBoundsOwnerDrawn(item, subItemIndex, r, preferredSize) : CalculateCellEditorBoundsStandard(item, subItemIndex, r, preferredSize);
        }

        protected Rectangle CalculateCellEditorBoundsOwnerDrawn(OlvListItem item, int subItemIndex, Rectangle r, Size preferredSize)
        {
            var renderer = View == View.Details
                               ? (GetColumn(subItemIndex).Renderer ?? DefaultRenderer)
                               : ItemRenderer;
            if (renderer == null)
            {
                return r;
            }
            using (var g = CreateGraphics())
            {
                return renderer.GetEditRectangle(g, r, item, subItemIndex, preferredSize);
            }
        }

        protected Rectangle CalculateCellEditorBoundsStandard(OlvListItem item, int subItemIndex, Rectangle cellBounds, Size preferredSize)
        {
            if (View != View.Details)
            {
                return cellBounds;
            }
            /* Allow for image (if there is one). */            
            var offset = 0;
            object imageSelector = null;
            if (subItemIndex == 0)
            {
                imageSelector = item.ImageSelector;
            }
            else
            {
                /* We only check for subitem images if we are owner drawn or showing subitem images */
                if (OwnerDraw || ShowImagesOnSubItems)
                {
                    imageSelector = item.GetSubItem(subItemIndex).ImageSelector;
                }
            }
            if (GetActualImageIndex(imageSelector) != -1)
            {
                offset += SmallImageSize.Width + 2;
            }
            /* Allow for checkbox */
            if (CheckBoxes && StateImageList != null && subItemIndex == 0)
            {
                offset += StateImageList.ImageSize.Width + 2;
            }
            /* Allow for indent (first column only) */
            if (subItemIndex == 0 && item.IndentCount > 0)
            {
                offset += (SmallImageSize.Width * item.IndentCount);
            }
            /* Do the adjustment */
            if (offset > 0)
            {
                cellBounds.X += offset;
                cellBounds.Width -= offset;
            }
            return cellBounds;
        }

        protected virtual void SetControlValue(Control control, Object value, String stringValue)
        {
            /* Handle combobox explicitly */
            var comboBox = control as ComboBox;
            if (comboBox != null)
            {
                var cb = comboBox;
                if (cb.Created)
                {
                    cb.SelectedValue = value;
                }
                else
                {
                    BeginInvoke(new MethodInvoker(delegate
                                                      {
                                                          cb.SelectedValue = value;
                                                      }));
                }
                return;
            }
            if (Munger.PutProperty(control, "Value", value))
            {
                return;
            }
            /* There wasn't a Value property, or we couldn't set it, so set the text instead */
            try
            {
                var valueAsString = value as String;
                control.Text = valueAsString ?? stringValue;
            }
            catch (ArgumentOutOfRangeException)
            {
                /* The value couldn't be set via the Text property. */
            }
        }

        protected virtual void ConfigureControl()
        {
            CellEditor.Validating += CellEditorValidating;
            CellEditor.Select();
        }

        protected virtual Object GetControlValue(Control control)
        {
            if (control == null)
            {
                return null;
            }
            var textBox = control as TextBox;
            if (textBox != null)
            {
                return textBox.Text;
            }
            var comboBox = control as ComboBox;
            if (comboBox != null)
            {
                return comboBox.SelectedValue;
            }
            var checkBox = control as CheckBox;
            if (checkBox != null)
            {
                return checkBox.Checked;
            }
            try
            {
                return control.GetType().InvokeMember("Value", BindingFlags.GetProperty, null, control, null);
            }
            catch (MissingMethodException)
            {
                /* Microsoft throws this */
                return control.Text;
            }
            catch (MissingFieldException)
            {
                /* Mono throws this */
                return control.Text;
            }
        }

        protected virtual void CellEditorValidating(object sender, CancelEventArgs e)
        {
            CellEditEventArgs.Cancel = false;
            CellEditEventArgs.NewValue = GetControlValue(CellEditor);
            OnCellEditorValidating(CellEditEventArgs);
            if (CellEditEventArgs.Cancel)
            {
                CellEditEventArgs.Control.Select();
                e.Cancel = true;
            }
            else
            {
                FinishCellEdit();
            }
        }

        public virtual Rectangle CalculateCellBounds(OlvListItem item, int subItemIndex)
        {
            /* TODO: Check if this is the same thing as OLVListItem.GetSubItemBounds() ?
             * We use ItemBoundsPortion.Label rather than ItemBoundsPortion.Item
             * since Label extends to the right edge of the cell, whereas Item gives just the
             * current text width. */
            return CalculateCellBounds(item, subItemIndex, ItemBoundsPortion.Label);
        }

        public virtual Rectangle CalculateCellTextBounds(OlvListItem item, int subItemIndex)
        {
            return CalculateCellBounds(item, subItemIndex, ItemBoundsPortion.ItemOnly);
        }

        private Rectangle CalculateCellBounds(OlvListItem item, int subItemIndex, ItemBoundsPortion portion)
        {
            /* SubItem.Bounds works for every subitem, except the first. */
            if (subItemIndex > 0)
            {
                return item.SubItems[subItemIndex].Bounds;
            }
            /* For non detail views, we just use the requested portion */
            var r = GetItemRect(item.Index, portion);
            if (r.Y < -10000000 || r.Y > 10000000)
            {
                r.Y = item.Bounds.Y;
            }
            if (View != View.Details)
            {
                return r;
            }
            /* Finding the bounds of cell 0 should not be a difficult task, but it is. Problems:
             * 1) item.SubItem[0].Bounds is always the full bounds of the entire row, not just cell 0.
             * 2) if column 0 has been dragged to some other position, the bounds always has a left edge of 0.
             * We avoid both these problems by using the position of sides the column header to calculate
             * the sides of the cell */
            var sides = NativeMethods.GetScrolledColumnSides(this, 0);
            r.X = sides.X + 4;
            r.Width = sides.Y - sides.X - 5;
            return r;
        }

        protected virtual Control GetCellEditor(OlvListItem item, int subItemIndex)
        {
            var column = GetColumn(subItemIndex);
            var value = column.GetValue(item.RowObject) ?? GetFirstNonNullValue(column);
            /* TODO: What do we do if value is still null here?
             * Ask the registry for an instance of the appropriate editor. */
            var editor = EditorRegistry.GetEditor(item.RowObject, column, value) ?? MakeDefaultCellEditor(column);
            return editor;
        }

        internal object GetFirstNonNullValue(OlvColumn column)
        {
            for (var i = 0; i < Math.Min(GetItemCount(), 1000); i++)
            {
                var value = column.GetValue(GetModelObject(i));
                if (value != null)
                {
                    return value;
                }
            }
            return null;
        }

        protected virtual Control MakeDefaultCellEditor(OlvColumn column)
        {
            var tb = new TextBox();
            if (column.AutoCompleteEditor)
            {
                ConfigureAutoComplete(tb, column);
            }
            return tb;
        }

        public void ConfigureAutoComplete(TextBox tb, OlvColumn column)
        {
            ConfigureAutoComplete(tb, column, 1000);
        }

        public void ConfigureAutoComplete(TextBox tb, OlvColumn column, int maxRows)
        {
            /* Don't consider more rows than we actually have */
            maxRows = Math.Min(GetItemCount(), maxRows);
            /* Reset any existing autocomplete */
            tb.AutoCompleteCustomSource.Clear();
            /* CONSIDER: Should we use ClusteringStrategy here?
             * Build a list of unique values, to be used as autocomplete on the editor */
            var alreadySeen = new Dictionary<string, bool>();
            var values = new List<string>();
            for (var i = 0; i < maxRows; i++)
            {
                var valueAsString = column.GetStringValue(GetModelObject(i));
                if (string.IsNullOrEmpty(valueAsString) || alreadySeen.ContainsKey(valueAsString)) { continue; }
                values.Add(valueAsString);
                alreadySeen[valueAsString] = true;
            }
            tb.AutoCompleteCustomSource.AddRange(values.ToArray());
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            tb.AutoCompleteMode = column.AutoCompleteEditorMode;
        }

        public virtual void CancelCellEdit()
        {
            if (!IsCellEditing)
            {
                return;
            }
            /* Let the world know that the user has cancelled the edit operation */
            CellEditEventArgs.Cancel = true;
            CellEditEventArgs.NewValue = GetControlValue(CellEditor);
            OnCellEditFinishing(CellEditEventArgs);
            /* Now cleanup the editing process */
            CleanupCellEdit(false);
        }

        public virtual bool PossibleFinishCellEditing()
        {
            return PossibleFinishCellEditing(false);
        }

        public virtual bool PossibleFinishCellEditing(bool expectingCellEdit)
        {
            if (!IsCellEditing)
            {
                return true;
            }
            CellEditEventArgs.Cancel = false;
            CellEditEventArgs.NewValue = GetControlValue(CellEditor);
            OnCellEditorValidating(CellEditEventArgs);
            if (CellEditEventArgs.Cancel)
            {
                return false;
            }
            FinishCellEdit(expectingCellEdit);
            return true;
        }

        public virtual void FinishCellEdit()
        {
            FinishCellEdit(false);
        }

        public virtual void FinishCellEdit(bool expectingCellEdit)
        {
            if (!IsCellEditing)
            {
                return;
            }
            CellEditEventArgs.Cancel = false;
            CellEditEventArgs.NewValue = GetControlValue(CellEditor);
            OnCellEditFinishing(CellEditEventArgs);
            /* If someone doesn't cancel the editing process, write the value back into the model */
            if (!CellEditEventArgs.Cancel)
            {
                CellEditEventArgs.Column.PutValue(CellEditEventArgs.RowObject, CellEditEventArgs.NewValue);
                RefreshItem(CellEditEventArgs.ListViewItem);
            }
            CleanupCellEdit(expectingCellEdit);
        }

        protected virtual void CleanupCellEdit(bool expectingCellEdit)
        {
            if (CellEditor == null)
            {
                return;
            }
            CellEditor.Validating -= CellEditorValidating;
            var soonToBeOldCellEditor = CellEditor;
            CellEditor = null;
            /* Delay cleaning up the cell editor so that if we are immediately going to 
             * start a new cell edit (because the user pressed Tab) the new cell editor
             * has a chance to grab the focus. Without this, the ListView gains focus
             * momentarily (after the cell editor is remove and before the new one is created)
             * causing the list's selection to flash momentarily.*/
            EventHandler toBeRun = null;
            toBeRun = delegate
                          {
                              Application.Idle -= toBeRun;
                              Controls.Remove(soonToBeOldCellEditor);
                              Invalidate();
                              if (IsCellEditing) { return; }
                              Select();
                              PauseAnimations(false);
                          };
            /* We only want to delay the removal of the control if we are expecting another cell
             * to be edited. Otherwise, we remove the control immediately. */
            if (expectingCellEdit)
            {
                RunWhenIdle(toBeRun);
            }
            else
            {
                toBeRun(null, null);
            }
        }
    }
}
