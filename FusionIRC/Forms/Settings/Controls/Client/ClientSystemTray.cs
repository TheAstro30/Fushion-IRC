﻿/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FusionIRC.Forms.Settings.Controls.Base;
using FusionIRC.Helpers;
using ircCore.Settings;
using ircCore.Utils;

namespace FusionIRC.Forms.Settings.Controls.Client
{
    public partial class ClientSystemTray : BaseControlRenderer, ISettings
    {
        private Icon _icon;
        private readonly Icon _defaultIcon;

        private string _iconFile;

        public event Action<ISettings> OnSettingsChanged;

        public bool SettingsChanged { get; set; }

        public ClientSystemTray()
        {
            InitializeComponent();

            Header = "System Tray";

            chkAlways.Checked = SettingsManager.Settings.Client.TrayIcon.AlwaysShow;
            chkMinimized.Checked = SettingsManager.Settings.Client.TrayIcon.HideMinimized;
            chkNotification.Checked = SettingsManager.Settings.Client.TrayIcon.ShowNotifications;

            chkNotification.Enabled = chkAlways.Checked || chkMinimized.Checked;

            _iconFile = Functions.MainDir(SettingsManager.Settings.Client.TrayIcon.Icon);
            _defaultIcon = WindowManager.MainForm.Icon;
            _icon = File.Exists(_iconFile)
                        ? Icon.ExtractAssociatedIcon(_iconFile)
                        : null;
            pnlIcon.BackgroundImage = _icon != null
                                          ? _icon.ToBitmap()
                                          : _defaultIcon != null ? _defaultIcon.ToBitmap() : null;
            pnlIcon.BackgroundImageLayout = ImageLayout.Center;

            chkAlways.CheckStateChanged += ControlsChanged;
            chkMinimized.CheckedChanged += ControlsChanged;
            chkNotification.CheckedChanged += ControlsChanged;

            btnDefault.Click += ButtonClickHandler;
            btnSelect.Click += ButtonClickHandler;
        }

        public void SaveSettings()
        {
            SettingsManager.Settings.Client.TrayIcon.AlwaysShow = chkAlways.Checked;
            SettingsManager.Settings.Client.TrayIcon.HideMinimized = chkMinimized.Checked;
            SettingsManager.Settings.Client.TrayIcon.Icon = !string.IsNullOrEmpty(_iconFile)
                                                                ? Functions.MainDir(_iconFile)
                                                                : string.Empty;
            SettingsManager.Settings.Client.TrayIcon.ShowNotifications = chkNotification.Checked;
        }

        /* Callbacks */
        private void ControlsChanged(object sender, EventArgs e)
        {
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
            chkNotification.Enabled = chkAlways.Checked || chkMinimized.Checked;
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Text.ToUpper())
            {
                case "DEFAULT":
                    _iconFile = string.Empty;
                    _icon = null;
                    break;

                case "SELECT":
                    using (var d = new OpenFileDialog
                                       {
                                           Title = @"Select an icon file",
                                           Filter = @"Icon Files (*.ico)|*.ico"                                           
                                       }
                        )
                    {
                        if (d.ShowDialog(this) == DialogResult.Cancel)
                        {
                            return;
                        }
                        _iconFile = d.FileName;
                        _icon = File.Exists(_iconFile)
                                    ? Icon.ExtractAssociatedIcon(_iconFile)
                                    : null;
                    }
                    break;
            }
            pnlIcon.BackgroundImage = _icon != null
                                          ? _icon.ToBitmap()
                                          : _defaultIcon != null ? _defaultIcon.ToBitmap() : null;
            SettingsChanged = true;
            if (OnSettingsChanged != null)
            {
                OnSettingsChanged(this);
            }
        }
    }
}
