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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Drawing.Drawing2D;
using libolv.Implementation;
using libolv.Implementation.TreeView;
using libolv.Rendering.Renderers;

namespace libolv
{
    public partial class TreeListView
    {
        public class TreeRenderer : HighlightTextRenderer
        {
            private bool _isShowLines = true;

            public static int PixelsPerLevel = 16 + 1;

            public TreeRenderer()
            {
                LinePen = new Pen(
                    Color.Blue, 1.0f)
                              {
                                  DashStyle = DashStyle.Dot
                              };
            }

            private Branch Branch
            {
                get { return TreeListView.TreeModel.GetBranch(RowObject); }
            }

            public Pen LinePen { get; set; }

            public TreeListView TreeListView
            {
                get { return (TreeListView)ListView; }
            }

            public bool IsShowLines
            {
                get { return _isShowLines; }
                set { _isShowLines = value; }
            }

            public override void Render(Graphics g, Rectangle r)
            {
                DrawBackground(g, r);
                var br = Branch;
                var paddedRectangle = ApplyCellPadding(r);
                var expandGlyphRectangle = paddedRectangle;
                expandGlyphRectangle.Offset((br.Level - 1)*PixelsPerLevel, 0);
                expandGlyphRectangle.Width = PixelsPerLevel;
                expandGlyphRectangle.Height = PixelsPerLevel;
                expandGlyphRectangle.Y = AlignVertically(paddedRectangle, expandGlyphRectangle);
                var expandGlyphRectangleMidVertical = expandGlyphRectangle.Y + (expandGlyphRectangle.Height/2);
                if (IsShowLines)
                {
                    DrawLines(g, r, LinePen, br, expandGlyphRectangleMidVertical);
                }
                if (br.CanExpand)
                {
                    DrawExpansionGlyph(g, expandGlyphRectangle, br.IsExpanded);
                }
                var indent = br.Level*PixelsPerLevel;
                paddedRectangle.Offset(indent, 0);
                paddedRectangle.Width -= indent;
                DrawImageAndText(g, paddedRectangle);
            }

            protected virtual void DrawExpansionGlyph(Graphics g, Rectangle r, bool isExpanded)
            {
                if (UseStyles)
                {
                    DrawExpansionGlyphStyled(g, r, isExpanded);
                }
                else
                {
                    DrawExpansionGlyphManual(g, r, isExpanded);
                }
            }

            protected virtual bool UseStyles
            {
                get { return !IsPrinting && Application.RenderWithVisualStyles; }
            }

            protected virtual void DrawExpansionGlyphStyled(Graphics g, Rectangle r, bool isExpanded)
            {
                var element = VisualStyleElement.TreeView.Glyph.Closed;
                if (isExpanded)
                {
                    element = VisualStyleElement.TreeView.Glyph.Opened;
                }
                var renderer = new VisualStyleRenderer(element);
                renderer.DrawBackground(g, r);
            }

            protected virtual void DrawExpansionGlyphManual(Graphics g, Rectangle r, bool isExpanded)
            {
                const int h = 8;
                const int w = 8;
                var x = r.X + 4;
                var y = r.Y + (r.Height/2) - 4;
                g.DrawRectangle(new Pen(SystemBrushes.ControlDark), x, y, w, h);
                g.FillRectangle(Brushes.White, x + 1, y + 1, w - 1, h - 1);
                g.DrawLine(Pens.Black, x + 2, y + 4, x + w - 2, y + 4);
                if (!isExpanded)
                {
                    g.DrawLine(Pens.Black, x + 4, y + 2, x + 4, y + h - 2);
                }
            }

            protected virtual void DrawLines(Graphics g, Rectangle r, Pen p, Branch br, int glyphMidVertical)
            {
                var r2 = r;
                r2.Width = PixelsPerLevel;
                /* Vertical lines have to start on even points, otherwise the dotted line looks wrong.
                 * This is only needed if pen is dotted. */
                var top = r2.Top;
                /* Draw lines for ancestors */
                int midX;
                var ancestors = br.Ancestors;
                foreach (var ancestor in ancestors)
                {
                    if (!ancestor.IsLastChild && !ancestor.IsOnlyBranch)
                    {
                        midX = r2.Left + r2.Width/2;
                        g.DrawLine(p, midX, top, midX, r2.Bottom);
                    }
                    r2.Offset(PixelsPerLevel, 0);
                }
                /* Draw lines for this branch */
                midX = r2.Left + r2.Width/2;
                /* Horizontal line first */
                g.DrawLine(p, midX, glyphMidVertical, r2.Right, glyphMidVertical);
                /* Vertical line second */
                if (br.IsFirstBranch)
                {
                    if (!br.IsLastChild && !br.IsOnlyBranch)
                    {
                        g.DrawLine(p, midX, glyphMidVertical, midX, r2.Bottom);
                    }
                }
                else
                {
                    g.DrawLine(p, midX, top, midX, br.IsLastChild ? glyphMidVertical : r2.Bottom);
                }
            }

            protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
            {
                var br = Branch;
                var r = Bounds;
                if (br.CanExpand)
                {
                    r.Offset((br.Level - 1)*PixelsPerLevel, 0);
                    r.Width = PixelsPerLevel;
                    if (r.Contains(x, y))
                    {
                        hti.HitTestLocation = HitTestLocation.ExpandButton;
                        return;
                    }
                }
                r = Bounds;
                var indent = br.Level*PixelsPerLevel;
                r.X += indent;
                r.Width -= indent;
                /* Ignore events in the indent zone */
                if (x < r.Left)
                {
                    hti.HitTestLocation = HitTestLocation.Nothing;
                }
                else
                {
                    StandardHitTest(g, hti, r, x, y);
                }
            }

            protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OlvListItem item, int subItemIndex, Size preferredSize)
            {
                return StandardGetEditRectangle(g, cellBounds, preferredSize);
            }
        }
    }
}