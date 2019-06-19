/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Text;

namespace ircCore.Utils
{
    public static class StringExtensions
    {
        public static string ReplaceEx(this string original, string pattern, string replacement)
        {
            /* Case sensitive replace */
            return ReplaceEx(original, pattern, replacement, StringComparison.Ordinal);
        }

        public static string ReplaceEx(this string original, string pattern, string replacement, StringComparison comparison)
        {
            if (original == null)
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(pattern))
            {
                return original;
            }
            var posCurrent = 0;
            var lenPattern = pattern.Length;
            var idxNext = original.IndexOf(pattern, comparison);
            var result = new StringBuilder(Math.Min(4096, original.Length));
            while (idxNext >= 0)
            {
                result.Append(original, posCurrent, idxNext - posCurrent);
                result.Append(replacement);
                posCurrent = idxNext + lenPattern;
                idxNext = original.IndexOf(pattern, posCurrent, comparison);
            }
            result.Append(original, posCurrent, original.Length - posCurrent);
            return result.ToString();
        }
    }
}
