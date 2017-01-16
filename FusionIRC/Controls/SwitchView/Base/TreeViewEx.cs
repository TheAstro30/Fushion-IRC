/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FusionIRC.Controls.SwitchView.Base
{
    public class TreeViewEx : TreeView
    {
        /* IRC Treeview Control - Base TreeView Extension Class
           By: Jason James Newland
           ©2010-2012 - KangaSoft Software
           All Rights Reserved
         */
        public TreeViewEx()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            if (e.Node == e.Node.TreeView.SelectedNode)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, SystemColors.HighlightText, SystemColors.Highlight);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, SystemColors.HighlightText,
                                      TextFormatFlags.GlyphOverhangPadding);
            }
            else
            {
                var fore = e.Node.ForeColor;
                if (fore == Color.Empty) { fore = e.Node.TreeView.ForeColor; }
                e.Graphics.FillRectangle(new SolidBrush(e.Node.TreeView.BackColor), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, fore,
                                      TextFormatFlags.GlyphOverhangPadding);
            }
        }

        protected override void WndProc(ref Message mMsg)
        {
            if (mMsg.Msg == Convert.ToInt32(0x14))
            {
                /* If message is erase background reset message to null (reduce flicker) */
                mMsg.Msg = Convert.ToInt32(0x0);
            }
            base.WndProc(ref mMsg);
        }
    }
}
