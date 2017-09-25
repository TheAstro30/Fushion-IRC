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
using System.Windows.Forms;
using libolv.Implementation;

namespace libolv.DragDrop.DragSource
{
    public class SimpleDragSource : IDragSource
    {
        public SimpleDragSource()
        {
            /* Empty */
        }

        public SimpleDragSource(bool refreshAfterDrop)
        {
            RefreshAfterDrop = refreshAfterDrop;
        }

        /* Public properties */
        public bool RefreshAfterDrop { get; set; }

        /* IDragSource Members */
        public virtual Object StartDrag(ObjectListView olv, MouseButtons button, OlvListItem item)
        {
            /* We only drag on left mouse */
            return button != MouseButtons.Left ? null : CreateDataObject(olv);
        }

        public virtual DragDropEffects GetAllowedEffects(Object data)
        {
            return DragDropEffects.All | DragDropEffects.Link; /* why didn't MS include 'Link' in 'All'?? */
        }

        public virtual void EndDrag(Object dragObject, DragDropEffects effect)
        {
            var data = dragObject as OlvDataObject;
            if (data == null)
            {
                return;
            }
            if (RefreshAfterDrop)
            {
                data.ListView.RefreshObjects(data.ModelObjects);
            }
        }

        protected virtual object CreateDataObject(ObjectListView olv)
        {
            return new OlvDataObject(olv);
        }
    }
}
