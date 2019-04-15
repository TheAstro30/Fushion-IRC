/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Text.RegularExpressions;

namespace ircScript.Classes
{
    /* Script parser class */
    public class ScriptParser
    {
        private readonly Regex _paramStrip = new Regex(@"\$\d+", RegexOptions.Compiled);

        /* Inherited on all subsequent "script" classes - this will grow over time and be improved upon */
        public string Parse(string lineData, string[] args)
        {
            /* Main script "parser" - mainly for parsing arguments */ 
            if (args == null || args.Length == 0)
            {
                /* Strip out $1, $2 - etc */
                return _paramStrip.Replace(lineData, "");
            }
            /* Process command-line arguments - replacing $1, $2 etc */
            var index = 1;
            foreach (var s in args)
            {
                lineData = lineData.Replace(string.Format("${0}", index), s);
                index++;
            }
            /* Return the parsed line (remove any unused params) */
            return _paramStrip.Replace(lineData, "");
        }
    }
}
