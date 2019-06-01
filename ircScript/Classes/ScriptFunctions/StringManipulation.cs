/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
namespace ircScript.Classes.ScriptFunctions
{
    public static class StringManipulation
    {
        public static string Left(string args, int count)
        {
            if (count > 0 && count < args.Length - 1)
            {
                return args.Substring(0, count);
            }
            return string.Empty;
        }

        public static string Right(string args, int count)
        {
            if (args.Length - count > 0 && count < args.Length - 1)
            {
                return args.Substring(args.Length - count);
            }
            return string.Empty;
        }

        public static string Mid(string args, int start, int end)
        {
            var count = end != -1 ? end : args.Length - (start - 1);
            if (start <= 0 || count == 0 || count > args.Length)
            {
                return string.Empty;
            }
            return args.Substring(start - 1, count);
        }
    }
}
