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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using libolv.Implementation.VirtualGroups;

namespace libolv
{
    public class FastObjectListView : VirtualObjectListView
    {
        public FastObjectListView()
        {
            Init();
        }

        private void Init()
        {
            VirtualListDataSource = new FastObjectListDataSource(this);
            GroupingStrategy = new FastListGroupingStrategy();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override IEnumerable FilteredObjects
        {
            get
            {
                /* This is much faster than the base method */
                return ((FastObjectListDataSource)VirtualListDataSource).FilteredObjectList;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override IEnumerable Objects
        {
            get
            {
                /* This is much faster than the base method */
                return ((FastObjectListDataSource)VirtualListDataSource).ObjectList;
            }
            set { base.Objects = value; }
        }

        public override void Unsort()
        {
            ShowGroups = false;
            PrimarySortColumn = null;
            PrimarySortOrder = SortOrder.None;
            SetObjects(Objects);
        }
    }
}
