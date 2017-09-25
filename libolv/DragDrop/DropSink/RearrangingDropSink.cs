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

namespace libolv.DragDrop.DropSink
{
    public class RearrangingDropSink : SimpleDropSink
    {
        public RearrangingDropSink()
        {
            CanDropBetween = true;
            CanDropOnBackground = true;
            CanDropOnItem = false;
        }

        public RearrangingDropSink(bool acceptDropsFromOtherLists) : this()
        {
            AcceptExternal = acceptDropsFromOtherLists;
        }

        protected override void OnModelCanDrop(ModelDropEventArgs args)
        {
            base.OnModelCanDrop(args);
            if (args.Handled)
            {
                return;
            }
            args.Effect = DragDropEffects.Move;
            /* Don't allow drops from other list, if that's what's configured */
            if (!AcceptExternal && args.SourceListView != ListView)
            {
                args.Effect = DragDropEffects.None;
                args.DropTargetLocation = DropTargetLocation.None;
                args.InfoMessage = "This list doesn't accept drops from other lists";
            }
            /* If we are rearranging a list, don't allow drops on the background */
            if (args.DropTargetLocation != DropTargetLocation.Background || args.SourceListView != ListView) { return; }
            args.Effect = DragDropEffects.None;
            args.DropTargetLocation = DropTargetLocation.None;
        }

        protected override void OnModelDropped(ModelDropEventArgs args)
        {
            base.OnModelDropped(args);
            if (!args.Handled)
            {
                RearrangeModels(args);
            }
        }

        public virtual void RearrangeModels(ModelDropEventArgs args)
        {
            switch (args.DropTargetLocation)
            {
                case DropTargetLocation.AboveItem:
                    ListView.MoveObjects(args.DropTargetIndex, args.SourceModels);
                    break;

                case DropTargetLocation.BelowItem:
                    ListView.MoveObjects(args.DropTargetIndex + 1, args.SourceModels);
                    break;

                case DropTargetLocation.Background:
                    ListView.AddObjects(args.SourceModels);
                    break;

                default:
                    return;
            }
            if (args.SourceListView != ListView)
            {
                args.SourceListView.RemoveObjects(args.SourceModels);
            }
        }
    }
}
