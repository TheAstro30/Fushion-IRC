/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;

namespace ircCore.Controls
{
    public class FormEx : Form
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            /* Draws a background color similar to Windows' Task panes */
            var rect = new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height - 40);
            using (var b = new SolidBrush(Color.White))
            {
                e.Graphics.FillRectangle(b, rect);
                using (var p = new Pen(b))
                {
                    e.Graphics.DrawRectangle(p, rect);
                }
            }
            using (var b = new SolidBrush(Color.LightGray))
            {
                using (var p = new Pen(b))
                {
                    e.Graphics.DrawLine(p, 0, rect.Bottom, rect.Width, rect.Bottom);
                }
            }
        }
    }
}