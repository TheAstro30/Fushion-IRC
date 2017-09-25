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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using libolv.Implementation;

namespace libolv.Rendering.Renderers
{
    [Browsable(true), ToolboxItem(true)]
    public class BaseRenderer : AbstractRenderer
    {
        private bool _canWrap;
        private Rectangle? _cellPadding;
        private StringAlignment? _cellVerticalAlignment;
        private int _spacing = 1;
        private bool _useGdiTextRendering = true;
        private Object _aspect;
        private OlvColumn _column;
        private Font _font;
        private Brush _textBrush;

        /* Properties */
        [Category("Appearance"), Description("Can the renderer wrap text that does not fit completely within the cell"), DefaultValue(false)]
        public bool CanWrap
        {
            get { return _canWrap; }
            set
            {
                _canWrap = value;
                if (_canWrap)
                {
                    UseGdiTextRendering = false;
                }
            }
        }

        [Category("ObjectListView"), Description("The number of pixels that renderer will leave empty around the edge of the cell"), DefaultValue(null)]
        public Rectangle? CellPadding
        {
            get { return _cellPadding; }
            set { _cellPadding = value; }
        }

        [Category("ObjectListView"), Description("How will cell values be vertically aligned?"), DefaultValue(null)]
        public virtual StringAlignment? CellVerticalAlignment
        {
            get { return _cellVerticalAlignment; }
            set { _cellVerticalAlignment = value; }
        }

        [Browsable(false)]
        protected virtual Rectangle? EffectiveCellPadding
        {
            get
            {
                if (_cellPadding.HasValue)
                {
                    return _cellPadding.Value;
                }
                if (OlvSubItem != null && OlvSubItem.CellPadding.HasValue)
                {
                    return OlvSubItem.CellPadding.Value;
                }
                if (ListItem != null && ListItem.CellPadding.HasValue)
                {
                    return ListItem.CellPadding.Value;
                }
                if (Column != null && Column.CellPadding.HasValue)
                {
                    return Column.CellPadding.Value;
                }
                return ListView != null && ListView.CellPadding.HasValue
                           ? (Rectangle?)ListView.CellPadding.Value
                           : null;
            }
        }

        [Browsable(false)]
        protected virtual StringAlignment EffectiveCellVerticalAlignment
        {
            get
            {
                if (_cellVerticalAlignment.HasValue)
                {
                    return _cellVerticalAlignment.Value;
                }
                if (OlvSubItem != null && OlvSubItem.CellVerticalAlignment.HasValue)
                {
                    return OlvSubItem.CellVerticalAlignment.Value;
                }
                if (ListItem != null && ListItem.CellVerticalAlignment.HasValue)
                {
                    return ListItem.CellVerticalAlignment.Value;
                }
                if (Column != null && Column.CellVerticalAlignment.HasValue)
                {
                    return Column.CellVerticalAlignment.Value;
                }
                return ListView != null ? ListView.CellVerticalAlignment : StringAlignment.Center;
            }
        }

        [Category("Appearance"), Description("The image list from which keyed images will be fetched for drawing."), DefaultValue(null)]
        public ImageList ImageList { get; set; }

        [Category("Appearance"), Description("When rendering multiple images, how many pixels should be between each image?"), DefaultValue(1)]
        public int Spacing
        {
            get { return _spacing; }
            set { _spacing = value; }
        }

        [Category("Appearance"), Description("Should text be rendered using GDI routines?"), DefaultValue(true)]
        public bool UseGdiTextRendering
        {
            get
            {
                /* Can't use GDI routines on a GDI+ printer context */
                return !IsPrinting && _useGdiTextRendering;
            }
            set { _useGdiTextRendering = value; }
        }

        /* State Properties */
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Object Aspect
        {
            get { return _aspect ?? (_aspect = _column.GetValue(RowObject)); }
            set { _aspect = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle Bounds { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OlvColumn Column
        {
            get { return _column; }
            set { _column = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DrawListViewItemEventArgs DrawItemEvent { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DrawListViewSubItemEventArgs Event { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font Font
        {
            get
            {
                if (_font != null || ListItem == null)
                {
                    return _font;
                }
                if (SubItem == null || ListItem.UseItemStyleForSubItems)
                {
                    return ListItem.Font;
                }
                return SubItem.Font;
            }
            set { _font = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ImageList ImageListOrDefault
        {
            get { return ImageList ?? ListView.SmallImageList; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDrawBackground
        {
            get { return !IsPrinting; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsItemSelected { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPrinting { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OlvListItem ListItem { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObjectListView ListView { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OlvListSubItem OlvSubItem { get; private set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object RowObject { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OlvListSubItem SubItem
        {
            get { return OlvSubItem; }
            set { OlvSubItem = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Brush TextBrush
        {
            get
            {
                return _textBrush ?? new SolidBrush(GetForegroundColor());
            }
            set { _textBrush = value; }
        }

        /* Private methods */
        private void ClearState()
        {
            Event = null;
            DrawItemEvent = null;
            Aspect = null;
            Font = null;
            TextBrush = null;
        }

        /* Utilities */
        protected virtual Rectangle AlignRectangle(Rectangle outer, Rectangle inner)
        {
            var r = new Rectangle(outer.Location, inner.Size);
            /* Align horizontally depending on the column alignment */
            if (inner.Width < outer.Width)
            {
                r.X = AlignHorizontally(outer, inner);
            }
            /* Align vertically too */
            if (inner.Height < outer.Height)
            {
                r.Y = AlignVertically(outer, inner);
            }
            return r;
        }

        protected int AlignHorizontally(Rectangle outer, Rectangle inner)
        {
            var alignment = Column == null ? HorizontalAlignment.Left : Column.TextAlign;
            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    return outer.Left + 1;

                case HorizontalAlignment.Center:
                    return outer.Left + ((outer.Width - inner.Width) / 2);

                case HorizontalAlignment.Right:
                    return outer.Right - inner.Width - 1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected int AlignVertically(Rectangle outer, Rectangle inner)
        {
            return AlignVertically(outer, inner.Height);
        }

        protected int AlignVertically(Rectangle outer, int innerHeight)
        {
            switch (EffectiveCellVerticalAlignment)
            {
                case StringAlignment.Near:
                    return outer.Top + 1;

                case StringAlignment.Center:
                    return outer.Top + ((outer.Height - innerHeight) / 2);

                case StringAlignment.Far:
                    return outer.Bottom - innerHeight - 1;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual Rectangle CalculateAlignedRectangle(Graphics g, Rectangle r)
        {
            if (Column == null || Column.TextAlign == HorizontalAlignment.Left)
            {
                return r;
            }
            var width = CalculateCheckBoxWidth(g);
            width += CalculateImageWidth(g, GetImageSelector());
            width += CalculateTextWidth(g, GetText());
            /* If the combined width is greater than the whole cell, we just use the cell itself */
            return width >= r.Width ? r : AlignRectangle(r, new Rectangle(0, 0, width, r.Height));
        }

        protected virtual int CalculateCheckBoxWidth(Graphics g)
        {
            return ListView.CheckBoxes && ColumnIsPrimary
                       ? CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.UncheckedNormal).Width + 6
                       : 0;
        }

        protected virtual int CalculateImageWidth(Graphics g, object imageSelector)
        {
            if (imageSelector == null || imageSelector == DBNull.Value)
            {
                return 0;
            }
            /* Draw from the image list (most common case) */
            var il = ImageListOrDefault;
            if (il != null)
            {
                var selectorAsInt = -1;
                if (imageSelector is Int32)
                {
                    selectorAsInt = (Int32)imageSelector;
                }
                else
                {
                    var selectorAsString = imageSelector as String;
                    if (selectorAsString != null)
                    {
                        selectorAsInt = il.Images.IndexOfKey(selectorAsString);
                    }
                }
                if (selectorAsInt >= 0)
                {
                    return il.ImageSize.Width;
                }
            }
            /* Is the selector actually an image? */
            var image = imageSelector as Image;
            return image != null ? image.Width : 0;
        }

        protected virtual int CalculateTextWidth(Graphics g, string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return 0;
            }
            if (UseGdiTextRendering)
            {
                var proposedSize = new Size(int.MaxValue, int.MaxValue);
                return
                    TextRenderer.MeasureText(g, txt, Font, proposedSize,
                                             TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix).Width;
            }
            using (var fmt = new StringFormat())
            {
                fmt.Trimming = StringTrimming.EllipsisCharacter;
                return 1 + (int)g.MeasureString(txt, Font, int.MaxValue, fmt).Width;
            }
        }

        public virtual Color GetBackgroundColor()
        {
            if (!ListView.Enabled)
            {
                return SystemColors.Control;
            }
            if (IsItemSelected && !ListView.UseTranslucentSelection && ListView.FullRowSelect)
            {
                if (ListView.Focused)
                {
                    return ListView.HighlightBackgroundColorOrDefault;
                }
                if (!ListView.HideSelection)
                {
                    return ListView.UnfocusedHighlightBackgroundColorOrDefault;
                }
            }
            return SubItem == null || ListItem.UseItemStyleForSubItems
                       ? ListItem.BackColor
                       : SubItem.BackColor;
        }

        public virtual Color GetForegroundColor()
        {
            if (IsItemSelected && !ListView.UseTranslucentSelection && (ColumnIsPrimary || ListView.FullRowSelect))
            {
                if (ListView.Focused)
                {
                    return ListView.HighlightForegroundColorOrDefault;
                }
                if (!ListView.HideSelection)
                {
                    return ListView.UnfocusedHighlightForegroundColorOrDefault;
                }
            }
            return SubItem == null || ListItem.UseItemStyleForSubItems
                       ? ListItem.ForeColor
                       : SubItem.ForeColor;
        }

        protected virtual Image GetImage()
        {
            return GetImage(GetImageSelector());
        }

        protected virtual Image GetImage(Object imageSelector)
        {
            if (imageSelector == null || imageSelector == DBNull.Value)
            {
                return null;
            }
            var il = ImageListOrDefault;
            if (il != null)
            {
                if (imageSelector is Int32)
                {
                    var index = (Int32)imageSelector;
                    return index < 0 || index >= il.Images.Count ? null : il.Images[index];
                }
                var str = imageSelector as String;
                if (str != null)
                {
                    return il.Images.ContainsKey(str) ? il.Images[str] : null;
                }
            }
            return imageSelector as Image;
        }

        protected virtual Object GetImageSelector()
        {
            return ColumnIsPrimary ? ListItem.ImageSelector : OlvSubItem.ImageSelector;
        }

        protected virtual string GetText()
        {
            return SubItem == null ? ListItem.Text : SubItem.Text;
        }

        protected virtual Color GetTextBackgroundColor()
        {
            //TODO: Refactor with GetBackgroundColor() - they are almost identical
            if (IsItemSelected && !ListView.UseTranslucentSelection && (ColumnIsPrimary || ListView.FullRowSelect))
            {
                if (ListView.Focused)
                {
                    return ListView.HighlightBackgroundColorOrDefault;
                }
                if (!ListView.HideSelection)
                {
                    return ListView.UnfocusedHighlightBackgroundColorOrDefault;
                }
            }
            return SubItem == null || ListItem.UseItemStyleForSubItems
                       ? ListItem.BackColor
                       : SubItem.BackColor;
        }

        /* IRenderer members */
        public override bool RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, object rowObject)
        {
            ClearState();
            DrawItemEvent = e;
            ListItem = (OlvListItem)e.Item;
            SubItem = null;
            ListView = (ObjectListView)ListItem.ListView;
            Column = ListView.GetColumn(0);
            RowObject = rowObject;
            Bounds = itemBounds;
            IsItemSelected = ListItem.Selected;
            return OptionalRender(g, itemBounds);
        }

        public override bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject)
        {
            ClearState();
            Event = e;
            ListItem = (OlvListItem)e.Item;
            SubItem = (OlvListSubItem)e.SubItem;
            ListView = (ObjectListView)ListItem.ListView;
            Column = (OlvColumn)e.Header;
            RowObject = rowObject;
            Bounds = cellBounds;
            IsItemSelected = ListItem.Selected;
            return OptionalRender(g, cellBounds);
        }

        public override void HitTest(OlvListViewHitTestInfo hti, int x, int y)
        {
            ClearState();
            ListView = hti.ListView;
            ListItem = hti.Item;
            SubItem = hti.SubItem;
            Column = hti.Column;
            RowObject = hti.RowObject;
            IsItemSelected = ListItem.Selected;
            Bounds = SubItem == null ? ListItem.Bounds : ListItem.GetSubItemBounds(Column.Index);
            using (var g = ListView.CreateGraphics())
            {
                HandleHitTest(g, hti, x, y);
            }
        }

        public override Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex, Size preferredSize)
        {
            ClearState();
            ListView = (ObjectListView)item.ListView;
            ListItem = item;
            SubItem = item.GetSubItem(subItemIndex);
            Column = ListView.GetColumn(subItemIndex);
            RowObject = item.RowObject;
            IsItemSelected = ListItem.Selected;
            Bounds = cellBounds;
            return HandleGetEditRectangle(g, cellBounds, item, subItemIndex, preferredSize);
        }

        /* IRenderer implementation */
        public virtual bool OptionalRender(Graphics g, Rectangle r)
        {
            if (ListView.View == View.Details)
            {
                Render(g, r);
                return true;
            }
            return false;
        }

        public virtual void Render(Graphics g, Rectangle r)
        {
            StandardRender(g, r);
        }

        protected virtual void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
        {
            var r = CalculateAlignedRectangle(g, Bounds);
            StandardHitTest(g, hti, r, x, y);
        }

        protected virtual Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex, Size preferredSize)
        {
            /* MAINTAINER NOTE: This type testing is wrong (design-wise). The base class should return cell bounds,
             * and a more specialized class should return StandardGetEditRectangle(). But BaseRenderer is used directly
             * to draw most normal cells, as well as being directly subclassed for user implemented renderers. And this
             * method needs to return different bounds in each of those cases. We should have a StandardRenderer and make
             * BaseRenderer into an ABC -- but that would break too much existing code. And so we have this hack :(

             * If we are a standard renderer, return the position of the text, otherwise, use the whole cell.
             */
            return GetType() == typeof(BaseRenderer) ? StandardGetEditRectangle(g, cellBounds, preferredSize) : cellBounds;
        }

        /* Standard IRenderer implementations */
        protected void StandardRender(Graphics g, Rectangle r)
        {
            DrawBackground(g, r);
            /* Adjust the first columns rectangle to match the padding used by the native mode of the ListView */
            if (ColumnIsPrimary)
            {
                r.X += 3;
                r.Width -= 1;
            }
            r = ApplyCellPadding(r);
            DrawAlignedImageAndText(g, r);
            /* Show where the bounds of the cell padding are (debugging) */
            if (ObjectListView.ShowCellPaddingBounds)
            {
                g.DrawRectangle(Pens.Purple, r);
            }
        }

        public virtual Rectangle ApplyCellPadding(Rectangle r)
        {
            var padding = EffectiveCellPadding;
            if (!padding.HasValue)
            {
                return r;
            }
            /* The two subtractions below look wrong, but are correct! */
            var paddingRectangle = padding.Value;
            r.Width -= paddingRectangle.Right;
            r.Height -= paddingRectangle.Bottom;
            r.Offset(paddingRectangle.Location);
            return r;
        }

        protected void StandardHitTest(Graphics g, OlvListViewHitTestInfo hti, Rectangle bounds, int x, int y)
        {
            var r = bounds;
            /* Match tweaking from renderer */
            if (ColumnIsPrimary && !(this is TreeListView.TreeRenderer))
            {
                r.X += 3;
                r.Width -= 1;
            }
            r = ApplyCellPadding(r);
            var width = 0;
            /* Did they hit a check box on the primary column? */
            if (ColumnIsPrimary && ListView.CheckBoxes)
            {
                var r2 = CalculateCheckBoxBounds(g, r);
                var r3 = r2;
                r3.Inflate(2, 2); /* slightly larger hit area */
                if (r3.Contains(x, y))
                {
                    hti.HitTestLocation = HitTestLocation.CheckBox;
                    return;
                }
                width = r3.Width;
            }
            /* Did they hit the image? If they hit the image of a non-primary column that has a checkbox,
             * it counts as a checkbox hit */
            r.X += width;
            r.Width -= width;
            width = CalculateImageWidth(g, GetImageSelector());
            var rTwo = r;
            rTwo.Width = width;
            if (rTwo.Contains(x, y))
            {
                if (Column != null && (Column.Index > 0 && Column.CheckBoxes))
                {
                    hti.HitTestLocation = HitTestLocation.CheckBox;
                }
                else
                {
                    hti.HitTestLocation = HitTestLocation.Image;
                }
                return;
            }
            /* Did they hit the text? */
            r.X += width;
            r.Width -= width;
            width = CalculateTextWidth(g, GetText());
            rTwo = r;
            rTwo.Width = width;
            if (rTwo.Contains(x, y))
            {
                hti.HitTestLocation = HitTestLocation.Text;
                return;
            }
            hti.HitTestLocation = HitTestLocation.InCell;
        }

        protected Rectangle StandardGetEditRectangle(Graphics g, Rectangle cellBounds, Size preferredSize)
        {
            var r = CalculateAlignedRectangle(g, cellBounds);
            r = CalculatePaddedAlignedBounds(g, r, preferredSize);
            var width = CalculateCheckBoxWidth(g);
            width += CalculateImageWidth(g, GetImageSelector());
            /* Indent the primary column by the required amount */
            if (ColumnIsPrimary && ListItem.IndentCount > 0)
            {
                var indentWidth = ListView.SmallImageSize.Width;
                width += (indentWidth * ListItem.IndentCount);
            }

            /* If there was either a check box or an image, take the check box and the image out of the rectangle,
             * but ensure that there is minimum width to the editor */
            if (width > 0)
            {
                r.X += width;
                r.Width = Math.Max(r.Width - width, 40);
            }
            return r;
        }

        protected Rectangle CalculatePaddedAlignedBounds(Graphics g, Rectangle bounds, Size preferredSize)
        {
            var r = ApplyCellPadding(bounds);
            r = AlignRectangle(r, new Rectangle(0, 0, r.Width, preferredSize.Height));
            return r;
        }

        /* Drawing routines */
        protected virtual void DrawAlignedImage(Graphics g, Rectangle r, Image image)
        {
            if (image == null)
            {
                return;
            }
            /* By default, the image goes in the top left of the rectangle */
            var imageBounds = new Rectangle(r.Location, image.Size);
            /* If the image is too tall to be drawn in the space provided, proportionally scale it down.
             * Too wide images are not scaled. */
            if (image.Height > r.Height)
            {
                var scaleRatio = (float)r.Height / image.Height;
                imageBounds.Width = (int)(image.Width * scaleRatio);
                imageBounds.Height = r.Height - 1;
            }
            /* Align and draw our (possibly scaled) image */
            g.DrawImage(image, AlignRectangle(r, imageBounds));
        }

        protected virtual void DrawAlignedImageAndText(Graphics g, Rectangle r)
        {
            DrawImageAndText(g, CalculateAlignedRectangle(g, r));
        }

        protected virtual void DrawBackground(Graphics g, Rectangle r)
        {
            if (!IsDrawBackground)
            {
                return;
            }
            var backgroundColor = GetBackgroundColor();
            using (Brush brush = new SolidBrush(backgroundColor))
            {
                g.FillRectangle(brush, r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2);
            }
        }

        protected virtual int DrawCheckBox(Graphics g, Rectangle r)
        {
            // TODO: Unify this with CheckStateRenderer
            var imageIndex = ListItem.StateImageIndex;
            if (IsPrinting)
            {
                if (ListView.StateImageList == null || imageIndex < 0)
                {
                    return 0;
                }
                return DrawImage(g, r, ListView.StateImageList.Images[imageIndex]) + 4;
            }
            r = CalculateCheckBoxBounds(g, r);
            var boxState = GetCheckBoxState(ListItem.CheckState);
            CheckBoxRenderer.DrawCheckBox(g, r.Location, boxState);
            return CheckBoxRenderer.GetGlyphSize(g, boxState).Width + 6;
        }

        protected virtual CheckBoxState GetCheckBoxState(CheckState checkState)
        {
            /* Should the checkbox be drawn as disabled? */
            if (IsCheckBoxDisabled)
            {
                switch (checkState)
                {
                    case CheckState.Checked:
                        return CheckBoxState.CheckedDisabled;

                    case CheckState.Unchecked:
                        return CheckBoxState.UncheckedDisabled;

                    default:
                        return CheckBoxState.MixedDisabled;
                }
            }
            /* Is the cursor currently over this checkbox? */
            if (IsItemHot)
            {
                switch (checkState)
                {
                    case CheckState.Checked:
                        return CheckBoxState.CheckedHot;

                    case CheckState.Unchecked:
                        return CheckBoxState.UncheckedHot;

                    default:
                        return CheckBoxState.MixedHot;
                }
            }
            /* Not hot and not disabled -- just draw it normally */
            switch (checkState)
            {
                case CheckState.Checked:
                    return CheckBoxState.CheckedNormal;

                case CheckState.Unchecked:
                    return CheckBoxState.UncheckedNormal;

                default:
                    return CheckBoxState.MixedNormal;
            }

        }

        protected virtual bool IsCheckBoxDisabled
        {
            get
            {
                return ListView.RenderNonEditableCheckboxesAsDisabled &&
                       (ListView.CellEditActivation == ObjectListView.CellEditActivateMode.None ||
                        (Column != null && !Column.IsEditable));
            }
        }

        protected bool IsItemHot
        {
            get
            {
                return ListView != null &&
                       ListItem != null &&
                       ListView.HotRowIndex == ListItem.Index &&
                       ListView.HotColumnIndex == (Column == null ? 0 : Column.Index) &&
                       ListView.HotCellHitLocation == HitTestLocation.CheckBox;
            }
        }

        protected Rectangle CalculateCheckBoxBounds(Graphics g, Rectangle cellBounds)
        {
            var checkBoxSize = CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.CheckedNormal);
            return AlignRectangle(cellBounds, new Rectangle(0, 0, checkBoxSize.Width, checkBoxSize.Height));
        }

        protected virtual int DrawImage(Graphics g, Rectangle r, Object imageSelector)
        {
            if (imageSelector == null || imageSelector == DBNull.Value)
            {
                return 0;
            }
            /* Draw from the image list (most common case) */
            var il = ListView.SmallImageList;
            if (il != null)
            {
                var selectorAsInt = -1;
                if (imageSelector is Int32)
                {
                    selectorAsInt = (Int32)imageSelector;
                    if (selectorAsInt >= il.Images.Count)
                    {
                        selectorAsInt = -1;
                    }
                }
                else
                {
                    var selectorAsString = imageSelector as String;
                    if (selectorAsString != null)
                    {
                        selectorAsInt = il.Images.IndexOfKey(selectorAsString);
                    }
                }
                if (selectorAsInt >= 0)
                {
                    if (IsPrinting)
                    {
                        /* For some reason, printing from an image list doesn't work onto a printer context
                         * so get the image from the list and fall through to the "print an image" case */
                        imageSelector = il.Images[selectorAsInt];
                    }
                    else
                    {
                        if (il.ImageSize.Height < r.Height)
                        {
                            r.Y = AlignVertically(r, new Rectangle(Point.Empty, il.ImageSize));
                        }
                        /* If we are not printing, it's probable that the given Graphics object is double buffered using a
                         * BufferedGraphics object. But the ImageList.Draw method doesn't honor the Translation matrix that's
                         * probably in effect on the buffered graphics. So we have to calculate our drawing rectangle,
                         * relative to the cells natural boundaries. This effectively simulates the Translation matrix. */
                        var r2 = new Rectangle(r.X - Bounds.X, r.Y - Bounds.Y, r.Width, r.Height);
                        il.Draw(g, r2.Location, selectorAsInt);
                        return il.ImageSize.Width;
                    }
                }
            }
            /* Is the selector actually an image? */
            var image = imageSelector as Image;
            if (image != null)
            {
                var top = r.Y;
                if (image.Size.Height < r.Height)
                {
                    r.Y = AlignVertically(r, new Rectangle(Point.Empty, image.Size));
                }
                g.DrawImageUnscaled(image, r.X, top);
                return image.Width;
            }
            return 0;
        }

        protected virtual void DrawImageAndText(Graphics g, Rectangle r)
        {
            int offset;
            if (ListView.CheckBoxes && ColumnIsPrimary)
            {
                offset = DrawCheckBox(g, r);
                r.X += offset;
                r.Width -= offset;
            }
            offset = DrawImage(g, r, GetImageSelector());
            r.X += offset;
            r.Width -= offset;
            DrawText(g, r, GetText());
        }

        protected virtual int DrawImages(Graphics g, Rectangle r, ICollection imageSelectors)
        {
            /* Collect the non-null images */
            var images = imageSelectors.Cast<object>().Select(GetImage).Where(image => image != null).ToList();
            /* Figure out how much space they will occupy */
            var width = 0;
            var height = 0;
            foreach (var image in images)
            {
                width += (image.Width + Spacing);
                height = Math.Max(height, image.Height);
            }
            /* Align the collection of images within the cell */
            var r2 = AlignRectangle(r, new Rectangle(0, 0, width, height));
            /* Finally, draw all the images in their correct location */
            var pt = r2.Location;
            foreach (var image in images)
            {
                g.DrawImage(image, pt);
                pt.X += (image.Width + Spacing);
            }
            /* Return the width that the images occupy */
            return width;
        }

        public virtual void DrawText(Graphics g, Rectangle r, String txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return;
            }
            if (UseGdiTextRendering)
            {
                DrawTextGdi(g, r, txt);
            }
            else
            {
                DrawTextGdiPlus(g, r, txt);
            }
        }

        protected virtual void DrawTextGdi(Graphics g, Rectangle r, String txt)
        {
            var backColor = Color.Transparent;
            if (IsDrawBackground && IsItemSelected && ColumnIsPrimary && !ListView.FullRowSelect)
            {
                backColor = GetTextBackgroundColor();
            }
            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
                        TextFormatFlags.PreserveGraphicsTranslateTransform |
                        CellVerticalAlignmentAsTextFormatFlag;
            /* I think there is a bug in the TextRenderer. Setting or not setting SingleLine doesn't make 
             * any difference -- it is always single line. */
            if (!CanWrap)
            {
                flags |= TextFormatFlags.SingleLine;
            }
            TextRenderer.DrawText(g, txt, Font, r, GetForegroundColor(), backColor, flags);
        }

        private bool ColumnIsPrimary
        {
            get { return Column != null && Column.Index == 0; }
        }

        protected TextFormatFlags CellVerticalAlignmentAsTextFormatFlag
        {
            get
            {
                switch (EffectiveCellVerticalAlignment)
                {
                    case StringAlignment.Near:
                        return TextFormatFlags.Top;

                    case StringAlignment.Center:
                        return TextFormatFlags.VerticalCenter;

                    case StringAlignment.Far:
                        return TextFormatFlags.Bottom;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected virtual StringFormat StringFormatForGdiPlus
        {
            get
            {
                var fmt = new StringFormat
                {
                    LineAlignment = EffectiveCellVerticalAlignment,
                    Trimming = StringTrimming.EllipsisCharacter,
                    Alignment = Column == null ? StringAlignment.Near : Column.TextStringAlign
                };
                if (!CanWrap)
                {
                    fmt.FormatFlags = StringFormatFlags.NoWrap;
                }
                return fmt;
            }
        }

        protected virtual void DrawTextGdiPlus(Graphics g, Rectangle r, String txt)
        {
            using (var fmt = StringFormatForGdiPlus)
            {
                /* Draw the background of the text as selected, if it's the primary column
                 * and it's selected and it's not in FullRowSelect mode. */
                var f = Font;
                if (IsDrawBackground && IsItemSelected && ColumnIsPrimary && !ListView.FullRowSelect)
                {
                    var size = g.MeasureString(txt, f, r.Width, fmt);
                    var r2 = r;
                    r2.Width = (int)size.Width + 1;
                    using (Brush brush = new SolidBrush(ListView.HighlightBackgroundColorOrDefault))
                    {
                        g.FillRectangle(brush, r2);
                    }
                }
                RectangleF rf = r;
                g.DrawString(txt, f, TextBrush, rf, fmt);
            }
        }
    }
}
