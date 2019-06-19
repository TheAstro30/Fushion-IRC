/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Text.RegularExpressions;

namespace ircCore.Utils
{
    public sealed class WildcardMatch : Regex
    {
        /* Wildcard matching extension class
           by: Derik Palacino
           http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/bce145b8-95d4-4be4-8b07-e8adee7286f1
         */
        public WildcardMatch(string pattern) : base(WildcardToRegex(pattern))
        {
            /* Empty constructor */
        }

        public WildcardMatch(string pattern, RegexOptions options) : base(WildcardToRegex(pattern), options)
        {
            /* Empty constructor */
        }

        public static string WildcardToRegex(string pattern)
        {
            return string.Format("^{0}$", Escape(pattern).ReplaceEx("\\*", ".*").ReplaceEx("\\?", "."));
        }
    }
}
