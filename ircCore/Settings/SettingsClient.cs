/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using ircCore.Utils;

namespace ircCore.Settings
{
    [Serializable]
    public class SettingsClient
    {
        [XmlElement("appearance")]
        public AppearanceData Appearance = new AppearanceData();

        [XmlElement("confirmation")]
        public SettingsConfirmation Confirmation = new SettingsConfirmation();

        [XmlElement("tabs")]
        public SettingsTabs Tabs = new SettingsTabs();
    }

    [Serializable]
    public class AppearanceData
    {
        public class ControlBarData
        {
            public class ControlData
            {
                [XmlAttribute("name")]
                public string Name { get; set; }

                [XmlAttribute("visible")]
                public bool Visible { get; set; }

                [XmlAttribute("position")]
                public string PositionString
                {
                    get { return XmlFormatting.WritePointFormat(Position); }
                    set { Position = XmlFormatting.ParsePointFormat(value); }
                }

                [XmlIgnore]
                public Point Position { get; set; }

                [XmlAttribute("dock")]
                public DockStyle Dock { get; set; }
            }

            [XmlElement("control")]
            public List<ControlData> Control = new List<ControlData>();
        }

        [XmlElement("controlBars")]
        public ControlBarData ControlBars = new ControlBarData();
    }
}
