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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using libolv.Implementation;

namespace libolv.Rendering.Adornments
{
    public class GraphicAdornment
    {
        private ContentAlignment _adornmentCorner = ContentAlignment.MiddleCenter;
        private ContentAlignment _alignment = ContentAlignment.BottomRight;
        private ContentAlignment _referenceCorner = ContentAlignment.MiddleCenter;
        private int _transparency = 128;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ContentAlignment AdornmentCorner
        {
            get { return _adornmentCorner; }
            set { _adornmentCorner = value; }
        }

        [Category("ObjectListView"), Description("How will the adornment be aligned"),
         DefaultValue(ContentAlignment.BottomRight), NotifyParentProperty(true)]
        public ContentAlignment Alignment
        {
            get { return _alignment; }
            set
            {
                _alignment = value;
                ReferenceCorner = value;
                AdornmentCorner = value;
            }
        }

        [Category("ObjectListView"), Description("The offset by which the position of the adornment will be adjusted"),
         DefaultValue(typeof (Size), "0,0")]
        public Size Offset { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ContentAlignment ReferenceCorner
        {
            get { return _referenceCorner; }
            set { _referenceCorner = value; }
        }

        [Category("ObjectListView"), Description("The degree of rotation that will be applied to the adornment."),
         DefaultValue(0), NotifyParentProperty(true)]
        public int Rotation { get; set; }

        [Category("ObjectListView"),
         Description("The transparency of this adornment. 0 is completely transparent, 255 is completely opaque."),
         DefaultValue(128)]
        public int Transparency
        {
            get { return _transparency; }
            set { _transparency = Math.Min(255, Math.Max(0, value)); }
        }

        /* Calculations */

        public virtual Point CalculateAlignedPosition(Point pt, Size size, ContentAlignment corner)
        {
            switch (corner)
            {
                case ContentAlignment.TopLeft:
                    return pt;

                case ContentAlignment.TopCenter:
                    return new Point(pt.X - (size.Width/2), pt.Y);

                case ContentAlignment.TopRight:
                    return new Point(pt.X - size.Width, pt.Y);

                case ContentAlignment.MiddleLeft:
                    return new Point(pt.X, pt.Y - (size.Height/2));

                case ContentAlignment.MiddleCenter:
                    return new Point(pt.X - (size.Width/2), pt.Y - (size.Height/2));

                case ContentAlignment.MiddleRight:
                    return new Point(pt.X - size.Width, pt.Y - (size.Height/2));

                case ContentAlignment.BottomLeft:
                    return new Point(pt.X, pt.Y - size.Height);

                case ContentAlignment.BottomCenter:
                    return new Point(pt.X - (size.Width/2), pt.Y - size.Height);

                case ContentAlignment.BottomRight:
                    return new Point(pt.X - size.Width, pt.Y - size.Height);
            }
            /* Should never reach here */
            return pt;
        }

        public virtual Rectangle CreateAlignedRectangle(Rectangle r, Size sz)
        {
            return CreateAlignedRectangle(r, sz, ReferenceCorner, AdornmentCorner, Offset);
        }

        public virtual Rectangle CreateAlignedRectangle(Rectangle r, Size sz, ContentAlignment corner,
                                                        ContentAlignment referenceCorner, Size offset)
        {
            var referencePt = CalculateCorner(r, referenceCorner);
            var topLeft = CalculateAlignedPosition(referencePt, sz, corner);
            return new Rectangle(topLeft + offset, sz);
        }

        public virtual Point CalculateCorner(Rectangle r, ContentAlignment corner)
        {
            switch (corner)
            {
                case ContentAlignment.TopLeft:
                    return new Point(r.Left, r.Top);

                case ContentAlignment.TopCenter:
                    return new Point(r.X + (r.Width/2), r.Top);

                case ContentAlignment.TopRight:
                    return new Point(r.Right, r.Top);

                case ContentAlignment.MiddleLeft:
                    return new Point(r.Left, r.Top + (r.Height/2));

                case ContentAlignment.MiddleCenter:
                    return new Point(r.X + (r.Width/2), r.Top + (r.Height/2));

                case ContentAlignment.MiddleRight:
                    return new Point(r.Right, r.Top + (r.Height/2));

                case ContentAlignment.BottomLeft:
                    return new Point(r.Left, r.Bottom);

                case ContentAlignment.BottomCenter:
                    return new Point(r.X + (r.Width/2), r.Bottom);

                case ContentAlignment.BottomRight:
                    return new Point(r.Right, r.Bottom);
            }
            /* Should never reach here */
            return r.Location;
        }

        public virtual Rectangle CalculateItemBounds(OlvListItem item, OlvListSubItem subItem)
        {
            return item == null
                       ? Rectangle.Empty
                       : (subItem == null ? item.Bounds : item.GetSubItemBounds(item.SubItems.IndexOf(subItem)));
        }

        /* Commands */
        protected virtual void ApplyRotation(Graphics g, Rectangle r)
        {
            if (Rotation == 0)
            {
                return;
            }
            /* THINK: Do we want to reset the transform? I think we want to push a new transform */
            g.ResetTransform();
            var m = new Matrix();
            m.RotateAt(Rotation, new Point(r.Left + r.Width/2, r.Top + r.Height/2));
            g.Transform = m;
        }

        protected virtual void UnapplyRotation(Graphics g)
        {
            if (Rotation == 0) { return; }
            g.ResetTransform();
        }
    }
}
