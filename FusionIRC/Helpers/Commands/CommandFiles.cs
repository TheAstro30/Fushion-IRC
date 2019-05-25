/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ircScript.Classes.ScriptFunctions;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandFiles
    {
        public static void Write(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var overWrite = false;
            string[] parse;
            int i;
            if (args.StartsWith("-"))
            {
                i = args.IndexOf(' ');
                if (i == -1)
                {
                    return;
                }
                if (args[1] == 'c' || args[1] == 'C')
                {
                    overWrite = true;
                }
                parse = Misc.ParseFilenameParamater(args.Substring(i + 1));
            }
            else
            {
                parse = Misc.ParseFilenameParamater(args);
            }
            /* Get filename and data */
            i = parse[0].IndexOf(' ');
            if (i == -1)
            {
                return;
            }
            var file = parse.Length == 2 ? parse[0] : parse[0].Substring(0, i);
            var data = parse.Length == 2 ? parse[1] : parse[0].Substring(i + 1);
            try
            {
                using (
                    var fo = new FileStream(file, overWrite ? FileMode.Create : FileMode.Append, FileAccess.Write,
                                            FileShare.Write))
                {
                    try
                    {
                        using (var fw = new StreamWriter(fo))
                        {
                            fw.WriteLine(data);
                            fw.Flush();
                            fw.Close();
                        }
                    }
                    catch
                    {
                        Debug.Assert(true);
                    }
                    finally
                    {
                        fo.Close();
                    }
                }
            }
            catch
            {
                return;
            }
            return;
        }

        public static void WriteIni(string args)
        {
            /* Writeini <file> <section> <key> <value> */
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var file = string.Empty;
            if (args[0] == '"')
            {
                /* Filename starts with "<file>" */
                var end = args.IndexOf("\"", 1);
                if (end != -1)
                {
                    file = args.Substring(1, end - 1);
                    args = args.Substring(end + 1);
                }
            }
            var parts = new List<string>(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
            string section;
            string key;
            var value = new List<string>();
            if (!string.IsNullOrEmpty(file) && parts.Count >= 3)
            {
                section = parts[0];
                key = parts[1];
                value.AddRange(parts.GetRange(2, parts.Count - 2));
            }            
            else if (parts.Count >= 4)
            {
                file = parts[0];
                section = parts[1];
                key = parts[2];
                value.AddRange(parts.GetRange(3, parts.Count - 3));
            }
            else
            {
                return;
            }            
            Ini.WriteIni(file, section, key, string.Join(" ", value));
            Ini.FlushIni(file);
        }
    }
}
