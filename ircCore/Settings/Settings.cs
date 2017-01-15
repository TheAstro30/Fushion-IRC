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

        public Settings()
        {
            /* Set up basic settings */
            var w = new WindowData { Name = "application", Size = new Size(969, 593), Position = new Point(55, 55) };
            SettingsWindows.Window.Add(w);
            /* Create a blank theme */
            Themes.Theme.Add(new SettingsTheme.ThemeListData {Name = "Default", Path = "default.thm"});
        }
    }
}
