/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Globalization;
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

        public static string CTime()
        {
            return Epoch(DateTime.Now).ToString(CultureInfo.InvariantCulture);
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

        public static string GetDuration(int durationSecs, bool digitalTime)
        {
            var ts = new TimeSpan(0, 0, 0, durationSecs);
            if (!digitalTime)
            {
                var wks = ts.Days / 7;
                var days = ts.Days - (wks * 7);
                if (durationSecs > 0)
                {
                    return (wks > 0 ? string.Format(wks > 1 ? "{0:#}wks" : "{0:#}wk", wks) + " " : null) +
                           (days > 0 ? string.Format(days > 1 ? "{0:#}days" : "{0:#}day", days) + " " : null) +
                           (ts.Hours > 0 ? string.Format(ts.Hours > 1 ? "{0:#}hrs" : "{0:#}hr", ts.Hours) + " " : null) +
                           (ts.Minutes > 0 ? string.Format(ts.Minutes > 1 ? "{0:#}mins" : "{0:#}min", ts.Minutes) + " " : null) +
                           (ts.Seconds > 0 ? string.Format(ts.Seconds > 1 ? "{0:#}secs" : "{0:#}sec", ts.Seconds) : null);
                }
                return "0secs";
            }
            var hrs = (ts.Days * 24) + ts.Hours;
            return string.Format((hrs > 0 ? hrs < 99 ? "{0:00}:" : "{0:000}:" : null) + "{1:00}:{2:00}", hrs, ts.Minutes, ts.Seconds);
        }

        /* Private methods */
        private static int StrToInt(string value)
        {
            int i;
            return int.TryParse(value, out i) ? i : 0;
        }

        private static uint Epoch(DateTime fromDate)
        {
            var baseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var span = fromDate.ToUniversalTime() - baseDate;
            return (uint)span.TotalSeconds;
        }
    }
}
