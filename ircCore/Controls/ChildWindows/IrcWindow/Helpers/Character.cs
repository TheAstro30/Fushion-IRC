/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using ircCore.Controls.ChildWindows.IrcWindow.Structures;

namespace ircCore.Controls.ChildWindows.IrcWindow.Helpers
{
    /* Class for get character/word under mouse for copy selection */
    internal static class Character
    {
        internal static int GetLineNumber(TextData textData, int y, int windowHeight, int scrollValue, int fontHeight, double lineSpacing, int padding, out int lineIndex, out int wrappedIndex, out int startY)
        {
            /* We calculate the Y position of the mouse to what line is actually under the mouse at that point */
            var currentLine = windowHeight;            
            var line = textData.WrappedLinesCount - 1;
            for (var lineCount = textData.Wrapped.Count - 1; lineCount >= 0; lineCount--)
            {
                /* We already know the scroll value is the last visible line, which is why we work backwards */
                var lineHeight = (int)(fontHeight * lineSpacing); /* Make sure this is always reset on each iteration */
                for (var lineWrapped = textData.Wrapped[lineCount].Lines.Count - 1; lineWrapped >= 0; lineWrapped--)
                {
                    if (line > scrollValue)
                    {
                        /* Line isn't visible */
                        line--;
                        continue;
                    }
                    if (lineWrapped == 0)
                    {
                        /* We need to take in to account padding at the start of a new wrapped line */
                        lineHeight += padding;                        
                    }                    
                    if (y >= currentLine - lineHeight && y <= currentLine)
                    {
                        /* We have a line */
                        lineIndex = lineCount;
                        wrappedIndex = lineWrapped;
                        startY = currentLine - fontHeight -1;
                        return line;
                    }
                    currentLine -= lineHeight;
                    line--;
                }
            }
            lineIndex = -1;
            wrappedIndex = -1;
            startY = -1;
            return line;
        }

        internal static char ReturnChar(Graphics deviceContext, WrapData.WrapLineData wrapData, int x, bool indented, int indentWidth, Font font, out int position, out int startX, ref bool bolding, ref bool underLining, ref bool italics)
        {
            /* Return the character that is under the mouse pointer at position X for line number Y */
            var ch = 0;
            var charWidth = indented ? indentWidth : 0;
            var width = charWidth;
            var textLen = wrapData.Text.Length - 1;
            startX = 0;
            /* Set font formatting */
            var bold = wrapData.IsBold;
            var underLine = wrapData.IsUnderline;
            var italic = wrapData.IsItalic;
            font = Functions.SetFont(font, bold, underLine, italic);            
            while (ch <= textLen)
            {
                int index;
                var controlBytes = wrapData.GetControlBytesAtPosition(ch, out index);
                if (controlBytes.Count > 0)
                {
                    foreach (var controlByte in controlBytes)
                    {
                        switch (controlByte)
                        {
                            case ControlByte.Bold:
                                bold = !bold;
                                font = Functions.SetFont(font, bold, underLine, italic);
                                break;

                            case ControlByte.Italic:
                                italic = !italic;
                                font = Functions.SetFont(font, bold, underLine, italic);
                                break;

                            case ControlByte.Underline:
                                underLine = !underLine;
                                font = Functions.SetFont(font, bold, underLine, italic);
                                break;

                            case ControlByte.Normal:
                                bold = false;
                                italic = false;
                                underLine = false;
                                font = Functions.SetFont(font, false, false, false);
                                break;
                        }
                    }
                }                
                var c = wrapData.Text[ch];
                var x2 = TextMeasurement.MeasureStringWidth(deviceContext, font, c);
                charWidth += x2;                
                startX = width;
                if (x >= width  && x <= width + x2)
                {
                    position = ch;
                    bolding = bold;
                    underLining = underLine;
                    italics = italic;                    
                    return c;
                }
                width += x2;
                ch++;
                if (ch <= textLen || x <= width) { continue; }                
                /* Selection was started outside the right-hand side of the text (assume backwards) */
                startX = 0;
                position = textLen;
                bolding = false;
                underLining = false;
                italics = false;
                return (char) 0;
            }
            position = -1;            
            return (char)0;
        }

        internal static string ReturnWord(Graphics deviceContext, WrapData wrapData, int wrapIndex, int startPosition)
        {
            /* We search backwards and forwards in the current line for a space either side of our current startPosition */
            var startSpace = wrapData.Lines[wrapIndex].Text.LastIndexOf(' ', startPosition);
            var endSpace = wrapData.Lines[wrapIndex].Text.IndexOf(' ', startPosition);
            var s = wrapData.Lines[wrapIndex].Text;
            if (startSpace == -1)
            {
                /* Start of line, check for broken at start */
                startSpace = 0;
                if (wrapData.Lines[wrapIndex].BrokenAtStart)
                {
                    for (var start = wrapIndex - 1; start >= 0; start--)
                    {
                        s = string.Format("{0}{1}", wrapData.Lines[start].Text, s);
                        startPosition = (wrapData.Lines[start].Text.Length + startPosition) - 1;
                        if (!wrapData.Lines[start].BrokenAtStart)
                        {
                            break;
                        }
                    }                    
                    startSpace = s.LastIndexOf(' ', startPosition);
                    endSpace = s.IndexOf(' ', startPosition);
                    if (startSpace == -1)
                    {
                        startSpace = 0;
                    }
                }
            }
            if (endSpace == -1)
            {                
                /* No space after this current word so it's either end of line or broken over to next line (word cut in half) */
                if (wrapData.Lines[wrapIndex].BrokenAtEnd)
                {
                    for (var end = wrapIndex + 1; end <= wrapData.Lines.Count - 1; end++)
                    {
                        s = string.Format("{0}{1}", s, wrapData.Lines[end].Text);                        
                        if (!wrapData.Lines[end].BrokenAtEnd)
                        {
                            break;
                        }
                    }
                    startSpace = s.LastIndexOf(' ', startPosition);
                    endSpace = s.IndexOf(' ', startPosition);
                    if (startSpace == -1)
                    {
                        startSpace = 0;
                    }
                    if (endSpace == -1)
                    {
                        endSpace = s.Length - 1;
                    }             
                }
                else
                {
                    endSpace = s.Length - 1;
                }
            }
            /* Now we should have our word ;) */
            return s.Substring(startSpace, (endSpace - startSpace) + 1).Trim();            
        }
    }
}