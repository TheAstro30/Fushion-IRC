/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;

namespace ircCore.Controls.Rendering
{
    public class CustomRenderer : ToolStripProfessionalRenderer
    {
        public CustomRenderer()
        {
            /* Default */
        }

        public CustomRenderer(ProfessionalColorTable table) : base (table)
        {
            /* Custom color table */
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item is ToolStripStatusLabel)
                TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont,
                    e.TextRectangle, e.TextColor, Color.Transparent,
                    e.TextFormat | TextFormatFlags.EndEllipsis);
            else
                base.OnRenderItemText(e);
        }
    }
}
