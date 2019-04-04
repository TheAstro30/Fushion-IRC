/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Settings.Theming
{
    public enum ChildWindowType
    {
        [Description("Console")]
        Console = 0,

        [Description("Channel")]
        Channel = 1,

        [Description("Private")]
        Private = 2,

        [Description("Chat")]
        DccChat = 3
    }

    public enum ThemeColor
    {
        [Description("Text Output Backcolor")]
        OutputWindowBackColor = 0,

        [Description("Text Output Forecolor")]
        OutputWindowForeColor = 1,

        [Description("Input Box Backcolor")]
        InputWindowBackColor = 2,

        [Description("Input Box Forecolor")]
        InputWindowForeColor = 3,

        [Description("Nicklist Backcolor")]
        NicklistBackColor = 4,

        [Description("Nicklist Forecolor")]
        NicklistForeColor = 5,

        [Description("Switch Tree Backcolor")]
        SwitchTreeBackColor = 6,

        [Description("Switch Tree Forecolor")]
        SwitchTreeForeColor = 7
    }

    public enum ThemeMessage
    {
        [Description("Connecting")]
        ConnectingText = 0,

        [Description("Connected")]
        ConnectedText = 1,

        [Description("Disconnected")]
        DisconnectedText = 2,

        [Description("Connection Cancelled")]
        ConnectionCancelledText = 3,

        [Description("Connection Error")]
        ConnectionErrorText = 4,

        [Description("Server Ping/Pong")]
        ServerPingPongText = 5,

        [Description("IRC Welcome")]
        WelcomeText = 6,

        [Description("Channel Topic")]
        ChannelTopic = 7,

        [Description("Channel Topic Set By")]
        ChannelTopicSet = 8,

        [Description("Channel Topic Changed")]
        ChannelTopicChange = 9,

        [Description("Channel Message")]
        ChannelText = 10,

        [Description("Channel Message Self")]
        ChannelSelfText = 11,

        [Description("Channel Action")]
        ChannelActionText = 12,

        [Description("Channel Action Self")]
        ChannelSelfActionText = 13,

        [Description("Private Message")]
        PrivateText = 14,

        [Description("Private Message Self")]
        PrivateSelfText = 15,

        [Description("Private Action")]
        PrivateActionText = 16,

        [Description("Private Action Self")]
        PrivateSelfActionText = 17,

        [Description("Notice Message")]
        NoticeText = 18,

        [Description("Notice Message Self")]
        NoticeSelfText = 19,

        [Description("Channel Join")]
        ChannelJoinText = 20,

        [Description("Channel Join Self")]
        ChannelSelfJoinText = 21,

        [Description("Channel Part")]
        ChannelPartText = 22,

        [Description("Mode Text")]
        ModeChannelText = 23,

        [Description("Mode Self Text")]
        ModeSelfText = 24,

        [Description("Quit Message")]
        QuitText = 25,

        [Description("Channel Kick Message")]
        ChannelKickText = 26,

        [Description("Channel Kick Message Self")]
        ChannelSelfKickText = 27,

        [Description("Nick Change")]
        NickChangeUserText = 28,

        [Description("Nick Change Self")]
        NickChangeSelfText = 29,

        [Description("Target Message Text")]
        MessageTargetText = 30,

        [Description("MOTD Text")]
        MotdText = 31,

        [Description("RAW Text")]
        RawText = 32,

        [Description("Wallops Text")]
        WallopsText = 33,

        [Description("Local Users Text")]
        LUsersText = 34,

        [Description("Invite Text")]
        InviteText = 35,

        [Description("CTCP Text")]
        CtcpText = 36,

        [Description("CTCP Text Self")]
        CtcpSelfText = 37,

        [Description("CTCP Reply")]
        CtcpReplyText = 38,

        [Description("Local Info Reply")]
        LocalInfoReplyText = 39,

        [Description("DNS Text")]
        DnsText = 40,

        [Description("DNS Look-up Reply")]
        DnsLookupReplyText = 41,

        [Description("Info Text")]
        InfoText = 42
    }

    public enum ThemeNicklistImage
    {
        [Description("Owner (!,~, .)")]
        Owner = 0,

        [Description("Protected Operator (&)")]
        Protected = 1,

        [Description("Operator (@)")]
        Operator = 2,

        [Description("Half Operator (&)")]
        HalfOperator = 3,

        [Description("Voiced (+)")]
        Voice = 4
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

        /* DNS */
        public string DnsAddress { get; set; }
        public string DnsHost { get; set; }
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

        public static ImageList GetNicklistImages()
        {
            return GetNicklistImages(CurrentTheme);
        }

        public static ImageList GetNicklistImages(Theme theme)
        {
            var images = new ImageList {ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16)};
            foreach (var keyPair in theme.NicklistImages)
            {
                images.Images.Add(keyPair.Value);
            }
            return images;
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
            sb.Replace("$dnsip", messageData.DnsAddress);
            sb.Replace("$dnshost", messageData.DnsHost);
            pmd.DefaultColor = theme.Messages[messageData.Message].DefaultColor;
            pmd.Message = sb.ToString();
            return pmd;
        }
    }
}