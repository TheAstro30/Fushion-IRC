/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using FusionIRC.Forms.Settings.Controls.Base;
using ircCore.Settings;

namespace FusionIRC.Forms.Settings.Controls.Client
{
    public partial class ClientCaching : BaseControlRenderer, ISettings
    {
        public event Action<ISettings> OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ClientCaching()
        {
            InitializeComponent();

            Header = "Caching";

            txtText.Text = SettingsManager.Settings.Windows.Caching.Output.ToString();
            txtInput.Text = SettingsManager.Settings.Windows.Caching.Input.ToString();
            txtChat.Text = SettingsManager.Settings.Windows.Caching.ChatSearch.ToString();

            txtText.TextChanged += ControlsChanged;
            txtInput.TextChanged += ControlsChanged;
            txtChat.TextChanged += ControlsChanged;
        }
       
        public void SaveSettings()
        {
            int i;
            if (!int.TryParse(txtText.Text, out i))
            {
                i = 500;
            }
            SettingsManager.Settings.Windows.Caching.Output = i;
            if (!int.TryParse(txtInput.Text, out i))
            {
                i = 50;
            }
            SettingsManager.Settings.Windows.Caching.Input = i;
            if (!int.TryParse(txtChat.Text, out i))
            {
                i = 25;
            }
            SettingsManager.Settings.Windows.Caching.ChatSearch = i;
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
