/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using ircCore.Settings.Theming.Structures;

namespace ircCore.Settings.Theming
{   
    [Serializable]
    public class Theme
    {
        /* Binary serialized class of all output display data and color schemes */        
        private const string Rgb = "#FFFFFF,#000000,#00007F,#009300,#FF0000,#7F0000,#9C009C,#FC7F00,#FFFF00,#00FC00,#009393,#00FFFF,#0000FC,#FF00FF,#7F7F7F,#D2D2D2";
        private const string DefaultColors = "0,15,0,1,0,1,0,1";

        /* Public properties */
        public bool ThemeChanged { get; set; }

        public string Name { get; set; }

        public Color[] Colors { get; set; }

        public string TimeStampFormat { get; set; }

        public Dictionary<ThemeColor, int> ThemeColors = new Dictionary<ThemeColor, int>();

        public Dictionary<ChildWindowType, Font> ThemeFonts = new Dictionary<ChildWindowType, Font>();

        public Dictionary<ChildWindowType, ThemeBackgroundData> ThemeBackgrounds = new Dictionary<ChildWindowType, ThemeBackgroundData>();

        public Dictionary<ThemeMessage, ThemeMessageData> Messages = new Dictionary<ThemeMessage, ThemeMessageData>();

        public Dictionary<ThemeNicklistImage, Bitmap> NicklistImages = new Dictionary<ThemeNicklistImage, Bitmap>();

        public List<ThemeSoundData> ThemeSounds = new List<ThemeSoundData>(); 
        
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
            ThemeFonts.Add(ChildWindowType.Console, new Font("Lucida Console", 10));
            ThemeFonts.Add(ChildWindowType.Channel, new Font("Lucida Console", 10));
            ThemeFonts.Add(ChildWindowType.Private, new Font("Lucida Console", 10));
            ThemeFonts.Add(ChildWindowType.DccChat, new Font("Lucida Console", 10));
            /* Default backgrounds */
            ThemeBackgrounds.Add(ChildWindowType.Console, new ThemeBackgroundData());
            ThemeBackgrounds.Add(ChildWindowType.Channel, new ThemeBackgroundData());
            ThemeBackgrounds.Add(ChildWindowType.Private, new ThemeBackgroundData());
            ThemeBackgrounds.Add(ChildWindowType.DccChat, new ThemeBackgroundData());
            /* Default messages */            
            Messages.Add(ThemeMessage.ConnectingText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * Connecting to: $server [$port]" });
            Messages.Add(ThemeMessage.ConnectedText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * Connected! Waiting for welcome message..." });
            Messages.Add(ThemeMessage.ConnectionCancelledText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * Connection cancelled" });
            Messages.Add(ThemeMessage.DisconnectedText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * Disconnected" });
            Messages.Add(ThemeMessage.ConnectionErrorText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * Error connecting to: $server ($text)" });
            Messages.Add(ThemeMessage.ServerPingPongText, new ThemeMessageData { DefaultColor = 3, Message = "$ts * Server request PING; client response PONG!" });
            Messages.Add(ThemeMessage.ChannelProperties, new ThemeMessageData { DefaultColor = 3, Message = "$ts * Looking up $chan properties..." });
            Messages.Add(ThemeMessage.ChannelTopic, new ThemeMessageData { DefaultColor = 3, Message = "$ts * Topic is: '$text'" });
            Messages.Add(ThemeMessage.ChannelTopicSet, new ThemeMessageData { DefaultColor = 3, Message = "$ts * Topic set by: $text" });
            Messages.Add(ThemeMessage.ChannelTopicChange, new ThemeMessageData { DefaultColor = 3, Message = "$ts * $nick changes $target topic to '$text'" });
            Messages.Add(ThemeMessage.ChannelText, new ThemeMessageData {DefaultColor = 1, Message = "$ts <$prefix$nick> $text"});
            Messages.Add(ThemeMessage.ChannelSelfText, new ThemeMessageData { DefaultColor = 1, Message = "$ts <12$prefix$me> $text" });
            Messages.Add(ThemeMessage.ChannelActionText, new ThemeMessageData { DefaultColor = 6, Message = "$ts * $prefix$nick $text" });
            Messages.Add(ThemeMessage.ChannelSelfActionText, new ThemeMessageData { DefaultColor = 13, Message = "$ts * $prefix$me 6$text" });
            Messages.Add(ThemeMessage.ChannelJoinText, new ThemeMessageData { DefaultColor = 3, Message = "$ts * $nick ($address) has joined: $target" });
            Messages.Add(ThemeMessage.ChannelSelfJoinText, new ThemeMessageData { DefaultColor = 3, Message = "$ts * Now talking in: $target" });
            Messages.Add(ThemeMessage.ChannelPartText, new ThemeMessageData { DefaultColor = 3, Message = "$ts * $nick ($address) has left: $target" });
            Messages.Add(ThemeMessage.ChannelPartTextMessage, new ThemeMessageData { DefaultColor = 3, Message = "$ts * $nick ($address) has left: $target '$text'" });
            Messages.Add(ThemeMessage.QuitText, new ThemeMessageData { DefaultColor = 4, Message = "$ts * $nick ($address) has quit IRC ($text)" });
            Messages.Add(ThemeMessage.ChannelKickText, new ThemeMessageData { DefaultColor = 4, Message = "$ts * $knick was kicked from $target by $nick '$text'" });
            Messages.Add(ThemeMessage.ChannelSelfKickText, new ThemeMessageData { DefaultColor = 4, Message = "$ts * You were kicked from $target by $nick '$text'" });
            Messages.Add(ThemeMessage.ModeChannelText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * $nick sets mode: $text" });            
            Messages.Add(ThemeMessage.ModeSelfText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * $me sets mode: $text" });
            Messages.Add(ThemeMessage.NickChangeUserText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * $nick is now known as: $newnick" });
            Messages.Add(ThemeMessage.NickChangeSelfText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * You are now known as: $newnick" });
            Messages.Add(ThemeMessage.PrivateText, new ThemeMessageData { DefaultColor = 1, Message = "$ts <$nick> $text" });
            Messages.Add(ThemeMessage.PrivateSelfText, new ThemeMessageData { DefaultColor = 1, Message = "$ts <12$me> $text" });
            Messages.Add(ThemeMessage.PrivateActionText, new ThemeMessageData { DefaultColor = 6, Message = "$ts * $nick $text" });
            Messages.Add(ThemeMessage.PrivateSelfActionText, new ThemeMessageData { DefaultColor = 13, Message = "$ts * $me 6$text" });
            Messages.Add(ThemeMessage.MessageTargetText, new ThemeMessageData { DefaultColor = 1, Message = "$ts -> *$target* $text" });            
            Messages.Add(ThemeMessage.NoticeText, new ThemeMessageData { DefaultColor = 5, Message = "$ts -$nick- $text" });
            Messages.Add(ThemeMessage.NoticeSelfText, new ThemeMessageData { DefaultColor = 1, Message = "$ts -> -$target- $text" });
            Messages.Add(ThemeMessage.MotdText, new ThemeMessageData { DefaultColor = 1, Message = "$ts $text" });
            Messages.Add(ThemeMessage.WelcomeText, new ThemeMessageData { DefaultColor = 1, Message = "$ts * $text" });
            Messages.Add(ThemeMessage.RawText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * $text" });
            Messages.Add(ThemeMessage.WallopsText, new ThemeMessageData { DefaultColor = 6, Message = "$ts !$nick! $text" });
            Messages.Add(ThemeMessage.LUsersText, new ThemeMessageData { DefaultColor = 3, Message = "$ts * $text" });
            Messages.Add(ThemeMessage.InviteText, new ThemeMessageData { DefaultColor = 3, Message = "$ts * $nick has invited you to join $chan" });
            Messages.Add(ThemeMessage.CtcpText, new ThemeMessageData { DefaultColor = 4, Message = "$ts [$nick ($address)] $ctcp" });
            Messages.Add(ThemeMessage.CtcpSelfText, new ThemeMessageData { DefaultColor = 4, Message = "$ts -> [$nick] $ctcp" });
            Messages.Add(ThemeMessage.CtcpReplyText, new ThemeMessageData { DefaultColor = 4, Message = "$ts [$nick ($address) $ctcp reply]: $text" });
            Messages.Add(ThemeMessage.LocalInfoReplyText, new ThemeMessageData { DefaultColor = 6, Message = "$ts Local IP: $dnsip ($dnshost)" });
            Messages.Add(ThemeMessage.DnsText, new ThemeMessageData { DefaultColor = 6, Message = "$ts * DNS Look-up: $text" });
            Messages.Add(ThemeMessage.DnsLookupReplyText, new ThemeMessageData { DefaultColor = 6, Message = "$ts * DNS Look-up: $dnsip ($dnshost)" });
            Messages.Add(ThemeMessage.InfoText, new ThemeMessageData { DefaultColor = 2, Message = "$ts * $text" });
            Messages.Add(ThemeMessage.EchoText, new ThemeMessageData { DefaultColor = 1, Message = "$text" });
            Messages.Add(ThemeMessage.DccChatConnectingText, new ThemeMessageData { DefaultColor = 3, Message = "$ts * DCC chat with $nick - establishing connection..." });
            Messages.Add(ThemeMessage.DccChatConnectedText, new ThemeMessageData { DefaultColor = 3, Message = "$ts * Connection established" });
            Messages.Add(ThemeMessage.DccChatDisconnectedText, new ThemeMessageData { DefaultColor = 4, Message = "$ts * DCC session closed." });
            Messages.Add(ThemeMessage.DccChatConnectionErrorText, new ThemeMessageData { DefaultColor = 4, Message = "$ts * DCC connection error ($text)" });
            /* Image list... */
            NicklistImages.Add(ThemeNicklistImage.Owner, Properties.Resources.owner.ToBitmap());
            NicklistImages.Add(ThemeNicklistImage.Protected, Properties.Resources.prot.ToBitmap());
            NicklistImages.Add(ThemeNicklistImage.Operator, Properties.Resources.op.ToBitmap());
            NicklistImages.Add(ThemeNicklistImage.HalfOperator, Properties.Resources.halfop.ToBitmap());
            NicklistImages.Add(ThemeNicklistImage.Voice, Properties.Resources.voice.ToBitmap());
            /* Theme sounds */
            ThemeSounds.AddRange(new[]
                                     {
                                         new ThemeSoundData
                                             {
                                                 ThemeSound = ThemeSound.Connect,
                                                 Type = ThemeSoundType.None,
                                                 Enabled = false
                                             },
                                         new ThemeSoundData
                                             {
                                                 ThemeSound = ThemeSound.Disconnect,
                                                 Type = ThemeSoundType.None,
                                                 Enabled = false
                                             },
                                         new ThemeSoundData
                                             {
                                                 ThemeSound = ThemeSound.PrivateMessage,
                                                 Type = ThemeSoundType.Default,
                                                 Enabled = true
                                             }
                                     });      
            /* Set this flag to true to ensure it will be saved if it doesn't exist (ie: load fails) */
            ThemeChanged = true;
        }        

        public Theme(Theme theme)
        {
            /* Copy constructor */
            Colors = new Color[16];
            for (var i = 0; i <= theme.Colors.Length - 1; i++)
            {
                Colors[i] = theme.Colors[i];
            }
            ThemeColors = new Dictionary<ThemeColor, int>(theme.ThemeColors);
            TimeStampFormat = theme.TimeStampFormat;
            ThemeFonts = new Dictionary<ChildWindowType, Font>(theme.ThemeFonts);
            ThemeBackgrounds = new Dictionary<ChildWindowType, ThemeBackgroundData>(theme.ThemeBackgrounds);
            Messages = new Dictionary<ThemeMessage, ThemeMessageData>(theme.Messages);
            NicklistImages = new Dictionary<ThemeNicklistImage, Bitmap>(theme.NicklistImages);
            ThemeSounds = new List<ThemeSoundData>(theme.ThemeSounds);
        }
    }
}
