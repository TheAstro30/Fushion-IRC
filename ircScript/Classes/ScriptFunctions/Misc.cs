/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Globalization;
using System.Windows.Forms;
using ircCore.Utils;
using ircScript.Forms;

namespace ircScript.Classes.ScriptFunctions
{
    public static class Misc
    {
        public static string[] ParseFilenameParamater(string args)
        {
            /* Looks for filenames with quotation marks */
            if (args.StartsWith(((char)34).ToString(CultureInfo.InvariantCulture)))
            {
                var index = args.IndexOf(((char)34).ToString(), 1, StringComparison.Ordinal);
                return index == -1
                           ? new[] { args }
                           : new[]
                               {
                                   args.Substring(0, index).ReplaceEx(((char)34).ToString(), string.Empty),
                                   args.Substring(index + 1).TrimStart()
                               };
            }
            return new[] { args };
        }

        public static string ParseInput(string prompt, string text = null)
        {
            using (var input = new FrmInput{Prompt = prompt,InputText = text})
            {
                if (input.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(input.InputText))
                {
                    return input.InputText;
                }
            }
            return string.Empty;
        }
    }
}
