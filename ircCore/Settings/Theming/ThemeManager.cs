/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using ircCore.Properties;
using ircCore.Settings.Theming.Structures;
using ircCore.Utils;
using ircCore.Utils.DirectX;
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
        DccChat = 3,

        [Description("Channel List")]
        ChanList = 4
    }

    public enum ThemeColor
    {
        [Description("Text Output Backcolor")]
        OutputWindowBackColor = 0,

        [Description("Text Output Line Maker Color")]
        OutputWindowLineMarkerColor = 1,

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

        [Description("Channel Properties")]
        ChannelProperties = 8,

        [Description("Channel Topic Set By")]
        ChannelTopicSet = 9,

        [Description("Channel Topic Changed")]
        ChannelTopicChange =10,

        [Description("Channel Message")]
        ChannelText = 11,

        [Description("Channel Message Self")]
        ChannelSelfText = 12,

        [Description("Channel Action")]
        ChannelActionText = 13,

        [Description("Channel Action Self")]
        ChannelSelfActionText = 14,

        [Description("Private Message")]
        PrivateText = 15,

        [Description("Private Message Self")]
        PrivateSelfText = 16,

        [Description("Private Action")]
        PrivateActionText = 17,

        [Description("Private Action Self")]
        PrivateSelfActionText = 18,

        [Description("Notice Message")]
        NoticeText = 19,

        [Description("Notice Message Self")]
        NoticeSelfText = 20,

        [Description("Channel Join")]
        ChannelJoinText = 21,

        [Description("Channel Join Self")]
        ChannelSelfJoinText = 22,

        [Description("Channel Part")]
        ChannelPartText = 23,

        [Description("Channel Part (With Message)")]
        ChannelPartTextMessage = 24,

        [Description("Mode Text")]
        ModeChannelText = 25,

        [Description("Mode Self Text")]
        ModeSelfText = 26,

        [Description("Quit Message")]
        QuitText = 27,

        [Description("Channel Kick Message")]
        ChannelKickText = 28,

        [Description("Channel Kick Message Self")]
        ChannelSelfKickText = 29,

        [Description("Nick Change")]
        NickChangeUserText = 30,

        [Description("Nick Change Self")]
        NickChangeSelfText = 31,

        [Description("Target Message Text")]
        MessageTargetText = 32,

        [Description("MOTD Text")]
        MotdText = 33,

        [Description("RAW Text")]
        RawText = 34,

        [Description("Wallops Text")]
        WallopsText = 35,

        [Description("Local Users Text")]
        LUsersText = 36,

        [Description("Invite Text")]
        InviteText = 37,

        [Description("CTCP Text")]
        CtcpText = 38,

        [Description("CTCP Text Self")]
        CtcpSelfText = 39,

        [Description("CTCP Reply")]
        CtcpReplyText = 40,

        [Description("Local Info Reply")]
        LocalInfoReplyText = 41,

        [Description("DNS Text")]
        DnsText = 42,

        [Description("DNS Look-up Reply")]
        DnsLookupReplyText = 43,

        [Description("Info Text")]
        InfoText = 44,

        [Description("Normal Text")]
        EchoText = 45,

        [Description("DCC Chat Connecting Text")]
        DccChatConnectingText = 46,

        [Description("DCC Chat Connected Text")]
        DccChatConnectedText = 47,

        [Description("DCC Chat Disconnected Text")]
        DccChatDisconnectedText = 48,

        [Description("DCC Chat Connection Error Text")]
        DccChatConnectionErrorText = 49
    }

    public enum ThemeNicklistImage
    {
        [Description("Owner (!,~, .)")]
        Owner = 0,

        [Description("Protected Operator (&)")]
        Protected = 1,

        [Description("Operator (@)")]
        Operator = 2,

        [Description("Half Operator (%)")]
        HalfOperator = 3,

        [Description("Voiced (+)")]
        Voice = 4
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

        public static ThemeBackgroundData GetBackground(ChildWindowType window)
        {
            return GetBackground(window, CurrentTheme);
        }

        public static ThemeBackgroundData GetBackground(ChildWindowType window, Theme theme)
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

        public static void PlaySound(ThemeSound sound)
        {
            PlaySound(sound, CurrentTheme);
        }

        public static void PlaySound(ThemeSound sound, Theme theme)
        {
            var d = theme.ThemeSounds.FirstOrDefault(s => s.ThemeSound == sound);
            if (d == null || !d.Enabled)
            {
                return;
            }
            PlaySound(d);                      
        }

        public static object PlaySound(ThemeSoundData sound)
        {
            switch (sound.Type)
            {
                case ThemeSoundType.None:
                    return null;

                case ThemeSoundType.Default:
                    /* Play internal notification sound */
                    var s = new SoundPlayer(Resources.notification);
                    s.Play();
                    return s;

                case ThemeSoundType.User:
                    var path = Functions.MainDir(sound.SoundPath);
                    if (!File.Exists(path))
                    {
                        return null;
                    }
                    var dx = new Sound(path) {Volume = 100};
                    dx.PlayAsync();
                    return dx;
            }
            return null;
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
        public static ThemeMessageData ParseMessage(IncomingMessageData messageData)
        {
            return ParseMessage(messageData, CurrentTheme);
        }

        public static ThemeMessageData ParseMessage(IncomingMessageData messageData, Theme theme)
        {
            var pmd = new ThemeMessageData
                          {
                              DefaultColor = 1,
                              Message = "[Theme file corrupt/missing data]"
                          };
            if (!theme.Messages.ContainsKey(messageData.Message))
            {
                return pmd;
            }
            /* This code is kind of ugly... */            
            var sb = new StringBuilder(theme.Messages[messageData.Message].Message);
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