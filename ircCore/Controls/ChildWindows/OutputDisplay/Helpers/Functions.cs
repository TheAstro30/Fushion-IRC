/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ircCore.Controls.ChildWindows.Helpers;
using ircCore.Settings.Theming;

namespace ircCore.Controls.ChildWindows.OutputDisplay.Helpers
{
    internal static class Functions
    {        
        private static readonly Regex RegExColors = new Regex("(?:(\\d{1,2})?(?:,(\\d{1,2}))?)", RegexOptions.Compiled);

        internal const uint SrcCopy = 0x00CC0020;

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BitBlt(IntPtr hdc, int nXDest,
                                          int nYDest, int nWidth,
                                          int nHeight, IntPtr hdcSrc,
                                          int nXSrc, int nYSrc, uint dwRop);

        internal static Font SetFont(Font font, bool bold, bool underLine, bool italic)
        {
            /* Sets the font object to the current formatting of the line being drawn/measured */
            var fs = default(FontStyle);
            if (!bold && !underLine && !italic)
            {
                fs = FontStyle.Regular; 
            }
            else
            {
                if (bold) { fs = fs | FontStyle.Bold; }
                if (underLine) { fs = fs | FontStyle.Underline; }
                if (italic) { fs = fs | FontStyle.Italic; }
            }
            return new Font(font, fs);
        }

        internal static void ParseColorCodes(string text, int index, int length, ref Color foreColor, ref Color backColor, ref int charactersFound)
        {
            if (index + 1 > length || text.Substring(index + 1, 1) == " ")
            {                
                return;
            }
            foreColor = Color.Empty;
            backColor = Color.Empty;
            /* Look at next byte to make sure it is not a control code */
            switch (text[index + 1])
            {
                case (char)ControlByte.Bold:
                case (char)ControlByte.Underline:
                case (char)ControlByte.Reverse:
                case (char)ControlByte.Normal:
                case (char)ControlByte.Italic:
                case (char)ControlByte.Color:
                    return;
            }
            if (index + 6 <= length)
            {
                /* We do 5 characters forward */
                ParseColors(text.Substring(index + 1, 5), ref charactersFound, ref foreColor, ref backColor);
            }
            else
            {
                if (length - index != 0)
                {
                    /* We do to total length of string left */
                    ParseColors(text.Substring(index + 1, length - index), ref charactersFound, ref foreColor, ref backColor);
                }
            }
        }

        private static void ParseColors(string s, ref int nextPos, ref Color foreColor, ref Color backColor)
        {
            var m = RegExColors.Matches(s);
            if (m[0].Length == 0)
            {
                backColor = Color.Empty; /* Kind of important to do this... */
                return;
            }
            var col = m[0].Value.Split(',');
            int fore;
            Int32.TryParse(col[0], out fore);
            var back = -1;
            if (col.Length > 1) { Int32.TryParse(col[1], out back); }
            /* Make sure colors are within range */
            if (fore > 15) { fore %= 15; }
            if (back > 15) { back %= 15; }
            /* Set our colors */
            if (fore > -1) { foreColor = ThemeManager.CurrentTheme.Colors[fore]; }
            if (back > -1) { backColor = ThemeManager.CurrentTheme.Colors[back]; }
            /* Increase character position pointer */
            nextPos += m[0].Length;
        }
    }
}
