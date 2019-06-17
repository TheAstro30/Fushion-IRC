/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using FusionIRC.Forms.Theming.Helpers;
using ircCore.Settings.Theming;
using ircCore.Settings.Theming.Structures;
using ircCore.Utils;
using ircCore.Utils.DirectX;
using libolv;

namespace FusionIRC.Forms.Theming.Controls
{
    public partial class ThemeSounds : UserControl, IThemeSetting
    {
        private readonly OlvColumn _colEvent;
        private readonly OlvColumn _colFile;

        private object _sound;

        public Theme CurrentTheme { get; set; }

        public event Action ThemeChanged;

        public ThemeSounds(Theme theme)
        {
            InitializeComponent();

            CurrentTheme = theme;

            _colEvent = new OlvColumn("Event:", "Name")
                            {
                                CellPadding = null,
                                IsEditable = false,
                                Sortable = false,
                                Width = 120
                            };

            _colFile = new OlvColumn("Sound:", "SoundPathString")
                           {
                               CellPadding = null,
                               IsEditable = false,
                               Sortable = false,
                               Width = 120,
                               FillsFreeSpace = true
                           };
            lvSound.AllColumns.AddRange(new[] {_colEvent, _colFile});
            lvSound.Columns.AddRange(new ColumnHeader[] {_colEvent, _colFile});
            lvSound.RebuildColumns();
            /* Import sound list */
            lvSound.SetObjects(theme.ThemeSounds);
            /* No real nice way of doing this ... */
            foreach (var s in from object s in lvSound.Objects where ((ThemeSoundData)s).Enabled select s)
            {
                lvSound.CheckObject(s);
            }
            /* Handlers */
            lvSound.ItemChecked += ListCheckChanged;            
            lvSound.SelectedIndexChanged += ListSelectedIndexChanged;
            btnTest.Click += ButtonClickHandler;
            btnStop.Click += ButtonClickHandler;
            btnDefault.Click += ButtonClickHandler;
            btnSelect.Click += ButtonClickHandler;
            btnNone.Click += ButtonClickHandler;
        }
                
        public void SaveSettings()
        {
            /* Again, no real nice way of doing this... */
            for (var i = 0; i <= lvSound.Items.Count - 1; i++)
            {
                CurrentTheme.ThemeSounds[i].Enabled = lvSound.Items[i].Checked;
            }            
        }

        /* Callbacks */
        private void ListCheckChanged(object sender, EventArgs e)
        {
            /* Penis... */
            if (ThemeChanged != null)
            {
                ThemeChanged();
            }
        }

        private void ListSelectedIndexChanged(object sender, EventArgs e)
        {
            var enable = lvSound.SelectedObject != null;
            btnTest.Enabled = enable;
            btnStop.Enabled = enable;
            btnDefault.Enabled = enable;
            btnSelect.Enabled = enable;
            btnNone.Enabled = enable;
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            var s = (ThemeSoundData) lvSound.SelectedObject;
            switch (btn.Text.ToUpper())
            {
                case "TEST":
                    _sound = ThemeManager.PlaySound(s);
                    return;

                case "STOP":
                    if (_sound == null)
                    {
                        return;
                    }
                    if (_sound is Sound)
                    {
                        ((Sound)_sound).Stop();
                    }
                    else if (_sound is SoundPlayer)
                    {
                        ((SoundPlayer)_sound).Stop();
                    }
                    return;

                case "DEFAULT":
                    s.SoundPath = string.Empty;
                    s.Type = ThemeSoundType.Default;
                    break;

                case "SELECT":
                    using (var ofd = new OpenFileDialog { Title = @"Select a sound file for current event:", Filter = @"All Supported Sound Files|*.mp3;*.wav|MP3 Files (*.mp3)|*.mp3|WAV Files (*.wav)|*.wav" })
                    {
                        if (ofd.ShowDialog(this) == DialogResult.Cancel)
                        {
                            return;
                        }
                        s.SoundPath = Functions.MainDir(ofd.FileName);
                        s.Type = ThemeSoundType.User;
                    }                    
                    break;

                case "NONE":
                    s.SoundPath = string.Empty;
                    s.Type = ThemeSoundType.None;                    
                    break;
            }
            lvSound.RefreshObject(s);
            if (ThemeChanged != null)
            {
                ThemeChanged();
            }
        }
    }
}
