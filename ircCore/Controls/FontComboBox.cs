/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ircCore.Properties;

namespace ircCore.Controls
{
    public sealed class FontComboBox : ComboBox
    {
        /* Font combo box control
           By: Jason James Newland
           ©2012 - KangaSoft Software
           All Rights Reserved
         */
        internal struct FontNames
        {
            public FontFamily Family;

            public override string ToString()
            {
                return Family.Name;
            }
        }

        private readonly Image _ttImg = Resources.ttfIcon;

        public FontComboBox()
        {
            MaxDropDownItems = 20;
            IntegralHeight = false;
            Sorted = false;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            /* Populate combo box with current TTF fonts */
            foreach (var fn in from ff in FontFamily.Families where ff.IsStyleAvailable(FontStyle.Regular)
                               select new FontNames { Family = ff })
            {
                Items.Add(fn);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index == -1) { return; }
            var fontName = Items[e.Index].ToString();
            /* Create a back-buffer for all drawing - this overcomes text overdraw when scrolling */
            using (var drawbuffer = new Bitmap(e.Bounds.Width, e.Bounds.Height))
            {
                using (var grfx = Graphics.FromImage(drawbuffer))
                {                    
                    var rectIcon = new Rectangle();
                    var rectText = new Rectangle();
                    /* Set icon and text rectangle widths */
                    rectIcon.Height = rectIcon.Width = e.Bounds.Height;
                    rectText.X += rectIcon.Width;
                    rectText.Width = e.Bounds.Width - e.Bounds.Height;                                        
                    if ((e.State & DrawItemState.Focus) == 0)
                    {
                        /* Normal state */
                        using (var bbrush = new SolidBrush(SystemColors.Window))
                        {
                            grfx.FillRectangle(bbrush, 0, 0, e.Bounds.Width, e.Bounds.Height);
                        }
                        using (var brush = new SolidBrush(SystemColors.WindowText))
                        {
                            grfx.DrawString(fontName, Font, brush, rectText);
                        }
                    }
                    else
                    {
                        /* Selected/hi-light state */
                        using (var bbrush = new SolidBrush(SystemColors.Highlight))
                        {
                            grfx.FillRectangle(bbrush, 0, 0, e.Bounds.Width, e.Bounds.Height);
                        }
                        using (var brush = new SolidBrush(SystemColors.HighlightText))
                        {
                            grfx.DrawString(fontName, Font, brush, rectText);
                        }
                    }
                    /* Draw TTF icon */
                    grfx.DrawImage(_ttImg, rectIcon);                    
                }
                /* Draw out back-buffer */
                e.Graphics.DrawImageUnscaled(drawbuffer, e.Bounds);
            }
        }
    }
}

