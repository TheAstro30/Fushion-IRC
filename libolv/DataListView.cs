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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using libolv.Implementation.Adapters;

namespace libolv
{
    public class DataListView : ObjectListView
    {
        private DataSourceAdapter _adapter;

        public DataListView()
        {
            Adapter = new DataSourceAdapter(this);
        }

        /* Public Properties */
        [Category("Data"), Description("Should the control automatically generate columns from the DataSource"), DefaultValue(true)]
        public bool AutoGenerateColumns
        {
            get { return Adapter.AutoGenerateColumns; }
            set { Adapter.AutoGenerateColumns = value; }
        }

        [Category("Data"), TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
        public virtual Object DataSource
        {
            get { return Adapter.DataSource; }
            set { Adapter.DataSource = value; }
        }
        
        [Category("Data"), Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof (UITypeEditor)), DefaultValue("")]
        public virtual string DataMember
        {
            get { return Adapter.DataMember; }
            set { Adapter.DataMember = value; }
        }

        /* Implementation properties */
        protected DataSourceAdapter Adapter
        {
            get
            {
                Debug.Assert(_adapter != null, "Data adapter should not be null");
                return _adapter;
            }
            set { _adapter = value; }
        }

        /* Object manipulations */
        public override void AddObjects(ICollection modelObjects)
        {
            /* Empty */
        }

        public override void RemoveObjects(ICollection modelObjects)
        {
            /* Empty */
        }
    }
}
