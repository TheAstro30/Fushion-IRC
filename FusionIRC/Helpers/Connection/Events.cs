/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using FusionIRC.Forms.Child;
using ircCore.Settings.Theming;
using ircCore.Utils;
using ircScript;
using ircScript.Classes;
using ircScript.Classes.Structures;

namespace FusionIRC.Helpers.Connection
{
    internal static class Events
    {
        public static void Execute(string name, ScriptArgs e)
        {
            Execute(name, e, null, string.Empty);
        }

        public static void Execute(string name, ScriptArgs e, FrmChildWindow child, string text)
        {
            var s = ScriptManager.GetEvent(name);
            if (s == null)
            {
                return;
            }
            foreach (var script in s)
            {                
                if (child != null && script.EventParams != null)
                {                    
                    var process = false;
                    var match = new WildcardMatch(script.EventParams.Match);
                    var target = script.EventParams.Target.ToLower();
                    var winName = child.Tag.ToString().ToLower();
                    if (match.IsMatch(text))
                    {
                        match = new WildcardMatch(target);
                        if (match.IsMatch(winName))
                        {
                            process = true;
                        }
                        switch (child.WindowType)
                        {
                            case ChildWindowType.Channel:
                                if (target == "#")
                                {
                                    process = true;
                                }
                                break;

                            case ChildWindowType.Private:
                                if (target == "?")
                                {
                                    process = true;
                                }
                                break;

                            case ChildWindowType.DccChat:
                                if (target == "=")
                                {
                                    process = true;
                                }
                                break;
                        }
                    }
                    if (!process)
                    {
                        continue;
                    }
                }                
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
