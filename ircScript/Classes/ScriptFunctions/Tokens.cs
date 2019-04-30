/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace ircScript.Classes.ScriptFunctions
{
    public static class Tokens
    {
        public static string ScriptGetDelToken(string[] args, bool isGet)
        {
            int ch;
            if (!int.TryParse(args[2], out ch))
            {
                return string.Empty;
            }            
            var start = 1;
            var end = 1;
            if (args[1].Contains("-"))
            {
                var d = args[1].Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (d.Length > 1)
                {
                    /* $gettok(string,1-N,32)/$deltok(string,1-N,32) */
                    if (!int.TryParse(d[0], out start))
                    {
                        start = 1;
                    }
                    if (!int.TryParse(d[1], out end))
                    {
                        end = start;
                    }
                }
                else if (d.Length == 1)
                {
                    /* $gettok(string,1-,32)/$deltok(string,1-,32) */
                    if (!int.TryParse(d[0], out start))
                    {
                        start = 1;
                    }
                    end = TokenLength(args[0], (char) ch) - start;
                }
            }
            else
            {
                /* $gettok(string,1,32)/$deltok(string,1,32) */
                if (!int.TryParse(args[1], out start))
                {
                    start = 1;
                }
                end = start;
            }
            return start > 0
                       ? (isGet ? GetToken(args[0], (char) ch, start, end) : DelToken(args[0], (char) ch, start, end))
                       : (isGet ? TokenLength(args[0], (char) ch).ToString() : args[0]);
        }

        public static string ScriptAddToken(string[] args)
        {            
            int ch;
            return !int.TryParse(args[2], out ch) ? string.Empty : AddToken(args[0], args[1], (char) ch);
        }

        /* Private helper methods */
        private static int TokenLength(string line, char delim)
        {
            var token = line.Split(new[] {delim}, StringSplitOptions.RemoveEmptyEntries);
            return token.Length;
        }

        private static string AddToken(string line, string newToken, char delim)
        {
            var tokens = new List<string>(line.Split(new[] {delim}, StringSplitOptions.RemoveEmptyEntries));
            /* Only add the token if it doesn't already exist */
            if (tokens.Any(t => t.Equals(newToken,StringComparison.InvariantCultureIgnoreCase)))
            {
                return string.Empty;
            }
            tokens.Add(newToken);
            return string.Join(delim.ToString(), tokens);
        }

        private static string DelToken(string line, char delim, int start, int end)
        {
            var tokens = new List<string>(line.Split(new[] {delim}, StringSplitOptions.RemoveEmptyEntries));
            if (start == end && start > 0 && start < tokens.Count)
            {
                tokens.RemoveAt(start - 1);
                return string.Join(delim.ToString(), tokens);
            }
            if (end > start && end <= tokens.Count)
            {
                /* Range of tokens */
                for (var i = end - 1; i >= start - 1; i--)
                {
                    tokens.RemoveAt(i);
                }
                return string.Join(delim.ToString(), tokens);
            }
            return string.Empty;
        }

        private static string GetToken(string line, char delim, int start, int end)
        {
            var tokens = new List<string>(line.Split(new[] {delim}, StringSplitOptions.RemoveEmptyEntries));
            if (start == end && start > 0 && start <= tokens.Count)
            {
                return tokens[start - 1];
            }
            if (end > start && end <= tokens.Count)
            {
                for (var i = start - 1; i >= 0; i--)
                {
                    tokens.RemoveAt(i);
                }
                return string.Join(delim.ToString(), tokens);
            }
            return string.Empty;
        }
    }
}

