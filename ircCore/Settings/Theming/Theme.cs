/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using ircCore.Controls.ChildWindows.IrcWindow.Helpers;

namespace ircCore.Settings.Theming
{   
    [Serializable]
    public class Theme
    {
        /* Binary serialized class of all output display data and color schemes */
        [Serializable]
        public class ThemeBackgroundData
        {
            public string Path { get; set; }
            public BackgroundImageLayoutStyles LayoutStyle { get; set; }
        }

        [Serializable]
        public class ThemeMessageData
        {
            public int DefaultColor { get; set; }
            public string MessageFormat { get; set; }
        }

        private const string Rgb = "#FFFFFF,#000000,#00007F,#009300,#FF0000,#7F0000,#9C009C,#FC7F00,#FFFF00,#00FC00,#009393,#00FFFF,#0000FC,#FF00FF,#7F7F7F,#D2D2D2";
        private const string DefaultColors = "0,1";

        /* Public properties */
        public bool ThemeChanged { get; set; }

        public string Name { get; set; }

        public Color[] Colors { get; set; }

        public string TimeStampFormat { get; set; }

        public Dictionary<ThemeColor, int> ThemeColors = new Dictionary<ThemeColor, int>();

        public Dictionary<ThemeWindow, Font> ThemeFonts = new Dictionary<ThemeWindow, Font>();

        public Dictionary<ThemeWindow, ThemeBackgroundData> ThemeBackgrounds = new Dictionary<ThemeWindow, ThemeBackgroundData>();

        public Dictionary<ThemeMessage, ThemeMessageData> Messages = new Dictionary<ThemeMessage, ThemeMessageData>();
        
        /* Constructor */
        public Theme()
        {
            /* We set up our basic default theme if none exist or has been deleted */
            Name = "Default";
            Colors = new Color[16];
            /* Default Colors */
            var c = Rgb.Split(',');
            for (var color = 0; color <= c.Length - 1; color++)
            {
                Colors[color] = ColorTranslator.FromHtml(c[color]);
            }
            var d = DefaultColors.Split(',');
            for (var defColor = 0; defColor <= d.Length - 1; defColor++)
            {
                int i;
                if (int.TryParse(d[defColor], out i))
                {
                    ThemeColors.Add((ThemeColor) defColor, i);
                }
            }
            /* Default time stamp */
            TimeStampFormat = "[h:nnt]";
            /* Default fonts */
            ThemeFonts.Add(ThemeWindow.Console, new Font("Lucida Console", 10));
            ThemeFonts.Add(ThemeWindow.Channel, new Font("Lucida Console", 10));
            ThemeFonts.Add(ThemeWindow.Private, new Font("Lucida Console", 10));
            /* Default messages */
            Messages.Add(ThemeMessage.ChannelTopic, new ThemeMessageData { DefaultColor = 3, MessageFormat = "$ts * Topic is: '$text'" });
            Messages.Add(ThemeMessage.ChannelTopicSet, new ThemeMessageData { DefaultColor = 3, MessageFormat = "$ts * Topic set by: $nick $text" });
            Messages.Add(ThemeMessage.ChannelTopicChange, new ThemeMessageData { DefaultColor = 3, MessageFormat = "$ts * $nick changes $target topic to '$text'" });
            Messages.Add(ThemeMessage.ChannelText, new ThemeMessageData {DefaultColor = 1, MessageFormat = "$ts <$nick ($address)> $text"});
            Messages.Add(ThemeMessage.ChannelSelfText, new ThemeMessageData { DefaultColor = 1, MessageFormat = "$ts <$me> $text" });
            Messages.Add(ThemeMessage.ChannelActionText, new ThemeMessageData { DefaultColor = 6, MessageFormat = "$ts * $nick $text" });
            Messages.Add(ThemeMessage.ChannelSelfActionText, new ThemeMessageData { DefaultColor = 6, MessageFormat = "$ts * $me $text" });
            Messages.Add(ThemeMessage.ChannelJoinText, new ThemeMessageData { DefaultColor = 3, MessageFormat = "$ts * $nick ($address) has joined: $target" });
            Messages.Add(ThemeMessage.ChannelSelfJoinText, new ThemeMessageData { DefaultColor = 3, MessageFormat = "$ts * Now talking in: $target" });
            Messages.Add(ThemeMessage.ChannelPartText, new ThemeMessageData { DefaultColor = 3, MessageFormat = "$ts * $nick ($address) has left: $target" });
            Messages.Add(ThemeMessage.QuitText, new ThemeMessageData { DefaultColor = 4, MessageFormat = "$ts * $nick ($address) has quit IRC ($text)" });
            Messages.Add(ThemeMessage.ChannelKickText, new ThemeMessageData { DefaultColor = 4, MessageFormat = "$ts * $knick was kicked from $target by $nick '$text')" });
            Messages.Add(ThemeMessage.ChannelSelfKickText, new ThemeMessageData { DefaultColor = 4, MessageFormat = "$ts * You were kicked from $target by $nick '$text')" });
            Messages.Add(ThemeMessage.ModeChannelText, new ThemeMessageData { DefaultColor = 2, MessageFormat = "$ts * $nick sets mode: $text" });
            Messages.Add(ThemeMessage.ModeUserText, new ThemeMessageData { DefaultColor = 2, MessageFormat = "$ts * $nick sets mode: $text $target" });
            Messages.Add(ThemeMessage.ModeSelfText, new ThemeMessageData { DefaultColor = 2, MessageFormat = "$ts * $me sets mode: $text" });
            //delete this
            var bgd = new ThemeBackgroundData {Path = "c.jpg", LayoutStyle = BackgroundImageLayoutStyles.Photo};
            ThemeBackgrounds.Add(ThemeWindow.Channel, bgd);
            /* Set this flag to true to ensure it will be saved if it doesn't exist (ie: load fails) */
            ThemeChanged = true;
        }        
    }
}
