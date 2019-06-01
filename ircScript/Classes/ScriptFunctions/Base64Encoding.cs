/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ircScript.Classes.ScriptFunctions
{
    /* Encoding extension class */
    public static class Base64Encoding
    {
        private static readonly Regex Base64 = new Regex(@"^[a-zA-Z0-9\+/]*={0,2}$", RegexOptions.Compiled);

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
            if (string.IsNullOrEmpty(encodedText) || !IsBase64String(encodedText))
            {
                return string.Empty;
            }
            var textAsBytes = Convert.FromBase64String(encodedText);
            return encoding.GetString(textAsBytes);
        }

        public static bool IsBase64String(this string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Base64.IsMatch(s);
        }
    }
}
