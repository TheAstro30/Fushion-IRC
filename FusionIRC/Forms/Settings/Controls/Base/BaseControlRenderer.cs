/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;

namespace FusionIRC.Forms.Settings.Controls.Base
{
    public class BaseControlRenderer : UserControl
    {
        public string Header { get; set; }

        public BaseControlRenderer()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            /* Draw the header */
            using (var p = new Pen(Color.FromArgb(190, 190, 190)))
            {
                e.Graphics.DrawRectangle(p, 1, 1, ClientRectangle.Width - 2, 30);
                using (var b = new SolidBrush(Color.FromArgb(220, 220, 220)))
                {
                    e.Graphics.FillRectangle(b, 2, 2, ClientRectangle.Width - 3, 29);
                }
            }
            using (var b = new SolidBrush(Color.Black))
            {
                e.Graphics.DrawString(Header, new Font("Segoe UI", 12, FontStyle.Bold), b, 4, 5);
            }
            base.OnPaint(e);
        }

        private void InitializeComponent()
        {
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MaximumSize = new Size(427, 347);
            MinimumSize = new Size(427, 347);
            Size = new Size(427, 347);
        }
    }
}
