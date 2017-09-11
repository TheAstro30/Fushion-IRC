/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Settings
{
    public static class SettingsManager
    {
        public static Settings Settings = new Settings();

        public static void Load()
        {
            /* Load settings from disk */
            if (!XmlSerialize<Settings>.Load(Functions.MainDir(@"\data\settings.xml", false), ref Settings))
            {
                Settings = new Settings();
            }
        }

        public static void Save()
        {
            /* Save settings to disk */
            XmlSerialize<Settings>.Save(Functions.MainDir(@"\data\settings.xml", false), Settings);
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

        public static AppearanceData.ControlBarData.ControlData GetControlStyle(string name)
        {
            var d = Settings.Client.Appearance.ControlBars.Control.FirstOrDefault(o => o.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (d == null)
            {
                d = new AppearanceData.ControlBarData.ControlData { Name = name.ToLower(), Dock = DockStyle.Top, Visible = true };
                Settings.Client.Appearance.ControlBars.Control.Add(d);
            }
            return d;
        }
    }
}
