/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ircCore.Controls.ChildWindows.OutputDisplay.Helpers
{
    /* GDI font caching class for faster string/character width measurement */
    internal class FontData
    {
        public int[] NormalCharacterWidth { get; set; }
        public int[] BoldCharacterWidth { get; set; }
        public int[] ItalicCharacterWidth { get; set; }
    }

    internal static class TextMeasurement
    {
        private static readonly Dictionary<Font, FontData> Fonts = new Dictionary<Font, FontData>();

        /* Public measure string methods */
        public static int MeasureStringWidth(Graphics g, Font font, string text)
        {
            return string.IsNullOrEmpty(text)
                       ? 0
                       : MeasureStringWidth(g, text.ToCharArray(), font, FontLookupList(g, font));
        }

        public static int MeasureStringWidth(Graphics g, Font font, char chr)
        {
            return MeasureStringWidth(g, chr, font, FontLookupList(g, font));
        }

        public static int MeasureStringWidth(Graphics g, Font font, char[] chr)
        {
            return MeasureStringWidth(g, chr, font, FontLookupList(g, font));
        }

        /* Measure string overloads */
        private static int MeasureStringWidth(IDeviceContext g, IList<char> text, Font font, FontData data)
        {
            if (text == null || text.Count == 0)
            {
                return 0;
            }
            return text.Count == 1
                       ? MeasureStringWidth(g, text[0], font, data)
                       : text.Sum(chr => MeasureStringWidth(g, chr, font, data));
        }

        private static int MeasureStringWidth(IDeviceContext g, char chr, Font font, FontData data)
        {
            var chrValue = (int) chr;
            if ((font.Bold && font.Italic) || font.Bold)
            {
                if (chrValue > 255 && data.BoldCharacterWidth[chrValue] == 0)
                {
                    data.BoldCharacterWidth[chrValue] = MeasureString(g, font,
                                                                      chr.ToString(CultureInfo.InvariantCulture));
                }
                return data.BoldCharacterWidth[chrValue];
            }
            if (font.Italic)
            {
                if (chrValue > 255 && data.ItalicCharacterWidth[chrValue] == 0)
                {
                    data.ItalicCharacterWidth[chrValue] = MeasureString(g, font,
                                                                        chr.ToString(CultureInfo.InvariantCulture));
                }
                return data.ItalicCharacterWidth[chrValue];
            }
            if (chrValue > 255 && data.NormalCharacterWidth[chrValue] == 0)
            {
                data.NormalCharacterWidth[chrValue] = MeasureString(g, font, chr.ToString(CultureInfo.InvariantCulture));
            }
            return data.NormalCharacterWidth[chrValue];
        }

        /* Character lookup list */
        private static FontData FontLookupList(IDeviceContext g, Font font)
        {
            FontData data;
            if (!Fonts.ContainsKey(font))
            {
                data = new FontData
                           {
                               NormalCharacterWidth = BuildLookupList(g, font),
                               BoldCharacterWidth = BuildLookupList(g, new Font(font, FontStyle.Bold)),
                               ItalicCharacterWidth = BuildLookupList(g, new Font(font, FontStyle.Italic))
                           };
                Fonts.Add(font, data);
            }
            else
            {
                data = Fonts[font];
            }
            return data;
        }

        private static int[] BuildLookupList(IDeviceContext g, Font font)
        {
            var lookUp = new int[char.MaxValue];
            for (var i = (char) 0; i < (char) 256; i++)
            {
                lookUp[i] = MeasureString(g, font, i.ToString(CultureInfo.InvariantCulture));
            }
            return lookUp;
        }

        /* GDI measure string method */
        private static int MeasureString(IDeviceContext g, Font font, string text)
        {
            return string.IsNullOrEmpty(text)
                       ? 0
                       : TextRenderer.MeasureText(g, text, font, Size.Empty,
                                                  TextFormatFlags.Left | TextFormatFlags.NoPrefix |
                                                  TextFormatFlags.NoPadding | TextFormatFlags.NoClipping).Width;
        }
    }
}
