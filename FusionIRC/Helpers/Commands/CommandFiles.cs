/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using ircScript.Classes.ScriptFunctions;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandFiles
    {
        public static void WriteIni(string args)
        {
            /* Writeini <file> <section> <key> <value> */
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var parts = new List<string>(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
            if (parts.Count < 4)
            {
                return;
            }
            var value = new List<string>(parts.GetRange(3, parts.Count - 3));
            Ini.WriteIni(parts[0], parts[1], parts[2], string.Join(" ", value));
            Ini.FlushIni(parts[0]);
        }
    }
}
