/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Drawing;
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
            var w = Settings.Windows.Window.FirstOrDefault(o => o.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (w == null)
            {                
                w = new WindowData {Name = name.ToLower(), Position = new Point(-1, -1)}; /* -1 means default position */
                Settings.Windows.Window.Add(w);
            }
            return w;
        }
    }
}
