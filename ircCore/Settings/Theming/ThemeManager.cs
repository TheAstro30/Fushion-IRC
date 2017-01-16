/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Text;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Settings.Theming
{
    public enum ChildWindowType
    {
        Console = 0,
        Channel = 1,
        Private = 2,
        DccChat = 3
    }

    public enum ThemeColor
    {
        WindowBackColor = 0,
        WindowForeColor = 1
    }

    public enum ThemeMessage
    {
        ChannelTopic = 0,
        ChannelTopicSet = 1,
        ChannelTopicChange = 2,
        ChannelText = 3,
        ChannelSelfText = 4,
        ChannelActionText = 5,
        ChannelSelfActionText = 6,
        PrivateText = 7,
        PrivateSelfText = 8,
        PrivateActionText = 9,
        PrivateSelfActionText = 10,
        NoticeServerText = 11,
        NoticeText = 12,
        NoticeSelfText = 13,
        ChannelJoinText = 14,
        ChannelSelfJoinText = 15,
        ChannelPartText = 16,
        ModeChannelText = 17,        
        ModeSelfText = 18,
        QuitText = 19,
        ChannelKickText = 20,
        ChannelSelfKickText = 21,
        NickChange = 22
    }

    public class IncomingMessageData
    {
        public ThemeMessage Message { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Nick { get; set; }
        public string Address { get; set; }
        public string Target { get; set; }
        public string Text { get; set; }
        public string NewNick { get; set; }
        public string KickedNick { get; set; }        
    }

    public class ParsedMessageData
    {
        public int DefaultColor { get; set; }
        public string Message { get; set; }
    }

    public static class ThemeManager
    {
        /* Basic theme management class - mainly loads and saves the current theme */
        public static Action ThemeLoaded;

        public static Theme CurrentTheme = new Theme();

        public static void Load(string themeFile)
        {
            if (!BinarySerialize<Theme>.Load(themeFile, ref CurrentTheme))
            {
                CurrentTheme = new Theme();
            }
            if (ThemeLoaded != null)
            {
                ThemeLoaded();
            }
        }

        public static void Save(string themeFile)
        {
            if (!CurrentTheme.ThemeChanged)
            {
                return;
            }            
            CurrentTheme.ThemeChanged = false;
            BinarySerialize<Theme>.Save(themeFile, CurrentTheme);
        }

        /* Accessible theme properties */
        public static Font GetFont(ChildWindowType window)
        {
            return !CurrentTheme.ThemeFonts.ContainsKey(window) ? new Font("Lucida Console", 10) : CurrentTheme.ThemeFonts[window];
        }

        public static Theme.ThemeBackgroundData GetBackground(ChildWindowType window)
        {
            return !CurrentTheme.ThemeBackgrounds.ContainsKey(window) ? null : CurrentTheme.ThemeBackgrounds[window];
        }

        public static Color GetColor(ThemeColor color)
        {
            return !CurrentTheme.ThemeColors.ContainsKey(color) ? CurrentTheme.Colors[0] : CurrentTheme.Colors[CurrentTheme.ThemeColors[color]];
        }

        /* The main theme message parser */
        public static ParsedMessageData ParseMessage(IncomingMessageData messageData)
        {
            var pmd = new ParsedMessageData
                          {
                              DefaultColor = 1,
                              Message = "[Theme file corrupt/missing data]"
                          };
            if (!CurrentTheme.Messages.ContainsKey(messageData.Message))
            {
                return pmd;
            }
            /* This code is kind of ugly... */            
            var sb = new StringBuilder(CurrentTheme.Messages[messageData.Message].MessageFormat);
            sb.Replace("$ts", TimeFunctions.FormatTimeStamp(messageData.TimeStamp, CurrentTheme.TimeStampFormat));
            sb.Replace("$nick", messageData.Nick);
            sb.Replace("$address", messageData.Address);
            sb.Replace("$me", messageData.Nick);
            sb.Replace("$newnick", messageData.NewNick);
            sb.Replace("$knick", messageData.KickedNick);
            sb.Replace("$text", messageData.Text);
            sb.Replace("$target", messageData.Target);
            pmd.DefaultColor = CurrentTheme.Messages[messageData.Message].DefaultColor;
            pmd.Message = sb.ToString();
            return pmd;
        }
    }
}
