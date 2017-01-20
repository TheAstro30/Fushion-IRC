﻿/* FusionIRC IRC Client
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
    
        [XmlElement("connection")]
        public SettingsConnection Connection = new SettingsConnection();

        [XmlElement("windows")]
        public SettingsWindow Windows = new SettingsWindow();        

        [XmlElement("themes")]
        public SettingsTheme Themes = new SettingsTheme();

        [XmlElement("search")]
        public SettingsFind Search = new SettingsFind();

        [XmlElement("caching")]
        public SettingsCaching Caching = new SettingsCaching();

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
            /* Create a blank theme */
            Themes.Theme.Add(new SettingsTheme.ThemeListData {Name = "Default", Path = "default.thm"});
            /* Caching defaults */
            Caching.Output = 500;
            Caching.Input = 50;
            Caching.ChatSearch = 25;
        }
    }
}
