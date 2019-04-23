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
    internal class ScriptIdentifierParser
    {
        private readonly Regex _tokenIdentifiers = new Regex(@"\$\w+", RegexOptions.Compiled); /* Replaces $chan, etc */

        public string Parse(ScriptArgs e, string lineData)
        {
            var m = _tokenIdentifiers.Matches(lineData);
            var sb = new StringBuilder(lineData);
            if (m.Count > 0)
            {
                for (var i = m.Count - 1; i >= 0; i--)
                {
                    if (e == null)
                    {
                        /* Remove the identifers found */
                        sb.Replace(m[i].Value, "", m[i].Index, m[i].Length);
                    }
                    else
                    {
                        /* Replace identifiers from the values in ScriptArgs */
                        switch (m[i].Value.ToUpper())
                        {
                            case "$ME":
                                sb.Replace(m[i].Value, e.ClientConnection.UserInfo.Nick, m[i].Index, m[i].Value.Length);
                                break;

                            case "$CHAN":
                                sb.Replace(m[i].Value, e.Channel, m[i].Index, m[i].Value.Length);
                                break;

                            case "$NICK":
                                sb.Replace(m[i].Value, e.Nick, m[i].Index, m[i].Value.Length);
                                break;
                        }
                    }
                }
            }
            return sb.ToString(); 
        }
    }
}
