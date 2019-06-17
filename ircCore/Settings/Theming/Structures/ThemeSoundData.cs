/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.IO;
using ircCore.Utils;

namespace ircCore.Settings.Theming.Structures
{
    public enum ThemeSoundType
    {
        None = 0,
        Default = 1,
        User = 2
    }

    public enum ThemeSound
    {
        [Description("Connect")]
        Connect = 0,

        [Description("Disconnect")]
        Disconnect = 1,

        [Description("Private Message")]
        PrivateMessage = 10
    }

    [Serializable]
    public class ThemeSoundData
    {
        public ThemeSound ThemeSound { get; set; }

        public bool Enabled { get; set; }

        public string Name
        {
            get { return Functions.EnumUtils.GetDescriptionFromEnumValue(ThemeSound); }
        }

        public ThemeSoundType Type { get; set; }

        public string SoundPath { get; set; }

        public string SoundPathString
        {
            get
            {
                return Type == ThemeSoundType.Default
                           ? "<Default>"
                           : Type != ThemeSoundType.None
                                 ? Path.GetFileNameWithoutExtension(SoundPath)
                                 : string.Empty;
            }
        }
    }
}
