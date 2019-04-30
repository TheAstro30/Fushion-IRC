/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Text;

namespace ircScript.Classes.ScriptFunctions
{
    /* Encoding extension class */
    public static class Base64Encoding
    {
        public static string Base64Encode(this Encoding encoding, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            var textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        public static string Base64Decode(this Encoding encoding, string encodedText)
        {
            if (string.IsNullOrEmpty(encodedText))
            {
                return string.Empty;
            }
            var textAsBytes = Convert.FromBase64String(encodedText);
            return encoding.GetString(textAsBytes);
        }
    }
}
