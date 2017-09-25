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
using System.Linq;

namespace libolv.DragDrop.DropSink
{
    public class ModelDropEventArgs : OlvDropEventArgs
    {
        private IList _dragModels;
        private readonly ArrayList _toBeRefreshed = new ArrayList();

        public IList SourceModels
        {
            get { return _dragModels; }
            internal set
            {
                _dragModels = value;
                var tlv = SourceListView as TreeListView;
                if (tlv == null) { return; }
                foreach (var parent in SourceModels.Cast<object>().Select(tlv.GetParent).Where(parent => !_toBeRefreshed.Contains(parent)))
                {
                    _toBeRefreshed.Add(parent);
                }
            }
        }

        public ObjectListView SourceListView { get; internal set; }
        public object TargetModel { get; internal set; }

        public void RefreshObjects()
        {
            _toBeRefreshed.AddRange(SourceModels);
            var tlv = SourceListView as TreeListView;
            if (tlv == null)
            {
                SourceListView.RefreshObjects(_toBeRefreshed);
            }
            else
            {
                tlv.RebuildAll(true);
            }
            var tlv2 = ListView as TreeListView;
            if (tlv2 == null)
            {
                ListView.RefreshObject(TargetModel);
            }
            else
            {
                tlv2.RebuildAll(true);
            }
        }
    }
}
