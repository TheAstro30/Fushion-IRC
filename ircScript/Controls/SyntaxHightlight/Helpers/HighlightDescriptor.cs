/* Syntax highlighter - by Uriel Guy
 * Original version 2005
 * This version 2019 - Jason James Newland
 */
using System;
using System.Drawing;

namespace ircScript.Controls.SyntaxHightlight.Helpers
{
    public enum DescriptorType
    {
        Word = 0, /* Highlight single word */
        ToEol = 1, /* To end of line (ignores all other tokens and separators */
        ToCloseToken = 2 /* Highlights all text until the end token */
    }

    public enum DescriptorRecognition
    {
        WholeWord, /* Only if the whole token is equal to the word */
        StartsWith, /* If the word starts with the token */
        Contains /* If the word contains the Token */
    }

    public class HighlightDescriptor
    {
        public readonly string CloseToken;
        public readonly Color Color;
        public readonly DescriptorRecognition DescriptorRecognition;
        public readonly DescriptorType DescriptorType;
        public readonly Font Font;
        public readonly string Token;

        public HighlightDescriptor(string token, Color color, Font font, DescriptorType descriptorType, DescriptorRecognition dr)
        {
            if (descriptorType == DescriptorType.ToCloseToken)
            {
                throw new ArgumentException("You may not choose ToCloseToken DescriptorType without specifing an end token.");
            }
            Color = color;
            Font = font;
            Token = token;
            DescriptorType = descriptorType;
            DescriptorRecognition = dr;
            CloseToken = null;
        }

        public HighlightDescriptor(string token, string closeToken, Color color, Font font, DescriptorType descriptorType, DescriptorRecognition dr)
        {
            Color = color;
            Font = font;
            Token = token;
            DescriptorType = descriptorType;
            CloseToken = closeToken;
            DescriptorRecognition = dr;
        }
    }
}