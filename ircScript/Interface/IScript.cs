/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using ircScript.Structures;

namespace ircScript.Interface
{
    public interface IScript
    {
        string Name { get; set; }

        string DisplayName { get; set; }

        string LineData { get; set; }

        string Parse(ScriptArgs e, string[] args);
    }
}
