/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using FusionIRC.Forms.Child;
using ircScript;
using ircScript.Classes;
using ircScript.Classes.Structures;

namespace FusionIRC.Helpers.Connection
{
    internal static class Events
    {
        public static void Execute(string name, ScriptArgs e)
        {
            Execute(name, e, null);
        }

        public static void Execute(string name, ScriptArgs e, ScriptEventParams param)
        {
            var s = ScriptManager.GetEvent(name);
            if (s == null)
            {
                return;
            }
            foreach (var script in s)
            {
                script.EventParams = param;
                script.LineParsed += EventLineParsed;
                script.ParseCompleted += EventParseCompleted;
                script.Parse(e);
            }
        }

        private static void EventLineParsed(Script s, ScriptArgs e, string line)
        {
            CommandProcessor.Parse(e.ClientConnection, (FrmChildWindow) e.ChildWindow, line);
        }

        private static void EventParseCompleted(Script s)
        {
            s.LineParsed -= EventLineParsed;
            s.ParseCompleted -= EventParseCompleted;
        }
    }
}
