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
using System.Collections;
using System.Windows.Forms;
using libolv.Implementation.Events;

namespace libolv
{
    public partial class ObjectListView
    {
        public virtual void CheckIndeterminateObject(object modelObject)
        {
            SetObjectCheckedness(modelObject, CheckState.Indeterminate);
        }

        public virtual void CheckObject(object modelObject)
        {
            SetObjectCheckedness(modelObject, CheckState.Checked);
        }

        public virtual void CheckObjects(IEnumerable modelObjects)
        {
            foreach (var model in modelObjects)
            {
                CheckObject(model);
            }
        }

        public virtual void CheckSubItem(object rowObject, OlvColumn column)
        {
            if (column == null || rowObject == null || !column.CheckBoxes)
            {
                return;
            }
            column.PutCheckState(rowObject, CheckState.Checked);
            RefreshObject(rowObject);
        }

        public virtual void CheckIndeterminateSubItem(object rowObject, OlvColumn column)
        {
            if (column == null || rowObject == null || !column.CheckBoxes)
            {
                return;
            }
            column.PutCheckState(rowObject, CheckState.Indeterminate);
            RefreshObject(rowObject);
        }

        public virtual bool IsChecked(object modelObject)
        {
            var olvi = ModelToItem(modelObject);
            return olvi != null && olvi.CheckState == CheckState.Checked;
        }

        public virtual bool IsCheckedIndeterminate(object modelObject)
        {
            var olvi = ModelToItem(modelObject);
            return olvi != null && olvi.CheckState == CheckState.Indeterminate;
        }

        public virtual bool IsSubItemChecked(object rowObject, OlvColumn column)
        {
            if (column == null || rowObject == null || !column.CheckBoxes)
            {
                return false;
            }
            return (column.GetCheckState(rowObject) == CheckState.Checked);
        }

        protected virtual CheckState? GetCheckState(Object modelObject)
        {
            if (CheckStateGetter != null)
            {
                return CheckStateGetter(modelObject);
            }
            return PersistentCheckBoxes ? GetPersistentCheckState(modelObject) : (CheckState?)null;
        }

        protected virtual CheckState PutCheckState(Object modelObject, CheckState state)
        {
            if (CheckStatePutter != null)
            {
                return CheckStatePutter(modelObject, state);
            }
            return PersistentCheckBoxes ? SetPersistentCheckState(modelObject, state) : state;
        }

        protected virtual void SetObjectCheckedness(object modelObject, CheckState state)
        {
            var olvi = ModelToItem(modelObject);
            /* If we didn't find the given, we still try to record the check state. */
            if (olvi == null)
            {
                PutCheckState(modelObject, state);
                return;
            }
            if (olvi.CheckState == state)
            {
                return;
            }
            /* Trigger checkbox changing event. We only need to do this for virtual
             * lists, since setting CheckState triggers these events for non-virtual lists */
            var ice = new ItemCheckEventArgs(olvi.Index, state, olvi.CheckState);
            OnItemCheck(ice);
            if (ice.NewValue == olvi.CheckState)
            {
                return;
            }
            olvi.CheckState = PutCheckState(modelObject, state);
            RefreshItem(olvi);
            /* Trigger check changed event */
            OnItemChecked(new ItemCheckedEventArgs(olvi));
        }

        public virtual void ToggleCheckObject(object modelObject)
        {
            var olvi = ModelToItem(modelObject);
            if (olvi == null)
            {
                return;
            }
            var newState = CheckState.Checked;
            if (olvi.CheckState == CheckState.Checked)
            {
                newState = TriStateCheckBoxes ? CheckState.Indeterminate : CheckState.Unchecked;
            }
            else
            {
                if (olvi.CheckState == CheckState.Indeterminate && TriStateCheckBoxes)
                    newState = CheckState.Unchecked;
            }
            SetObjectCheckedness(modelObject, newState);
        }

        public virtual void ToggleSubItemCheckBox(object rowObject, OlvColumn column)
        {
            var currentState = column.GetCheckState(rowObject);
            var newState = CalculateToggledCheckState(column, currentState);
            var args = new SubItemCheckingEventArgs(column, ModelToItem(rowObject), column.Index, currentState, newState);
            OnSubItemChecking(args);
            if (args.Canceled)
            {
                return;
            }
            switch (args.NewValue)
            {
                case CheckState.Checked:
                    CheckSubItem(rowObject, column);
                    break;

                case CheckState.Indeterminate:
                    CheckIndeterminateSubItem(rowObject, column);
                    break;

                case CheckState.Unchecked:
                    UncheckSubItem(rowObject, column);
                    break;
            }
        }

        private static CheckState CalculateToggledCheckState(OlvColumn column, CheckState currentState)
        {
            switch (currentState)
            {
                case CheckState.Checked:
                    return column.TriStateCheckBoxes ? CheckState.Indeterminate : CheckState.Unchecked;

                case CheckState.Indeterminate:
                    return CheckState.Unchecked;

                default:
                    return CheckState.Checked;
            }
        }

        public virtual void UncheckObject(object modelObject)
        {
            SetObjectCheckedness(modelObject, CheckState.Unchecked);
        }

        public virtual void UncheckObjects(IEnumerable modelObjects)
        {
            foreach (var model in modelObjects)
            {
                UncheckObject(model);
            }
        }

        public virtual void UncheckSubItem(object rowObject, OlvColumn column)
        {
            if (column == null || rowObject == null || !column.CheckBoxes)
            {
                return;
            }
            column.PutCheckState(rowObject, CheckState.Unchecked);
            RefreshObject(rowObject);
        }
    }
}
