/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Xml.Serialization;

namespace ircCore.Settings
{
    [Serializable, XmlRoot("settings")]
    public class Settings
    {        
        [XmlElement("userInfo")]
        public SettingsUserInfo UserInfo = new SettingsUserInfo();

        [XmlElement("windows")]
        public SettingsWindow SettingsWindows = new SettingsWindow();        

        [XmlElement("themes")]
        public SettingsTheme Themes = new SettingsTheme();

        [XmlElement("search")]
        public SettingsFind Search = new SettingsFind();

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
            SettingsWindows.SwitchTreeWidth = 110;
            SettingsWindows.NicklistWidth = 112;
            var w = new WindowData { Name = "application", Size = new Size(1100, 750), Position = new Point(55, 55) };
            SettingsWindows.Window.Add(w);
            /* Create a blank theme */
            Themes.Theme.Add(new SettingsTheme.ThemeListData {Name = "Default", Path = "default.thm"});
        }
    }
}
