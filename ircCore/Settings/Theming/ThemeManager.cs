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
        ConnectingText = 0,
        ConnectedText = 1,
        DisconnectedText = 2,
        ConnectionCancelledText = 3,
        ConnectionErrorText = 4,
        ServerPingPongText = 5,
        WelcomeText = 6,
        ChannelTopic = 100,
        ChannelTopicSet = 101,
        ChannelTopicChange = 102,
        ChannelText = 103,
        ChannelSelfText = 104,
        ChannelActionText = 105,
        ChannelSelfActionText = 106,
        PrivateText = 107,
        PrivateSelfText = 108,
        PrivateActionText = 109,
        PrivateSelfActionText = 110,        
        NoticeText = 111,
        NoticeSelfText = 112,
        ChannelJoinText = 113,
        ChannelSelfJoinText = 114,
        ChannelPartText = 115,
        ModeChannelText = 116,        
        ModeSelfText = 117,
        QuitText = 118,
        ChannelKickText = 119,
        ChannelSelfKickText = 120,
        NickChangeUserText = 121,
        NickChangeSelfText = 122,
        MessageTargetText = 123,
        MotdText = 124,
        RawText = 125,
        WallopsText = 126,
        LUsersText = 127,
        InviteText = 128,
        CtcpText = 129,
        CtcpSelfText = 130,
        CtcpReplyText = 131
    }

    public class IncomingMessageData
    {
        public ThemeMessage Message { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Nick { get; set; }
        public string Prefix { get; set; }
        public string Address { get; set; }
        public string Target { get; set; }
        public string Text { get; set; }
        public string NewNick { get; set; }
        public string KickedNick { get; set; } 

        /* Server related properties */
        public string Server { get; set; }
        public int Port { get; set; }
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
            Load(themeFile, ref CurrentTheme, false);
        }

        public static void Load(string themeFile, ref Theme theme, bool isPreview)
        {
            if (!BinarySerialize<Theme>.Load(themeFile, ref theme))
            {
                theme = new Theme();
            }
            if (!isPreview && ThemeLoaded != null)
            {
                ThemeLoaded();
            }
        }

        public static void Save(string themeFile)
        {            
            Save(themeFile, CurrentTheme);
        }

        public static void Save(string themeFile, Theme theme)
        {
            if (!theme.ThemeChanged)
            {
                return;
            }
            theme.ThemeChanged = false;
            BinarySerialize<Theme>.Save(themeFile, theme);
        }

        /* Accessible theme properties */
        public static Font GetFont(ChildWindowType window)
        {
            return GetFont(window, CurrentTheme);
        }

        public static Font GetFont(ChildWindowType window, Theme theme)
        {
            return !theme.ThemeFonts.ContainsKey(window) ? new Font("Lucida Console", 10) : theme.ThemeFonts[window];
        }

        public static Theme.ThemeBackgroundData GetBackground(ChildWindowType window)
        {
            return GetBackground(window, CurrentTheme);
        }

        public static Theme.ThemeBackgroundData GetBackground(ChildWindowType window, Theme theme)
        {
            return !theme.ThemeBackgrounds.ContainsKey(window) ? null : theme.ThemeBackgrounds[window];
        }

        public static Color GetColor(ThemeColor color)
        {
            return GetColor(color, CurrentTheme);
        }

        public static Color GetColor(ThemeColor color, Theme theme)
        {
            return !theme.ThemeColors.ContainsKey(color) ? theme.Colors[0] : theme.Colors[theme.ThemeColors[color]];
        }

        /* The main theme message parser */
        public static ParsedMessageData ParseMessage(IncomingMessageData messageData)
        {
            return ParseMessage(messageData, CurrentTheme);
        }

        public static ParsedMessageData ParseMessage(IncomingMessageData messageData, Theme theme)
        {
            var pmd = new ParsedMessageData
                          {
                              DefaultColor = 1,
                              Message = "[Theme file corrupt/missing data]"
                          };
            if (!theme.Messages.ContainsKey(messageData.Message))
            {
                return pmd;
            }
            /* This code is kind of ugly... */            
            var sb = new StringBuilder(theme.Messages[messageData.Message].MessageFormat);
            sb.Replace("$ts", TimeFunctions.FormatTimeStamp(messageData.TimeStamp, theme.TimeStampFormat));
            sb.Replace("$nick", messageData.Nick);
            sb.Replace("$prefix", messageData.Prefix);
            sb.Replace("$address", messageData.Address);
            sb.Replace("$me", messageData.Nick);
            sb.Replace("$newnick", messageData.NewNick);
            sb.Replace("$knick", messageData.KickedNick);
            sb.Replace("$text", messageData.Text);
            sb.Replace("$chan", messageData.Target);
            sb.Replace("$ctcp", messageData.Target);
            sb.Replace("$target", messageData.Target);
            sb.Replace("$server", messageData.Server);
            sb.Replace("$port", messageData.Port.ToString());
            pmd.DefaultColor = theme.Messages[messageData.Message].DefaultColor;
            pmd.Message = sb.ToString();
            return pmd;
        }
    }
}