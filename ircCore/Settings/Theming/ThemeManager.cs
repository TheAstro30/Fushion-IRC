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
        WallopsText = 126
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
            sb.Replace("$prefix", messageData.Prefix);
            sb.Replace("$address", messageData.Address);
            sb.Replace("$me", messageData.Nick);
            sb.Replace("$newnick", messageData.NewNick);
            sb.Replace("$knick", messageData.KickedNick);
            sb.Replace("$text", messageData.Text);
            sb.Replace("$target", messageData.Target);
            sb.Replace("$server", messageData.Server);
            sb.Replace("$port", messageData.Port.ToString());
            pmd.DefaultColor = CurrentTheme.Messages[messageData.Message].DefaultColor;
            pmd.Message = sb.ToString();
            return pmd;
        }
    }
}
