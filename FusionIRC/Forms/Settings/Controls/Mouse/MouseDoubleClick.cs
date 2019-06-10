/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;

namespace FusionIRC.Forms.Settings.Controls.Mouse
{
    public partial class MouseDoubleClick : BaseControlRenderer, ISettings
    {
        public event Action<ISettings> OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public MouseDoubleClick()
        {
            InitializeComponent();

            Header = "Double-Click";

            txtConsole.Text = SettingsManager.Settings.Mouse.Console;
            txtChannel.Text = SettingsManager.Settings.Mouse.Channel;
            txtQuery.Text = SettingsManager.Settings.Mouse.Query;
            txtNicklist.Text = SettingsManager.Settings.Mouse.Nicklist;

            txtConsole.TextChanged += ControlsChanged;
            txtChannel.TextChanged += ControlsChanged;
            txtQuery.TextChanged += ControlsChanged;
            txtNicklist.TextChanged += ControlsChanged;
        }
        
        public void SaveSettings()
        {
            SettingsManager.Settings.Mouse.Console = txtConsole.Text;
            SettingsManager.Settings.Mouse.Channel = txtChannel.Text;
            SettingsManager.Settings.Mouse.Query = txtQuery.Text;
            SettingsManager.Settings.Mouse.Nicklist = txtNicklist.Text;
            SettingsChanged = false;
        }

        /* Callback */
        private void ControlsChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }
    }
}
