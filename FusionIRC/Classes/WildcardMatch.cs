/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Text.RegularExpressions;

namespace FusionIRC.Classes
{
    public sealed class WildcardMatch : Regex
    {
        /* Wildcard matching extension class
           by: Derik Palacino
           http://social.msdn.microsoft.com/Forums/en/csharpgeneral/thread/bce145b8-95d4-4be4-8b07-e8adee7286f1
         */
        
        /* Constructors */
        public WildcardMatch(string pattern) : base(WildcardToRegex(pattern))
        {
            /* Empty constructor */
        }

        public WildcardMatch(string pattern, RegexOptions options) : base(WildcardToRegex(pattern), options)
        {
            /* Empty constructor */
        }
        
        /* Public method */        
        public static string WildcardToRegex(string pattern)
        {
            return "^" + Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }        
    }
}