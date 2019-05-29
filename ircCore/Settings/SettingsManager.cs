/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Settings
{
    public static class SettingsManager
    {
        public static SettingsBase.Settings Settings = new SettingsBase.Settings();

        public static void Load()
        {
            /* Load settings from disk */
            if (!XmlSerialize<SettingsBase.Settings>.Load(Functions.MainDir(@"\data\settings.xml"), ref Settings))
            {
                Settings = new SettingsBase.Settings();
            }
        }

        public static void Save()
        {
            /* Save settings to disk */
            XmlSerialize<SettingsBase.Settings>.Save(Functions.MainDir(@"\data\settings.xml"), Settings);
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

        public static void CreatePopupList()
        {
            Settings.Scripts.Popups.Clear();
            Settings.Scripts.Popups.AddRange(new[]
                                                 {
                                                     new SettingsScripts.SettingsScriptPath
                                                         {
                                                             Path = @"\scripts\menubar.xml",
                                                             Type = PopupType.Commands
                                                         },
                                                     new SettingsScripts.SettingsScriptPath
                                                         {
                                                             Path = @"\scripts\console.xml",
                                                             Type = PopupType.Console
                                                         },
                                                     new SettingsScripts.SettingsScriptPath
                                                         {
                                                             Path = @"\scripts\channel.xml",
                                                             Type = PopupType.Channel
                                                         },
                                                     new SettingsScripts.SettingsScriptPath
                                                         {
                                                             Path = @"\scripts\nicklist.xml",
                                                             Type = PopupType.Nicklist
                                                         },
                                                     new SettingsScripts.SettingsScriptPath
                                                         {
                                                             Path = @"\scripts\private.xml",
                                                             Type = PopupType.Private
                                                         },
                                                     new SettingsScripts.SettingsScriptPath
                                                         {
                                                             Path = @"\scripts\dcc-chat.xml",
                                                             Type = PopupType.DccChat
                                                         }
                                                 });
        }
    }
}
