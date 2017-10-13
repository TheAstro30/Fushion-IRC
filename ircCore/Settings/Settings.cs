/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ircCore.Settings
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

        [XmlElement("themes")]
        public SettingsTheme Themes = new SettingsTheme();        

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
            Windows.NicklistWidth = 112;
            var w = new WindowData { Name = "application", Size = new Size(1100, 750), Position = new Point(55, 55) };
            Windows.Window.Add(w);
            w = new WindowData {Name = "console", Size = new Size(731, 255), Position = new Point(-1, -1)};
            Windows.Window.Add(w);
            /* Caching defaults */
            Windows.Caching.Output = 500;
            Windows.Caching.Input = 50;
            Windows.Caching.ChatSearch = 25;
            /* Identd */
            Connection.Identd.System = "UNIX";
            Connection.Identd.Port = 113;
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
            /* Create a blank theme */
            Themes.Theme.Add(new SettingsTheme.ThemeListData {Name = "Default", Path = @"\themes\default.thm"});            
        }
    }
}
