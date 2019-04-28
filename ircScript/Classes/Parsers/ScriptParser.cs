/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Text;
using System.Text.RegularExpressions;
using ircScript.Classes.Structures;

namespace ircScript.Classes.Parsers
{
    /* Script parser class */
    internal class ScriptParser
    {
        private readonly Regex _singleArgs = new Regex(@"\$\d+", RegexOptions.Compiled); /* Replaces $[N] */
        private readonly Regex _multiArgs = new Regex(@"\$\d+-", RegexOptions.Compiled); /* Replaces $[N]- */

        private readonly ScriptIdentifierParser _identifier = new ScriptIdentifierParser();
        private readonly ScriptVariableParser _variables = new ScriptVariableParser();

        private readonly ScriptVariables _localVariables;

        internal ScriptParser(ScriptVariables localVariables)
        {
            _localVariables = localVariables;
        }
        
        internal string Parse(ScriptArgs e, string lineData, string[] args)
        {
            System.Diagnostics.Debug.Print("CURRENT LINE PARSING: " + lineData);
            /* Main script "parser" - mainly for parsing arguments - replace $[N]- tokens first */
            lineData = ReplaceTokens(_multiArgs, lineData, args, true);
            /* Replace $1, $2 etc */
            lineData = ReplaceTokens(_singleArgs, lineData, args, false);
            /* Next thing to do would be to process local and global variables... */
            lineData = _variables.Parse(_localVariables, lineData);
            /* Parse $id (like $chan, $nick via ScriptArgs including looking for $alias */
            lineData = _identifier.Parse(e, lineData);            
            return lineData;
        }

        /* Private parsing methods */
        private static string ReplaceTokens(Regex match, string lineData, string[] args, bool multi)
        {
            var m = match.Matches(lineData);
            if (m.Count > 0)
            {
                var sb = new StringBuilder(lineData);
                for (var i = m.Count - 1; i >= 0; i--)
                {
                    int index;
                    if (multi)
                    {
                        /* Attempt to get the numeric value of $[N]- */                        
                        if (!int.TryParse(m[i].Value.Replace("$", "").Replace("-", ""), out index))
                        {
                            index = 0;
                        }
                        sb.Replace(m[i].Value,
                                   args != null && args.Length > 0 && args.Length - (index - 1) > 0
                                       ? string.Join(" ", args, index - 1, args.Length - (index - 1))
                                       : "");
                    }
                    else
                    {
                        /* Single tokens $[N] */
                        if (!int.TryParse(m[i].Value.Replace("$", ""), out index))
                        {
                            index = 0;
                        }
                        sb.Replace(m[i].Value, args != null && index - 1 < args.Length ? args[index - 1] : "");                        
                    }
                }                
                return sb.ToString();
            }
            /* Short, but, again, sweet :P */
            return lineData;
        }
    }
}
