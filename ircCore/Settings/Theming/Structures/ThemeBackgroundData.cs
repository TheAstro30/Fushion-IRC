/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;

namespace ircCore.Settings.Theming.Structures
{
    [Serializable]
    public class ThemeBackgroundData
    {
        public string Path { get; set; }

        public BackgroundImageLayoutStyles LayoutStyle { get; set; }
    }
}
