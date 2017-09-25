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
using System.Drawing;
using System.Windows.Forms;

namespace libolv.DragDrop.DropSink
{
    [Flags]
    public enum DropTargetLocation
    {
        None = 0,
        Background = 0x01,
        Item = 0x02,
        BetweenItems = 0x04,
        AboveItem = 0x08,
        BelowItem = 0x10,
        SubItem = 0x20,
        RightOfItem = 0x40,
        LeftOfItem = 0x80
    }

    public interface IDropSink
    {
        ObjectListView ListView { get; set; }
        void DrawFeedback(Graphics g, Rectangle bounds);
        void Drop(DragEventArgs args);
        void Enter(DragEventArgs args);
        void GiveFeedback(GiveFeedbackEventArgs args);
        void Leave();
        void Over(DragEventArgs args);
        void QueryContinue(QueryContinueDragEventArgs args);
    }

    public class AbstractDropSink : IDropSink
    {
        public virtual ObjectListView ListView { get; set; }

        public virtual void DrawFeedback(Graphics g, Rectangle bounds)
        {
            /* Not implemented */
        }

        public virtual void Drop(DragEventArgs args)
        {
            Cleanup();
        }

        public virtual void Enter(DragEventArgs args)
        {
            /* Not implemented */
        }

        public virtual void Leave()
        {
            Cleanup();
        }

        public virtual void Over(DragEventArgs args)
        {
            /* Not implemented */
        }

        public virtual void GiveFeedback(GiveFeedbackEventArgs args)
        {
            args.UseDefaultCursors = true;
        }

        public virtual void QueryContinue(QueryContinueDragEventArgs args)
        {
            /* Not implemented */
        }

        /* Commands */
        protected virtual void Cleanup()
        {
            /* Not implemented */
        }
    }
}
