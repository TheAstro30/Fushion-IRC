/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
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

        [Description("User Join")]
        UserJoin = 2,

        [Description("User Part")]
        UserPart = 3,

        [Description("User Quit")]
        UserQuit = 4,

        [Description("User Kick")]
        UserKick = 5,

        [Description("Kick (Yourself)")]
        SelfKick = 6,

        [Description("Nick Change")]
        Nick = 7,

        [Description("Private Message")]
        PrivateMessage = 8,

        [Description("Notices")]
        Notice = 9,

        [Description("Invites")]
        Invite = 10,

        [Description("Notify Online")]
        NotifyOnline = 11,

        [Description("Notify Offline")]
        NotifyOffline = 12,

        [Description("DCC Get Request")]
        DccGet = 13,

        [Description("DCC Chat Request")]
        DccChat = 14,

        [Description("DCC Transfer Completed")]
        DccTransferComplete = 15,

        [Description("DCC Transfer Failed")]
        DccTransferFailed = 16
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
                                 ? Path.GetFileName(SoundPath)
                                 : string.Empty;
            }
        }
    }

    [Serializable]
    public class ThemeSounds
    {        
        public bool Enable { get; set; }

        public List<ThemeSoundData> SoundData = new List<ThemeSoundData>();

        public ThemeSounds()
        {
            /* Empty default constructor */
        }

        public ThemeSounds(ThemeSounds sounds)
        {
            /* Copy constructor */
            Enable = sounds.Enable;
            SoundData = new List<ThemeSoundData>(sounds.SoundData);
        }
    }
}
