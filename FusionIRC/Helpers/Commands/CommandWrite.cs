/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace FusionIRC.Helpers.Commands
{
    public static class CommandWrite
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
                parse = ParseFilenameParamater(args.Substring(i + 1));
            }
            else
            {
                parse = ParseFilenameParamater(args);
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
                using (var fo = new FileStream(file, overWrite ? FileMode.Create : FileMode.Append, FileAccess.Write, FileShare.Write))
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

        private static string[] ParseFilenameParamater(string args)
        {
            /* Looks for filenames with quotation marks */
            if (args.StartsWith(((char)34).ToString(CultureInfo.InvariantCulture)))
            {
                var index = args.IndexOf(((char)34).ToString(CultureInfo.InvariantCulture), 1, System.StringComparison.Ordinal);
                return index == -1
                           ? new[] { args }
                           : new[]
                               {
                                   args.Substring(0, index).Replace(((char)34).ToString(CultureInfo.InvariantCulture), null),
                                   args.Substring(index + 1).TrimStart()
                               };
            }
            return new[] { args };
        }
    }
}
