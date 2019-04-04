/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.Helpers;
using ircCore.Controls.ChildWindows.OutputDisplay.Structures;

namespace ircCore.Controls.ChildWindows.OutputDisplay.Helpers
{
    /* Drawing functions for the output window */
    public enum BackgroundImageLayoutStyles
    {
        None = 0,
        Tile = 1,
        Center = 2,
        Stretch = 3,
        Zoom = 4,
        Photo = 5
    }

    public enum LineSpacingStyle
    {
        Single = 0,
        Double = 1,
        Paragraph = 2
    }

    public class OutputRenderer : UserControl
    {
        /* This class draws the output graphics and keeps track of lines drawn */
        private const TextFormatFlags FormatFlags = TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;

        private LineSpacingStyle _lineSpacingStyle;
        private Bitmap _bgImage;
        private BackgroundImageLayoutStyles _bgLayout = BackgroundImageLayoutStyles.None;
        private float _bgAspect;

        /* Properties */
        public TextData TextData { get; set; }

        public new Bitmap BackgroundImage
        {
            get { return _bgImage; }
            set
            {
                _bgImage = value;
                if (_bgImage != null)
                {
                    _bgAspect = (float)_bgImage.Width / _bgImage.Height;
                }
                else
                {
                    base.BackgroundImage = null;
                    base.BackgroundImageLayout = ImageLayout.None;
                    _bgLayout = BackgroundImageLayoutStyles.None;
                }
            }
        }

        public new BackgroundImageLayoutStyles BackgroundImageLayout
        {
            get { return _bgLayout; }
            set
            {
                /* Tile, center & strecth mode are handled by the control base */
                _bgLayout = value;
                switch (_bgLayout)
                {
                    case BackgroundImageLayoutStyles.None:
                    case BackgroundImageLayoutStyles.Zoom:
                    case BackgroundImageLayoutStyles.Photo:
                        base.BackgroundImage = null;
                        base.BackgroundImageLayout = ImageLayout.None;
                        break;

                    case BackgroundImageLayoutStyles.Center:
                        base.BackgroundImage = _bgImage;
                        base.BackgroundImageLayout = ImageLayout.Center;
                        break;

                    case BackgroundImageLayoutStyles.Stretch:
                        base.BackgroundImage = _bgImage;
                        base.BackgroundImageLayout = ImageLayout.Stretch;
                        break;

                    case BackgroundImageLayoutStyles.Tile:
                        base.BackgroundImage = _bgImage;
                        base.BackgroundImageLayout = ImageLayout.Tile;
                        break;
                }
            }
        }               

        public LineSpacingStyle LineSpacingStyle
        {
            get { return _lineSpacingStyle; }
            set
            {
                _lineSpacingStyle = value;
                switch (_lineSpacingStyle)
                {
                    case LineSpacingStyle.Single:
                        LineSpacing = 1;
                        LinePadding = 0;
                        break;

                    case LineSpacingStyle.Double:
                        LineSpacing = 2;
                        LinePadding = 0;
                        break;

                    case LineSpacingStyle.Paragraph:
                        LineSpacing = 1;
                        LinePadding = 3;
                        break;
                }                
            }
        }

        public int IndentWidth { get; set; }

        /* These two properties replace a single line, single character "-" line break with a drawn line */
        public bool ShowLineMarker { get; set; }
        public Color LineMarkerColor { get; set; }

        /* Internal properties */
        internal int LineSpacing { get; set; }
        internal int LinePadding { get; set; }

        internal MarkingData MarkingData { get; set; }

        internal int TextHeight { get; set; }

        /* Rendering */
        internal void Render(Graphics deviceContext, Font font, Rectangle clientRect, int scrollValue)
        {
            /* Check for marking */
            if (MarkingData != null && MarkingData.MouseStartedMoving)
            {                
                RenderMarkedText(deviceContext, font, clientRect, scrollValue);
            }
            else
            {             
                /* Render normal line data */
                RenderBackgroundImage(deviceContext, clientRect);
                RenderLineData(deviceContext, font, clientRect, scrollValue);
            }
        }

        /* Private rendering methods */
        private void RenderBackgroundImage(Graphics deviceContext, Rectangle clientRect)
        {
            /* Background image is drawn first before the text */
            if (BackgroundImage == null || (BackgroundImageLayout != BackgroundImageLayoutStyles.Zoom && BackgroundImageLayout != BackgroundImageLayoutStyles.Photo))
            {                
                /* Nothing to do here - as stretch, title and center image modes are handled directly by the control, no point proceeding */
                return; 
            }
            int width;
            int height;
            switch (BackgroundImageLayout)
            {
                case BackgroundImageLayoutStyles.Zoom:
                    /* Keeps its aspect */
                    float xw;
                    float yw;
                    if (_bgImage.Height < clientRect.Height)
                    {
                        /* Zoom by height */
                        yw = clientRect.Height;
                        xw = yw * _bgAspect;
                    }
                    else
                    {
                        /* Zoom by width */
                        yw = clientRect.Width;
                        xw = yw * _bgAspect;
                    }
                    width = clientRect.Width / 2;
                    height = clientRect.Height / 2;
                    deviceContext.DrawImage(_bgImage, new RectangleF(width - (xw / 2), height - (yw / 2), xw, yw));
                    break;

                case BackgroundImageLayoutStyles.Photo:
                    var c = _bgImage.Width;
                    var d = _bgImage.Height;
                    if (d > 500)
                    {
                        width = c / 3;
                        height = d / 3;
                    }
                    else if (d > 135)
                    {
                        width = c / 2;
                        height = d / 2;
                    }
                    else
                    {
                        width = c;
                        height = d;
                    }
                    deviceContext.DrawImage(_bgImage, new Rectangle(clientRect.Width - width, 0, width, height));
                    break;
            }            
        }

        private void RenderLineData(Graphics deviceContext, Font font, Rectangle clientRect, int scrollValue)
        {
            /* When outputting each line, we check if there are any control byte codes in the current line, otherwise we just draw out straight */
            var rect = new Rectangle(0, clientRect.Top - font.Height, clientRect.Width, (clientRect.Bottom + LinePadding) - 1);
            rect.Height += LineSpacing > 1 ? TextHeight / LineSpacing : 0; /* Shifts the text down when line spacing is used */
            var rectBottom = rect.Height;
            var linePosition = TextData.Lines.Count - 1;            
            var totalLines = TextData.WrappedLinesCount - 1;
            int line;
            for (line = TextData.Wrapped.Count - 1; line >= 0; line--)
            {
                var lineData = TextData.Wrapped[linePosition];                
                var defaultColor = TextData.Lines[linePosition].DefaultColor;
                /* Set our initial forecolour, backcolour and reset the font style to regular */
                var bold = false;
                var underLine = false;
                var italic = false;                 
                var foreColor = defaultColor;
                var backColor = Color.Empty;
                font = Functions.SetFont(font, false, false, false);                
                /* Set the intital rectangle area/size */                
                rect.Y = rectBottom - (TextHeight*lineData.Lines.Count);                
                rect.Height = rectBottom + font.Height;
                rectBottom = rect.Y;
                /* Draw out each wrapped line taking in to account scrollbar position */
                var count = lineData.Lines.Count - 1;                                              
                int currentLine;                
                for (currentLine = 0; currentLine <= count; currentLine++)
                {                    
                    /* Verify what wrapped lines are "cut" off at the bottom of the window (total pain in the ass to get to work right) */
                    if (totalLines > scrollValue)
                    {
                        /* We shift the top of the rectangle down a line */                        
                        rectBottom += TextHeight;                        
                        totalLines--;
                        count--; /* Decrease total wrapped lines count as this line is now off the bottom */
                        currentLine = -1; /* Set this back to -1 so on next iteration its 0 (gives appearance of lines moving up/down during scrolling) */
                        continue;
                    }
                    if (currentLine == 0)
                    {
                        rectBottom -= LinePadding; /* Add a bit of padding between lines */
                    }
                    if (ShowLineMarker && count == 0 && lineData.Lines[0].Text == "-")
                    {
                        using (var pen = new Pen(LineMarkerColor))
                        {
                            deviceContext.DrawLine(pen, 0, (rect.Y + (font.Height/2)) - LinePadding, clientRect.Width,
                                                   (rect.Y + (font.Height/2)) - LinePadding);
                        }
                        break;
                    }                    
                    /* Make sure the rectangle is set to the correct positions */
                    rect.X = currentLine > 0 ? IndentWidth : 0;                    
                    rect.Y = rectBottom + (TextHeight*currentLine);                    
                    var x = 0;                    
                    if (lineData.Lines[currentLine].ControlBytes.Count > 0)
                    {                        
                        for (var cc = 0; cc <= lineData.Lines[currentLine].ControlBytes.Count - 1; cc++)
                        {                            
                            var controlData = lineData.Lines[currentLine].ControlBytes[cc];                           
                            if (x == 0 && controlData.Position > 0)
                            {
                                /* Start of the line */
                                TextRenderer.DrawText(deviceContext,
                                                      lineData.Lines[currentLine].Text.Substring(x, controlData.Position),
                                                      font, rect, foreColor, backColor, FormatFlags);
                            }
                            x = controlData.Position;
                            rect.X = (currentLine > 0 ? IndentWidth : 0) + controlData.CurrentMeasurement;                           
                            switch (controlData.ControlByte)
                            {
                                case ControlByte.Bold:
                                    bold = !bold;
                                    font = Functions.SetFont(font, bold, underLine, italic);
                                    break;

                                case ControlByte.Underline:
                                    underLine = !underLine;
                                    font = Functions.SetFont(font, bold, underLine, italic);
                                    break;

                                case ControlByte.Italic:
                                    italic = !italic;
                                    font = Functions.SetFont(font, bold, underLine, italic);
                                    break;

                                case ControlByte.Normal:
                                    bold = false;
                                    underLine = false;
                                    italic = false;
                                    font = Functions.SetFont(font, false, false, false);
                                    foreColor = defaultColor;
                                    backColor = Color.Empty;                                    
                                    break;

                                case ControlByte.Color:
                                case ControlByte.Reverse:
                                    /* Reverse and colour are handled the same */
                                    foreColor = controlData.ForeColor != Color.Empty
                                                     ? controlData.ForeColor
                                                     : defaultColor;
                                    var tmpColor = backColor;
                                    backColor = controlData.BackColor != Color.Empty
                                                    ? controlData.BackColor
                                                    : controlData.ForeColor != Color.Empty
                                                          ? tmpColor
                                                          : Color.Empty;
                                    break;
                            }
                            var nextPos = cc + 1 <= lineData.Lines[currentLine].ControlBytes.Count - 1
                                              ? lineData.Lines[currentLine].ControlBytes[cc + 1].Position
                                              : 0;
                            if (nextPos == 0 || nextPos - x <= 0)
                            {
                                continue;
                            }
                            /* Dump line of text up to position */                            
                            TextRenderer.DrawText(deviceContext, lineData.Lines[currentLine].Text.Substring(x, nextPos - x), font, rect, foreColor, backColor, FormatFlags);
                        }
                        /* Draw whatever is left over */
                        if (x <= lineData.Lines[currentLine].Text.Length - 1)
                        {
                            TextRenderer.DrawText(deviceContext, lineData.Lines[currentLine].Text.Substring(x), font, rect, foreColor, backColor, FormatFlags);
                        }
                    }
                    else
                    {
                        /* No control codes present, dump entire line */
                        TextRenderer.DrawText(deviceContext, lineData.Lines[currentLine].Text, font, rect, foreColor, backColor, FormatFlags);
                    }
                }
                linePosition--;
                /* Check also if we've exceeded the top of the window as there's no point rendering if it's not in view */
                if (linePosition < 0 || rectBottom < -TextHeight)
                {                           
                    break;
                }
            }           
        }

        private void RenderMarkedText(Graphics deviceContext, Font font, Rectangle clientRect, int scrollValue)
        {            
            /* This will draw out all the marked text data on top of a captured Bitmap image */
            if (MarkingData.MarkStartLine < 0 || MarkingData.MarkStartLine > MarkingData.MarkEndLine)
            {
                return;
            }
            /* Draw the marking bitmap */
            deviceContext.DrawImageUnscaled(MarkingData.MarkBitmap, 0, 0);                        
            /* Calculate the amount of lines to draw and the starting line's Y position */
            var lines = MarkingData.MarkEndLine - MarkingData.MarkStartLine;                                          
            var currentY = TranslateLineNumberToPosition(MarkingData.MarkStartLine, scrollValue, clientRect) - 1;
            /* Loop and mark each line */
            for (var i = 0; i <= lines; i++)
            {                
                var id = TextData.GetWrappedIndexByLineNumber(MarkingData.MarkStartLine + i); /* + i fixed getting the correct line... (5 hours later!) */
                if (id == null)
                {
                    return;
                }
                var wrapData = TextData.Wrapped[id.ActualLineNumber].Lines[id.WrappedLineNumber];
                /* Set the font */                
                var bold = i == 0 ? MarkingData.IsBold : wrapData.IsBold;
                var underLine = i == 0 ? MarkingData.IsUnderline : wrapData.IsUnderline;
                var italic = i == 0 ? MarkingData.IsItalic : wrapData.IsItalic;
                font = Functions.SetFont(font, bold, underLine, italic);
                /* This next check took over 5 hours to figure out why it wouldn't work with LinePadding - I was defining currentY within the loop,
                 * as soon as its moved outside the loop and this conditional added, works fine :/ ... oh, well ... that's programming! */
                if (i > 0)
                {
                    currentY += TextHeight;
                    if (id.WrappedLineNumber == 0)
                    {
                        currentY += LinePadding;
                    }
                }
                var text = TextData.Wrapped[id.ActualLineNumber].Lines[id.WrappedLineNumber].Text;                
                int markStart;
                int markCharStart;
                if (i == 0)
                {
                    //markStart = TextData.Lines[id.ActualLineNumber].IsIndented && id.WrappedLineNumber > 0 && MarkingData.MarkStartCharPos == 0 ? IndentWidth : MarkingData.MarkStartXPos;
                    markStart = id.WrappedLineNumber > 0 && MarkingData.MarkStartCharPos == 0 ? IndentWidth : MarkingData.MarkStartXPos;
                    markCharStart = MarkingData.MarkStartCharPos;
                }
                else
                {
                    //markStart = TextData.Lines[id.ActualLineNumber].IsIndented && id.WrappedLineNumber > 0 ? IndentWidth : 0;
                    markStart = id.WrappedLineNumber > 0 ? IndentWidth : 0;
                    markCharStart = 0;
                }
                if (i < lines)
                {
                    /* Draw either first line i == 0 or select whole line */
                    DrawMarkedText(deviceContext, font, wrapData, text, markStart, currentY,
                                   i == 0 ? MarkingData.MarkStartCharPos : 0, text.Length - 1, bold, underLine, italic);
                }
                else
                {
                    /* Draw current line up to end point */
                    DrawMarkedText(deviceContext, font, wrapData, text, markStart, currentY, markCharStart,
                                   MarkingData.MarkEndCharPos, bold, underLine, italic);
                }
            }
        }

        /* Private helper methods */
        private void DrawMarkedText(Graphics deviceContext, Font font, WrapData.WrapLineData wrapData, string text, int startX, int startY, int startPosition, int endPosition, bool bold, bool underLine, bool italic)
        {
            /* This is supposed to take in to account bolding, italics and underlining as it marks */
            var markRect = new Rectangle(startX, startY, 0, font.Height);
            var index = 0;
            using (var markBrush = new SolidBrush(ForeColor))
            {
                for (var x = startPosition; x <= endPosition; x++)
                {
                    if (x > startPosition)
                    {                        
                        var controlBytes = wrapData.GetControlBytesAtPosition(x, out index);
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

                                    case ControlByte.Underline:
                                        underLine = !underLine;
                                        font = Functions.SetFont(font, bold, underLine, italic);
                                        break;

                                    case ControlByte.Italic:
                                        italic = !italic;
                                        font = Functions.SetFont(font, bold, underLine, italic);
                                        break;

                                    case ControlByte.Normal:
                                        bold = false;
                                        underLine = false;
                                        italic = false;
                                        font = Functions.SetFont(font, false, false, false);
                                        break;
                                }
                            }
                        }
                    }
                    if (startX == 0 && endPosition - startPosition == 0 && endPosition == text.Length - 1)
                    {
                        return;
                    }
                    if (startPosition < 0)
                    {
                        continue;
                    }
                    int i;
                    if (index == wrapData.ControlBytes.Count - 1)
                    {
                        /* No more codes - increase in speed */                        
                        var len = (endPosition - x) + 1;
                        var s = text.Substring(x, len <= text.Length - 1 ? len : text.Length - 1);
                        i = TextMeasurement.MeasureStringWidth(deviceContext, font, s);
                        markRect.Width = i;
                        deviceContext.FillRectangle(markBrush, markRect);
                        TextRenderer.DrawText(deviceContext, s, font, markRect, BackColor, FormatFlags);
                        return;
                    }
                    /* We have to dump character by character for now... */
                    var chr = text[x];
                    i = TextMeasurement.MeasureStringWidth(deviceContext, font, chr);
                    markRect.Width = i;
                    deviceContext.FillRectangle(markBrush, markRect);
                    TextRenderer.DrawText(deviceContext, chr.ToString(), font, markRect, BackColor, FormatFlags);
                    markRect.X += i;
                }
            }
        }

        private int TranslateLineNumberToPosition(int lineNum, int scrollValue, Rectangle clientRect)
        {
            var currentHeight = clientRect.Height - TextHeight;
            var wrapped = TextData.Wrapped.Count - 1;
            var currentWrappedIndex = TextData.Wrapped[wrapped].Lines.Count - 1;
            currentHeight += LineSpacing > 1 ? TextHeight / LineSpacing : 0; /* Shifts the text mark down when line spacing is used */
            for (var x = TextData.WrappedLinesCount - 1; x >= 0; x--)
            {               
                if (x > scrollValue)
                {
                    currentWrappedIndex--;
                    if (currentWrappedIndex < 0)
                    {
                        /* Ignore currentHeight here, just reset the wrapped indexes to it's LAST line */
                        wrapped--;
                        currentWrappedIndex = TextData.Wrapped[wrapped].Lines.Count - 1;
                    }
                    continue;
                }
                if (currentWrappedIndex < 0)
                {
                    /* We're at 0, minus LinePadding so our start Y position of the starting line thats's marked is correct */
                    wrapped--;
                    currentWrappedIndex = TextData.Wrapped[wrapped].Lines.Count - 1;
                    currentHeight -= LinePadding;
                }
                if (x == lineNum)
                {
                    return currentHeight;
                }
                currentHeight -= TextHeight;
                currentWrappedIndex--;
            }
            return currentHeight;
        }
    }
}