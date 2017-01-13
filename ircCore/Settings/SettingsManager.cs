/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Linq;
using ircCore.Utils.Serialization;

namespace ircCore.Settings
{
    public static class SettingsManager
    {
        public static Settings Settings = new Settings();

        public static void Load()
        {
            /* Load settings from disk */
            if (!XmlSerialize<Settings>.Load("settings.xml", ref Settings))
            {
                Settings = new Settings();
            }
        }

        public static void Save()
        {
            /* Save settings to disk */
            XmlSerialize<Settings>.Save("settings.xml", Settings);
        }

        public static WindowData GetWindowByName(string name)
        {
            var w = Settings.SettingsWindows.Window.FirstOrDefault(o => o.Name.ToLower() == name.ToLower());
            if (w == null)
            {
                w = new WindowData {Name = name.ToLower()};
                Settings.SettingsWindows.Window.Add(w);
            }
            return w;
        }
    }
}
