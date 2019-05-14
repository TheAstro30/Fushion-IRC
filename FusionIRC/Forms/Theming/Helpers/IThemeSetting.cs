/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms.Theming.Helpers
{
    public interface IThemeSetting
    {
        event Action ThemeChanged;

        Theme CurrentTheme { get; set; }

        void SaveSettings();
    }
}
