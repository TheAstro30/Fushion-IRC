/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;

namespace ircCore.Controls.ChildWindows.IrcWindow.Structures
{
    internal class MarkingData : IDisposable
    {
        /* This class holds all the necessary data for line marking copy selection */
        public Bitmap MarkBitmap { get; set; }

        public bool IsBold;
        public bool IsItalic;
        public bool IsUnderline;

        public bool MouseStartedMoving { get; set; }

        public bool MarkReverse { get; set; }

        public int MarkStartLine { get; set; }
        public int MarkEndLine { get; set; }
        public int MarkBackwardStart { get; set; }
        public int MarkStartXPos { get; set; }
        public int MarkEndXPos { get; set; }
        public int MarkOriginalXPos { get; set; }
        public int MarkStartCharPos { get; set; }
        public int MarkEndCharPos { get; set; }

        public void Dispose()
        {
            /* GDI Clean up */
            MarkBitmap.Dispose();
            MarkBitmap = null;
        }
    }
}