/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Text;

namespace ircCore.Utils
{
    public static class TimeFunctions
    {
        public static string FormatAsciiTime(string time, string format)
        {
            /* Returns the time from a long (tick value) time */
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(StrToInt(time)).ToLocalTime();
            return !string.IsNullOrEmpty(format) ? FormatTimeStamp(dt, format) : string.Format("{0:ddd MMM dd hh:mm:ss yyyy}", dt);
        }

        public static string FormatTimeStamp(DateTime date, string timeStamp)
        {
            /* Replace invalid chars (fixed again 9 July, 2010 : refactored 2017) */
            var sb = new StringBuilder(timeStamp);
            sb.Replace("m", "M");
            sb.Replace("N", "m");
            sb.Replace("n", "m");
            sb.Replace("S", "s");
            sb.Replace("D", "d");
            sb.Replace("Y", "y");
            sb.Replace("T", "t");
            /* {0:h hh H HH} style formatting */
            sb = new StringBuilder(string.Format(string.Format("{0}{1}{2}", "{0:", sb, "}"), date));            
            sb.Replace("A", "a");
            sb.Replace("P", "p");
            sb.Replace("M", "m");            
            return sb.ToString();
        }

        /* Private methods */
        private static int StrToInt(string value)
        {
            int i;
            return int.TryParse(value, out i) ? i : 0;
        }
    }
}
