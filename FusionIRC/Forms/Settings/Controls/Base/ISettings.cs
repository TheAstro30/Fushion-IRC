/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;

namespace FusionIRC.Forms.Settings.Controls.Base
{
    public interface ISettings
    {
        event Action<ISettings> OnSettingsChanged;

        bool SettingsChanged { get; set; }

        void SaveSettings();
    }
}
