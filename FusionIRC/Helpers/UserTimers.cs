/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Collections.Generic;
using ircScript.Classes.ScriptFunctions;

namespace FusionIRC.Helpers
{
    public static class UserTimers
    {
        /* Helper class for creating script-based timers */
        public static List<UserTimer> Timers = new List<UserTimer>();

        public static void CreateTimer(string name, int interval, bool isMilliseconds, bool isConstant)
        {
            var t = new UserTimer(isMilliseconds)
                        {
                            Name = name,
                            Interval = interval,
                            IsContinuous = isConstant,
                            Enabled = true
                        };
            t.OnTimer += OnTimer;
            t.OnTimerEnded += OnTimerEnded;
            Timers.Add(t);
            t.Start();
        }

        /* Callbacks */
        private static void OnTimer(UserTimer timer)
        {
            /* Main callback to trigger "on TIMER" events */
            System.Diagnostics.Debug.Print("TIMER FIRING: " + timer.Name + " " + DateTime.Now);
        }

        private static void OnTimerEnded(UserTimer timer)
        {
            System.Diagnostics.Debug.Print("Timer end");
            timer.OnTimer -= OnTimer;
            timer.OnTimerEnded -= OnTimerEnded;
            Timers.Remove(timer);
        }
    }
}
