/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Text.RegularExpressions;
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
            Execute(name, e, string.Empty);
        }

        public static void Execute(string name, ScriptArgs e, string text)
        {
            var s = ScriptManager.GetEvent(name);
            if (s == null)
            {                
                return;
            }
            foreach (var script in s)
            {
                var process = false;
                if (!string.IsNullOrEmpty(script.EventParams.Target))
                {
                    if (!string.IsNullOrEmpty(script.EventParams.Match))
                    {
                        var match = new WildcardMatch(script.EventParams.Match, RegexOptions.IgnoreCase);
                        if (match.IsMatch(text))
                        {
                            if (e.ChildWindow != null)
                            {
                                match = new WildcardMatch(script.EventParams.Target, RegexOptions.IgnoreCase);
                                if (!match.IsMatch(e.ChildWindow.Tag.ToString()))
                                {
                                    process = true;
                                }
                                switch (((FrmChildWindow) e.ChildWindow).WindowType)
                                {
                                    case ChildWindowType.Channel:
                                        if (script.EventParams.Target == "#")
                                        {
                                            process = true;
                                        }
                                        break;

                                    case ChildWindowType.Private:
                                        if (script.EventParams.Target == "?")
                                        {
                                            process = true;
                                        }
                                        break;

                                    case ChildWindowType.DccChat:
                                        if (script.EventParams.Target == "=")
                                        {
                                            process = true;
                                        }
                                        break;
                                }
                            }
                            else if (script.EventParams.Target == "?" || script.EventParams.Target == "*")
                            {
                                /* Most likely called from on NOTICE */
                                process = true;
                            }
                        }
                    }
                }
                else
                {
                    /* Probably an event without ScriptEventParams at all like connect:{ */
                    process = true;
                }
                if (!process)
                {
                    continue;
                }
                script.LineParsed += EventLineParsed;
                script.ParseCompleted += EventParseCompleted;
                script.Parse(e, text.Split(' '));
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
