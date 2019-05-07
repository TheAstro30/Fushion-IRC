/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.IO;

namespace ircScript.Classes.ScriptFunctions
{
    public static class FileFunctions
    {
        public static string Read(string file)
        {
            /* Reads a random line of the file */
            if (File.Exists(file))
            {
                try
                {
                    var lines = File.ReadAllLines(file);
                    var rnd = new Random();
                    var line = rnd.Next(lines.Length);
                    return lines[line];
                }
                catch
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        public static string Read(string file, int line)
        {
            /* Returns specific line */
            string[] lines;
            try
            {
                if (!File.Exists(file))
                {
                    return string.Empty;
                }
                lines = File.ReadAllLines(file);
            }
            catch
            {
                return string.Empty;
            }
            /* Return the count if 0, null/empty if exceeds count or the line number */
            return line == 0 ? lines.Length.ToString() : (line > lines.Length ? string.Empty : lines[line - 1]);
        }
    }
}
