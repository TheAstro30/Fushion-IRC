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
using System.Windows.Forms;
using System.Drawing;

namespace libolv.SubControls
{
    public class ToolStripCheckedListBox : ToolStripControlHost
    {
        public event ItemCheckEventHandler ItemCheck;

        public ToolStripCheckedListBox() : base(new CheckedListBox())
        {
            CheckedListBoxControl.MaximumSize = new Size(400, 700);
            CheckedListBoxControl.ThreeDCheckBoxes = true;
            CheckedListBoxControl.CheckOnClick = true;
            CheckedListBoxControl.SelectionMode = SelectionMode.One;
        }

        public CheckedListBox CheckedListBoxControl
        {
            get { return Control as CheckedListBox; }
        }

        public CheckedListBox.ObjectCollection Items
        {
            get { return CheckedListBoxControl.Items; }
        }

        public bool CheckedOnClick
        {
            get { return CheckedListBoxControl.CheckOnClick; }
            set { CheckedListBoxControl.CheckOnClick = value; }
        }

        public CheckedListBox.CheckedItemCollection CheckedItems
        {
            get { return CheckedListBoxControl.CheckedItems; }
        }

        public void AddItem(object item, bool isChecked)
        {
            Items.Add(item);
            if (isChecked)
            {
                CheckedListBoxControl.SetItemChecked(Items.Count - 1, true);
            }
        }

        public void AddItem(object item, CheckState state)
        {
            Items.Add(item);
            CheckedListBoxControl.SetItemCheckState(Items.Count - 1, state);
        }

        public CheckState GetItemCheckState(int i)
        {
            return CheckedListBoxControl.GetItemCheckState(i);
        }

        public void SetItemState(int i, CheckState checkState)
        {
            if (i >= 0 && i < Items.Count)
            {
                CheckedListBoxControl.SetItemCheckState(i, checkState);
            }
        }

        public void CheckAll()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                CheckedListBoxControl.SetItemChecked(i, true);
            }
        }

        public void UncheckAll()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                CheckedListBoxControl.SetItemChecked(i, false);
            }
        }

        /* Events */
        protected override void OnSubscribeControlEvents(Control c)
        {
            base.OnSubscribeControlEvents(c);
            var control = (CheckedListBox)c;
            control.ItemCheck += OnItemCheck;
        }

        protected override void OnUnsubscribeControlEvents(Control c)
        {
            base.OnUnsubscribeControlEvents(c);
            var control = (CheckedListBox)c;
            control.ItemCheck -= OnItemCheck;
        }

        private void OnItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (ItemCheck != null)
            {
                ItemCheck(this, e);
            }
        }
    }
}
