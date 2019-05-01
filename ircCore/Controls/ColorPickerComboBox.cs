/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ircCore.Controls
{
    /* Original code: Jonathan Wood © 2011
     * http://www.blackbeltcoder.com/Articles/controls/creating-a-color-picker-with-an-owner-draw-combobox
     */
    public class ColorInfo
    {
        public string Text { get; set; }
        public Color Color { get; set; }

        public ColorInfo(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }

    public sealed class ColorPickerComboBox : ComboBox
    {        
        public new ColorInfo SelectedItem
        {
            get
            {
                return (ColorInfo)base.SelectedItem;
            }
            set
            {
                base.SelectedItem = value;
            }
        }

        public new string SelectedText
        {
            get
            {
                return SelectedIndex >= 0 ? SelectedItem.Text : string.Empty;
            }
            set
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    if (((ColorInfo) Items[i]).Text != value)
                    {
                        continue;
                    }
                    SelectedIndex = i;
                    break;
                }
            }
        }

        public ColorPickerComboBox()
        {
            Font = new Font("Segoe UI", 9, FontStyle.Regular);           
            ItemHeight = 20;
            MinimumSize = new Size(140, 26);
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            DropDownHeight = 106;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            /* Init color items */
            var colorType = typeof(Color);
            var propInfoList = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (var c in propInfoList.Where(c => !c.Name.Equals("Transparent", StringComparison.InvariantCultureIgnoreCase)))
            {
                Items.Add(new ColorInfo(c.Name, Color.FromName(c.Name)));
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                var color = (ColorInfo) Items[e.Index];
                e.DrawBackground();
                /* Draw color box */
                var rect = new Rectangle
                               {X = e.Bounds.X + 2, Y = e.Bounds.Y + 2, Width = 18, Height = e.Bounds.Height - 5};
                using (var c = new SolidBrush(color.Color))
                {
                    e.Graphics.FillRectangle(c, rect);
                }
                e.Graphics.DrawRectangle(SystemPens.WindowText, rect);
                /* Draw color's name */
                var brush = (e.State & DrawItemState.Selected) != DrawItemState.None
                                ? SystemBrushes.HighlightText
                                : SystemBrushes.WindowText;
                e.Graphics.DrawString(color.Text, Font, brush,
                                      e.Bounds.X + rect.X + rect.Width + 2,
                                      e.Bounds.Y + ((e.Bounds.Height - Font.Height)/2));
                /* Draw the focus rectangle if appropriate */
                if ((e.State & DrawItemState.NoFocusRect) == DrawItemState.None)
                {
                    e.DrawFocusRectangle();
                }
            }
            base.OnDrawItem(e);
        }
    }
}
