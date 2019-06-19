/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.SettingsBase.Structures.Client;
using ircCore.Settings.SettingsBase.Structures.Connection;
using ircCore.Settings.SettingsBase.Structures.Dcc;
using ircCore.Settings.SettingsBase.Structures.Misc;
using ircCore.Settings.SettingsBase.Structures.Mouse;

namespace ircCore.Settings.SettingsBase
{
    [Serializable, XmlRoot("settings")]
    public class Settings
    {        
        [XmlElement("userInfo")]
        public SettingsUserInfo UserInfo = new SettingsUserInfo();
    
        [XmlElement("connection")]
        public SettingsConnection Connection = new SettingsConnection();        

        [XmlElement("windows")]
        public SettingsWindow Windows = new SettingsWindow();

        [XmlElement("client")]
        public SettingsClient Client = new SettingsClient();

        [XmlElement("mouse")]
        public SettingsMouse Mouse = new SettingsMouse();

        [XmlElement("dcc")]
        public SettingsDcc Dcc = new SettingsDcc();

        [XmlElement("themes")]
        public SettingsTheme Themes = new SettingsTheme();

        [XmlElement("editor")]
        public SettingsEditor Editor = new SettingsEditor();

        [XmlElement("scripts")]
        public SettingsScripts Scripts = new SettingsScripts(); 

        public Settings()
        {
            /* User info */
            var rand = new Random();
            var info = new SettingsUserInfo
                           {
                               Nick = string.Format("Fusion-{0}", rand.Next(1999, 9999)),
                               Alternative = string.Format("Fusion_{0}", rand.Next(1999, 9999)),
                               Ident = "FusionIRC",
                               RealName = "Fusion IRC Client"
                           };
            UserInfo = info;
            /* Set up basic settings */
            Windows.SwitchTreeWidth = 160;
            Windows.NicklistWidth = 130;
            var w = new WindowData { Name = "application", Size = new Size(1100, 750), Position = new Point(55, 55) };
            Windows.Window.Add(w);
            w = new WindowData {Name = "console", Size = new Size(731, 255), Position = new Point(-1, -1)};
            Windows.Window.Add(w);
            w = new WindowData { Name = "dcc-manager", Size = new Size(613, 301), Position = new Point(200, 183) };
            Windows.Window.Add(w);
            w = new WindowData { Name = "editor", Size = new Size(366, 395), Position = new Point(150, 160) };
            Windows.Window.Add(w);
            /* Caching defaults */
            Windows.Caching.Output = 500;
            Windows.Caching.Input = 50;
            Windows.Caching.ChatSearch = 25;
            /* Editor */
            Editor.SplitSize = 120;
            Editor.SyntaxHighlight = true;
            Editor.LineNumbering = true;
            Editor.Zoom = 100;
            Editor.Colors.KeyWordColor = Color.Blue;
            Editor.Colors.IdentifierColor = Color.DarkCyan;
            Editor.Colors.CustomIdentifierColor = Color.DeepPink;
            Editor.Colors.VariableColor = Color.Red;
            Editor.Colors.CommandColor = Color.BlueViolet;
            Editor.Colors.CommentColor = Color.Green;
            Editor.Colors.MiscColor = Color.Brown;
            /* Connection Options */
            Connection.Options.DefaultPort = 6667;
            Connection.Options.RetryTimes = 99;
            /* Identd */
            Connection.Identd.System = "UNIX";
            Connection.Identd.Port = 113;
            Connection.ShowConnectDialog = true;
            Connection.SslAcceptRequests = true;
            /* Local info */
            Connection.LocalInfo.LookupMethod = LocalInfoLookupMethod.Socket;
            /* Create control styles */
            var d = new AppearanceData.ControlBarData.ControlData
                        {
                            Name = "menubar",
                            Dock = DockStyle.Top,
                            Visible = true
                        };
            Client.Appearance.ControlBars.Control.Add(d);
            d = new AppearanceData.ControlBarData.ControlData
                    {
                        Name = "toolbar",
                        Dock = DockStyle.Top,
                        Position = new Point(0, 25),
                        Visible = true
                    };
            Client.Appearance.ControlBars.Control.Add(d);
            Client.Confirmation.ClientClose = CloseConfirmation.Connected;
            Client.Confirmation.Url = true;
            Client.Confirmation.ConfirmPaste = true;
            Client.Confirmation.PasteLines = 10;
            /* Channel seetings */
            Client.Channels.ShowFavoritesDialogOnConnect = true;
            /* Messages */
            Client.Messages.QuitMessage = "Leaving.";
            Client.Messages.CommandCharacter = "/";
            Client.Messages.LineSpacing = LineSpacingStyle.Paragraph;
            /* Show information */
            Client.Show.PingPong = true;
            Client.Show.Motd = true;
            /* Flash */
            Client.Flash.Channel = true;
            Client.Flash.Private = true;
            Client.Flash.Chat = true;
            /* Logging */
            Client.Logging.LogPath = @"\logs";
            /* Mouse */
            Mouse.Console = "lusers";
            Mouse.Channel = "channel";
            Mouse.Query = "whois $1";
            Mouse.Nicklist = "query $1";
            /* DCC */
            Dcc.Options.General.MinimumPort = 1024;
            Dcc.Options.General.MaximumPort = 5000;
            Dcc.Options.General.PacketSize = 4096;
            Dcc.Options.Timeouts.GetSendRequest = 120;
            Dcc.Options.Timeouts.GetSendTransfer = 120;
            Dcc.Options.Timeouts.ChatConnection = 120;
            Dcc.Options.Filter.FilterMethod = DccFilterMethod.AcceptOnly;
            Dcc.Options.Filter.ShowRejectionDialog = true;
            Dcc.Options.Filter.Extension.AddRange(new[]
                                                      {
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.avi"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.bmp"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.gif"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.log"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.mp2"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.mp3"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.mp4"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.png"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.rar"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.txt"},
                                                          new SettingsDcc.SettingsDccOptions.SettingsDccFilter.
                                                              SettingsDccExtension {Name = "*.zip"}
                                                      });
            /* Channels */
            Client.Channels.ChannelList.Minimum = 3;
            Client.Channels.ChannelList.Minimum = 1000;
            /* Create a blank theme */
            Themes.Theme.Add(new SettingsTheme.ThemeListData {Name = "Default", Path = @"\themes\default.thm"});                        
        }
    }
}
