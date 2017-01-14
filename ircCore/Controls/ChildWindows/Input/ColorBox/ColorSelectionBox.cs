/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ircCore.Controls.ChildWindows.Input.ColorBox
{
    public class ColorSelectionBox : Control
    {
        /* Color selection control
           By: Jason James Newland
           ©2011 - KangaSoft Software
           All Rights Reserved
         */
        private readonly Color[] _boxColor = new Color[16];

        private bool _showFocus = true;
        private int _selected;

        public ColorSelectionBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        public void SetBoxColor(int index, Color c)
        {
            _boxColor[index] = c;
            Refresh();
        }

        public bool ShowFocusRectangle
        {
            get { return _showFocus; }
            set { _showFocus = value; }
        }

        public int SelectedColor
        {
            get { return _selected; }
            set
            {
                _selected = value;
                Refresh();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            Height = 22;
            Width = (21 * 16) + 1;
            base.OnResize(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            /* Detect which box was clicked on */
            if (e.Y >= 1 && e.Y <= 21)
            {
                for (var i = 0; i < 16; i++)
                {
                    if (i == 0)
                    {
                        if (e.X >= 2 && e.X <= 21)
                        {
                            _selected = 0;
                            break;
                        }
                    }
                    else
                    {
                        if (e.X >= ((21 * (i + 1)) - 16) && e.X <= (21 * (i + 1)))
                        {
                            _selected = i;
                            break;
                        }
                    }
                }
                Refresh();
            }
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var r = new Rectangle(1, 1, 18, 18);
            var fr = new Rectangle(0, 0, 21, 21);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            for (var i = 0; i < 16; i++)
            {
                if (i > 0) { r.X = (21 * i) + 2; }
                else { r.X = 2; }
                r.Y = 1;
                using (var b = new SolidBrush(_boxColor[i]))
                {
                    e.Graphics.FillRectangle(b, r);
                }
                e.Graphics.DrawRectangle(Pens.Black, r);
                r.X -= 1;
                r.Y += 2;
                TextRenderer.DrawText(e.Graphics, string.Format("{0:00}", i), base.Font, r, Color.Black, TextFormatFlags.Left);
                if (!_showFocus || i != _selected) { continue; }
                /* Focus rectangle */
                fr.X = r.X;
                ControlPaint.DrawFocusRectangle(e.Graphics, fr);
            }
        }
    }
}
