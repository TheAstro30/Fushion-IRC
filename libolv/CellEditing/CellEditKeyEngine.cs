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
using System.Linq;
using System.Windows.Forms;
using libolv.Implementation;

namespace libolv.CellEditing
{
    public enum CellEditAtEdgeBehaviour
    {
        Ignore = 0,
        Wrap = 1,
        ChangeColumn = 2,
        ChangeRow = 3,
        EndEdit = 4
    }

    public enum CellEditCharacterBehaviour
    {
        Ignore = 0,
        ChangeColumnLeft = 1,
        ChangeColumnRight = 2,
        ChangeRowUp = 3,
        ChangeRowDown = 4,
        CancelEdit = 5,
        EndEdit = 6,
        CustomVerb1 = 7,
        CustomVerb2 = 8,
        CustomVerb3 = 9,
        CustomVerb4 = 10,
        CustomVerb5 = 11,
        CustomVerb6 = 12,
        CustomVerb7 = 13,
        CustomVerb8 = 14,
        CustomVerb9 = 15,
        CustomVerb10 = 16
    }

    public class CellEditKeyEngine
    {
        private IDictionary<Keys, CellEditCharacterBehaviour> _cellEditKeyMap;
        private IDictionary<Keys, CellEditAtEdgeBehaviour> _cellEditKeyAtEdgeBehaviourMap;

        /* Public interface */
        public virtual void SetKeyBehaviour(Keys key, CellEditCharacterBehaviour normalBehaviour, CellEditAtEdgeBehaviour atEdgeBehaviour)
        {
            CellEditKeyMap[key] = normalBehaviour;
            CellEditKeyAtEdgeBehaviourMap[key] = atEdgeBehaviour;
        }

        public virtual bool HandleKey(ObjectListView olv, Keys keyData)
        {
            if (olv == null) { throw new ArgumentNullException("olv"); }
            CellEditCharacterBehaviour behaviour;
            if (!CellEditKeyMap.TryGetValue(keyData, out behaviour))
            {
                return false;
            }
            ListView = olv;
            switch (behaviour)
            {
                case CellEditCharacterBehaviour.Ignore:
                    break;

                case CellEditCharacterBehaviour.CancelEdit:
                    HandleCancelEdit();
                    break;

                case CellEditCharacterBehaviour.EndEdit:
                    HandleEndEdit();
                    break;

                case CellEditCharacterBehaviour.ChangeColumnLeft:
                case CellEditCharacterBehaviour.ChangeColumnRight:
                    HandleColumnChange(keyData, behaviour);
                    break;

                case CellEditCharacterBehaviour.ChangeRowDown:
                case CellEditCharacterBehaviour.ChangeRowUp:
                    HandleRowChange(keyData, behaviour);
                    break;

                default:
                    return HandleCustomVerb(keyData, behaviour);
            }
            return true;
        }

        /* Implementation properties */
        protected ObjectListView ListView { get; set; }

        protected OlvListItem ItemBeingEdited
        {
            get { return ListView.CellEditEventArgs.ListViewItem; }
        }

        protected int SubItemIndexBeingEdited
        {
            get { return ListView.CellEditEventArgs.SubItemIndex; }
        }

        protected IDictionary<Keys, CellEditCharacterBehaviour> CellEditKeyMap
        {
            get
            {
                if (_cellEditKeyMap == null)
                {
                    InitializeCellEditKeyMaps();
                }
                return _cellEditKeyMap;
            }
            set { _cellEditKeyMap = value; }
        }
        
        protected IDictionary<Keys, CellEditAtEdgeBehaviour> CellEditKeyAtEdgeBehaviourMap
        {
            get
            {
                if (_cellEditKeyAtEdgeBehaviourMap == null)
                {
                    InitializeCellEditKeyMaps();
                }
                return _cellEditKeyAtEdgeBehaviourMap;
            }
            set { _cellEditKeyAtEdgeBehaviourMap = value; }
        }

        /* Initialization */
        protected virtual void InitializeCellEditKeyMaps()
        {
            _cellEditKeyMap = new Dictionary<Keys, CellEditCharacterBehaviour>();
            _cellEditKeyMap[Keys.Escape] = CellEditCharacterBehaviour.CancelEdit;
            _cellEditKeyMap[Keys.Return] = CellEditCharacterBehaviour.EndEdit;
            _cellEditKeyMap[Keys.Enter] = CellEditCharacterBehaviour.EndEdit;
            _cellEditKeyMap[Keys.Tab] = CellEditCharacterBehaviour.ChangeColumnRight;
            _cellEditKeyMap[Keys.Tab | Keys.Shift] = CellEditCharacterBehaviour.ChangeColumnLeft;
            _cellEditKeyMap[Keys.Left | Keys.Alt] = CellEditCharacterBehaviour.ChangeColumnLeft;
            _cellEditKeyMap[Keys.Right | Keys.Alt] = CellEditCharacterBehaviour.ChangeColumnRight;
            _cellEditKeyMap[Keys.Up | Keys.Alt] = CellEditCharacterBehaviour.ChangeRowUp;
            _cellEditKeyMap[Keys.Down | Keys.Alt] = CellEditCharacterBehaviour.ChangeRowDown;
            _cellEditKeyAtEdgeBehaviourMap = new Dictionary<Keys, CellEditAtEdgeBehaviour>();
            _cellEditKeyAtEdgeBehaviourMap[Keys.Tab] = CellEditAtEdgeBehaviour.Wrap;
            _cellEditKeyAtEdgeBehaviourMap[Keys.Tab | Keys.Shift] = CellEditAtEdgeBehaviour.Wrap;
            _cellEditKeyAtEdgeBehaviourMap[Keys.Left | Keys.Alt] = CellEditAtEdgeBehaviour.Wrap;
            _cellEditKeyAtEdgeBehaviourMap[Keys.Right | Keys.Alt] = CellEditAtEdgeBehaviour.Wrap;
            _cellEditKeyAtEdgeBehaviourMap[Keys.Up | Keys.Alt] = CellEditAtEdgeBehaviour.ChangeColumn;
            _cellEditKeyAtEdgeBehaviourMap[Keys.Down | Keys.Alt] = CellEditAtEdgeBehaviour.ChangeColumn;
        }

        /* Command handling */
        protected virtual void HandleEndEdit()
        {
            ListView.PossibleFinishCellEditing();
        }

        protected virtual void HandleCancelEdit()
        {
            ListView.CancelCellEdit();
        }

        protected virtual bool HandleCustomVerb(Keys keyData, CellEditCharacterBehaviour behaviour)
        {
            return false;
        }

        protected virtual void HandleRowChange(Keys keyData, CellEditCharacterBehaviour behaviour)
        {
            /* If we couldn't finish editing the current cell, don't try to move it */
            if (!ListView.PossibleFinishCellEditing())
            {
                return;
            }
            var olvi = ItemBeingEdited;
            var subItemIndex = SubItemIndexBeingEdited;
            var isGoingUp = behaviour == CellEditCharacterBehaviour.ChangeRowUp;
            /* Try to find a row above (or below) the currently edited cell
             * If we find one, start editing it and we're done. */
            var adjacentOlvi = GetAdjacentItemOrNull(olvi, isGoingUp);
            if (adjacentOlvi != null)
            {
                StartCellEditIfDifferent(adjacentOlvi, subItemIndex);
                return;
            }
            /* There is no adjacent row in the direction we want, so we must be on an edge. */
            CellEditAtEdgeBehaviour atEdgeBehaviour;
            CellEditKeyAtEdgeBehaviourMap.TryGetValue(keyData, out atEdgeBehaviour);
            switch (atEdgeBehaviour)
            {
                case CellEditAtEdgeBehaviour.Ignore:
                    break;

                case CellEditAtEdgeBehaviour.EndEdit:
                    ListView.PossibleFinishCellEditing();
                    break;

                case CellEditAtEdgeBehaviour.Wrap:
                    adjacentOlvi = GetAdjacentItemOrNull(null, isGoingUp);
                    StartCellEditIfDifferent(adjacentOlvi, subItemIndex);
                    break;

                case CellEditAtEdgeBehaviour.ChangeColumn:
                    /* Figure out the next editable column */
                    var editableColumnsInDisplayOrder = EditableColumnsInDisplayOrder;
                    var displayIndex = Math.Max(0, editableColumnsInDisplayOrder.IndexOf(ListView.GetColumn(subItemIndex)));
                    if (isGoingUp)
                    {
                        displayIndex = (editableColumnsInDisplayOrder.Count + displayIndex - 1) % editableColumnsInDisplayOrder.Count;
                    }
                    else
                    {
                        displayIndex = (displayIndex + 1) % editableColumnsInDisplayOrder.Count;
                    }
                    subItemIndex = editableColumnsInDisplayOrder[displayIndex].Index;
                    /* Wrap to the next row and start the cell edit */
                    adjacentOlvi = GetAdjacentItemOrNull(null, isGoingUp);
                    StartCellEditIfDifferent(adjacentOlvi, subItemIndex);
                    break;
            }
        }

        protected virtual void HandleColumnChange(Keys keyData, CellEditCharacterBehaviour behaviour)
        {
            /* If we couldn't finish editing the current cell, don't try to move it */
            /* Changing columns only works in details mode */
            if (!ListView.PossibleFinishCellEditing() || ListView.View != View.Details)
            {
                return;
            }
            var editableColumns = EditableColumnsInDisplayOrder;
            var olvi = ItemBeingEdited;
            var displayIndex = Math.Max(0, editableColumns.IndexOf(ListView.GetColumn(SubItemIndexBeingEdited)));
            var isGoingLeft = behaviour == CellEditCharacterBehaviour.ChangeColumnLeft;
            /* Are we trying to continue past one of the edges? */
            if ((isGoingLeft && displayIndex == 0) || (!isGoingLeft && displayIndex == editableColumns.Count - 1))
            {
                /* Yes, so figure out our at edge behaviour */
                CellEditAtEdgeBehaviour atEdgeBehaviour;
                CellEditKeyAtEdgeBehaviourMap.TryGetValue(keyData, out atEdgeBehaviour);
                switch (atEdgeBehaviour)
                {
                    case CellEditAtEdgeBehaviour.Ignore:
                        return;

                    case CellEditAtEdgeBehaviour.EndEdit:
                        HandleEndEdit();
                        return;

                    case CellEditAtEdgeBehaviour.ChangeRow:
                    case CellEditAtEdgeBehaviour.Wrap:
                        if (atEdgeBehaviour == CellEditAtEdgeBehaviour.ChangeRow)
                        {
                            olvi = GetAdjacentItem(olvi, isGoingLeft && displayIndex == 0);
                        }
                        if (isGoingLeft)
                        {
                            displayIndex = editableColumns.Count - 1;
                        }
                        else
                        {
                            displayIndex = 0;
                        }
                        break;
                }
            }
            else
            {
                if (isGoingLeft)
                {
                    displayIndex -= 1;
                }
                else
                {
                    displayIndex += 1;
                }
            }
            var subItemIndex = editableColumns[displayIndex].Index;
            StartCellEditIfDifferent(olvi, subItemIndex);
        }

        /* Utilities */
        protected void StartCellEditIfDifferent(OlvListItem olvi, int subItemIndex)
        {
            if (ItemBeingEdited == olvi && SubItemIndexBeingEdited == subItemIndex)
            {
                return;
            }
            ListView.EnsureVisible(olvi.Index);
            ListView.StartCellEdit(olvi, subItemIndex);
        }
        
        protected OlvListItem GetAdjacentItemOrNull(OlvListItem olvi, bool up)
        {
            return up ? ListView.GetPreviousItem(olvi) : ListView.GetNextItem(olvi);
        }
        
        protected OlvListItem GetAdjacentItem(OlvListItem olvi, bool up)
        {
            return GetAdjacentItemOrNull(olvi, up) ?? GetAdjacentItemOrNull(null, up);
        }

        protected List<OlvColumn> EditableColumnsInDisplayOrder
        {
            get
            {
                return ListView.ColumnsInDisplayOrder.Where(x => x.IsEditable).ToList();
            }
        }
    }
}
