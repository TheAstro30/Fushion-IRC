/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms.Child;
using ircClient;
using ircCore.Settings.Theming;
using ircCore.Utils;
using ircScript;
using ircScript.Classes;
using ircScript.Classes.Structures;

namespace FusionIRC.Helpers.Commands
{
    internal static class CommandAlias
    {
        public static bool ParseAlias(ClientConnection client, FrmChildWindow child, string command, string args)
        {
            /* First check it's not an alias, if it is - pass it back to command processor */
            var alias = ScriptManager.GetScript(ScriptManager.Aliases, command);
            if (alias == null)
            {
                return false;
            }
            var sp = new ScriptArgs
                         {
                             ClientConnection = client,
                             ChildWindow = child,
                             Channel = child.Tag.ToString()[0] == '#' ? child.Tag.ToString() : string.Empty,
                             Nick =
                                 child.WindowType == ChildWindowType.Private ||
                                 child.WindowType == ChildWindowType.DccChat
                                     ? child.Tag.ToString()
                                     : string.Empty
                         };
            alias.LineParsed += ScriptLineParsed;
            alias.ParseCompleted += ScriptParseCompleted;
            alias.Parse(sp, args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return true;
        }

        /* Script callbacks */
        private static void ScriptLineParsed(Script script, ScriptArgs e, string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }
            var i = data.IndexOf(' ');
            string command;
            var args = string.Empty;
            if (i == -1)
            {
                command = data.Trim();
            }
            else
            {
                command = data.Substring(0, i).Trim().ToUpper().ReplaceEx("/", string.Empty);
                args = data.Substring(i + 1).Trim();
            }
            CommandProcessor.ParseCommand(e.ClientConnection, (FrmChildWindow)e.ChildWindow, command, args);
        }

        private static void ScriptParseCompleted(Script script)
        {
            script.LineParsed -= ScriptLineParsed;
            script.ParseCompleted -= ScriptParseCompleted;
        }
    }
}
