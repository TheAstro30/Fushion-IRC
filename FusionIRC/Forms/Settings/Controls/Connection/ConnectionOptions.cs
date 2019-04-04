/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms.Settings.Controls.Base;

namespace FusionIRC.Forms.Settings.Controls.Connection
{
    public partial class ConnectionOptions : BaseControlRenderer, ISettings
    {
        public event Action OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ConnectionOptions()
        {
            InitializeComponent();

            Header = "Connection Options";
        }

        public void SaveSettings()
        {
            /* Not implemented */
        }
    }
}
