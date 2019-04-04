/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using ircCore.Controls.ChildWindows.Helpers;
using ircCore.Controls.ChildWindows.OutputDisplay.Structures;

namespace ircCore.Controls.ChildWindows.OutputDisplay.Helpers
{
    /* The main word wrap function for the window; this will wrap the text to the window width filling in the WrapData structure */
    internal static class Wrapping
    {
        internal static void WordWrap(Graphics deviceContext, Color defaultColor, Color backColor, bool wordWrap, int indentWidth, string text, int width, Font font, out WrapData data)
        {            
            data = new WrapData();
            /* We have to go via the text character by character, look for control byte codes, calculate the position in the string it starts and
             * remove 1 character (colour codes the length of the codes) [strip basically] so we know where in the final wrapped line it is ... */
            var foreCol = Color.Empty;
            var backCol = Color.Empty;
            var tmpBackCol = Color.Empty;
            var tmpForeCol = Color.Empty;
            var breakPos = 0;
            var currentWidth = 0;            
            var length = text.Length - 1;
            int i;
            var bold = false;
            var underLine = false;
            var italic = false;
            var reverse = false;
            var wld = new WrapData.WrapLineData();            
            var firstLineParsed = false;
            /* This method, all though refactored, is still in no way the best way to accomplish wrapping */
            for (i = 0; i <= length; i++)
            { 
                var c = text[i];
                switch (c)
                {
                    case (char)ControlByte.Bold:
                    case (char)ControlByte.Underline:
                    case (char)ControlByte.Italic:                        
                        switch (c)
                        {
                            case (char)ControlByte.Bold:
                                bold = !bold;                                
                                break;

                            case (char)ControlByte.Underline:
                                underLine = !underLine;                                
                                break;

                            case (char)ControlByte.Italic:
                                italic = !italic;                                
                                break;
                        }
                        wld.ControlBytes.Add(CreateNewControlByte((ControlByte)c, i, foreCol, backCol, currentWidth));
                        font = Functions.SetFont(font, bold, underLine, italic);
                        text = string.Format("{0}{1}", text.Substring(0, i), text.Substring(i + 1));
                        length--;                        
                        i--;
                        break;

                    case (char)ControlByte.Color:
                        /* When parsing colours, we need to check the character byte isn't on it's own, if it is colour formatting is removed */                        
                        var iTmp = 0;
                        Functions.ParseColorCodes(text, i, length, ref foreCol, ref backCol, ref iTmp);                      
                        wld.ControlBytes.Add(CreateNewControlByte(ControlByte.Color, i, foreCol, backCol, currentWidth, text.Substring(i, iTmp + 1)));
                        text = string.Format("{0}{1}", text.Substring(0, i), text.Substring(i + iTmp + 1));
                        length -= iTmp + 1;
                        i--;
                        break;

                    case (char)ControlByte.Normal:
                        wld.ControlBytes.Add(CreateNewControlByte(ControlByte.Normal, i, Color.Empty, Color.Empty, currentWidth));
                        bold = false;
                        underLine = false;
                        italic = false;
                        reverse = false;
                        font = Functions.SetFont(font, false, false, false);
                        text = string.Format("{0}{1}", text.Substring(0, i), text.Substring(i + 1));                        
                        length--;
                        i--;
                        break;

                    case (char)ControlByte.Reverse:
                        /* Reverse swaps the fore and back colour's around of the text */                        
                        reverse = !reverse;
                        if (reverse)
                        {
                            tmpBackCol = backCol;
                            tmpForeCol = foreCol;
                            backCol = defaultColor;
                            foreCol = backColor;
                        }
                        else
                        {
                            backCol = tmpBackCol;
                            foreCol = tmpForeCol;
                        }
                        wld.ControlBytes.Add(CreateNewControlByte(ControlByte.Reverse, i, foreCol, backCol, currentWidth));
                        text = string.Format("{0}{1}", text.Substring(0, i), text.Substring(i + 1));                        
                        length--;
                        i--;
                        break;

                    case ' ':                        
                        breakPos = i;
                        currentWidth += TextMeasurement.MeasureStringWidth(deviceContext, font, c);                        
                        break;

                    default:                        
                        currentWidth += TextMeasurement.MeasureStringWidth(deviceContext, font, c);
                        break;
                }
                if (!wordWrap || currentWidth + (firstLineParsed ? indentWidth : 0) <= width)
                {
                    /* Either word wrapping is false or the current width hasn't exceeded the window width yet */
                    continue;
                }
                /* If the current known break position is a little less than the right edge, break by word - otherwise it will cut the word in half */
                if (breakPos > i - (i/3))
                {
                    /* Break by word */
                    i = breakPos;
                }
                else
                {
                    /* Cut word */
                    wld.BrokenAtEnd = true;
                }
                if (wld.ControlBytes.Count > 0)
                {
                    /* Trim off any control code data that is beyond the break point */
                    while (wld.ControlBytes.Count > 0 && wld.ControlBytes[wld.ControlBytes.Count - 1].Position > i)
                    {
                        /* Turn reverse and bolding off - bolding is important or the positioning gets mucked up (severely) */
                        bold = false;
                        if (reverse)
                        {
                            reverse = false;
                            backCol = tmpBackCol;
                            foreCol = tmpForeCol;
                        }
                        /* Prepend left over codes back to the string after the break point */
                        var count = wld.ControlBytes.Count - 1;
                        var tmpData = wld.ControlBytes[count];
                        text = string.Format("{0}{1}{2}{3}", text.Substring(0, tmpData.Position),
                                             (char) tmpData.ControlByte,
                                             tmpData.ControlByte == ControlByte.Color ? tmpData.OriginalColor : "",
                                             text.Substring(tmpData.Position));
                        wld.ControlBytes.RemoveAt(count);
                    }
                }
                wld.Text = text.Substring(0, i);
                data.Lines.Add(wld);
                /* If the word was cut in half, the next new line must also be broken at the start ;) - this helps for links */
                var broken = wld.BrokenAtEnd;
                wld = new WrapData.WrapLineData
                          {
                              BrokenAtStart = broken,
                              IsBold = bold,
                              IsUnderline = underLine,
                              IsItalic = italic
                          };
                text = text.Substring(i).TrimStart();
                length = text.Length - 1;
                breakPos = 0;
                currentWidth = 0;
                i = -1;
                firstLineParsed = true;
            }
            /* Add what's left over */
            if (text.Length == 0) { return; }            
            wld.Text = text;
            data.Lines.Add(wld);
        }

        /* Private methods */
        private static ControlByteData CreateNewControlByte(ControlByte controlByte, int position, Color foreColor, Color backColor, int currentMeasurement, string originalColor = "")
        {
            return new ControlByteData
                       {
                           ControlByte = controlByte,
                           Position = position,
                           OriginalColor = originalColor,
                           ForeColor = foreColor,
                           BackColor = backColor,                           
                           CurrentMeasurement = currentMeasurement
                       };
        }
    }    
}
