/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
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
    public sealed class ThemeSounds : UserControl, IThemeSetting
    {
        private readonly CheckBox _chkEnable;
        private readonly ObjectListView _lvSound;
        private readonly OlvColumn _colEvent;
        private readonly OlvColumn _colFile;
        private readonly Button _btnTest;
        private readonly Button _btnStop;
        private readonly Button _btnDefault;
        private readonly Button _btnSelect;
        private readonly Button _btnNone;        
        
        private object _sound;

        public Theme CurrentTheme { get; set; }

        public event Action ThemeChanged;

        public ThemeSounds(Theme theme)
        {
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Size = new Size(438, 360);

            _chkEnable = new CheckBox
                             {
                                 AutoSize = true,
                                 Location = new Point(3, 3),
                                 Size = new Size(134, 19),
                                 TabIndex = 0,
                                 Text = @"Enable sound events",
                                 UseVisualStyleBackColor = true
                             };

            _lvSound = new ObjectListView
                           {
                               CheckBoxes = true,
                               FullRowSelect = true,
                               HeaderStyle = ColumnHeaderStyle.Nonclickable,
                               HideSelection = false,
                               Location = new Point(3, 28),
                               MultiSelect = false,
                               Size = new Size(351, 329),
                               TabIndex = 1,
                               UseCompatibleStateImageBehavior = false,
                               View = View.Details
                           };

            _colEvent = new OlvColumn("Event:", "Name")
                            {
                                CellPadding = null,
                                IsEditable = false,
                                Sortable = false,
                                Width = 200
                            };

            _colFile = new OlvColumn("Sound:", "SoundPathString")
                           {
                               CellPadding = null,
                               IsEditable = false,
                               Sortable = false,
                               Width = 120,
                               FillsFreeSpace = true
                           };

            _lvSound.AllColumns.AddRange(new[] { _colEvent, _colFile });
            _lvSound.Columns.AddRange(new ColumnHeader[] { _colEvent, _colFile });

            _btnTest = new Button
                           {
                               Enabled = false,
                               Location = new Point(360, 28),
                               Size = new Size(75, 23),
                               TabIndex = 2,
                               Text = @"Test",
                               UseVisualStyleBackColor = true
                           };

            _btnStop = new Button
                           {
                               Enabled = false,
                               Location = new Point(360, 57),
                               Size = new Size(75, 23),
                               TabIndex = 3,
                               Text = @"Stop",
                               UseVisualStyleBackColor = true
                           };

            _btnDefault = new Button
                              {
                                  Enabled = false,
                                  Location = new Point(360, 276),
                                  Size = new Size(75, 23),
                                  TabIndex = 4,
                                  Text = @"Default",
                                  UseVisualStyleBackColor = true
                              };

            _btnSelect = new Button
                             {
                                 Enabled = false,
                                 Location = new Point(360, 305),
                                 Size = new Size(75, 23),
                                 TabIndex = 5,
                                 Text = @"Select",
                                 UseVisualStyleBackColor = true
                             };

            _btnNone = new Button
                           {
                               Enabled = false,
                               Location = new Point(360, 334),
                               Size = new Size(75, 23),
                               TabIndex = 6,
                               Text = @"None",
                               UseVisualStyleBackColor = true
                           };

            Controls.AddRange(new Control[] {_chkEnable, _lvSound, _btnTest, _btnStop, _btnDefault, _btnSelect, _btnNone});
            
            CurrentTheme = theme;
                        
            _chkEnable.Checked = theme.ThemeSounds.Enable;
            /* Import sound list */
            _lvSound.RebuildColumns();
            _lvSound.SetObjects(theme.ThemeSounds.SoundData);
            /* No real nice way of doing this ... */
            foreach (var s in from object s in _lvSound.Objects where ((ThemeSoundData)s).Enabled select s)
            {
                _lvSound.CheckObject(s);
            }
            /* Handlers */
            _chkEnable.CheckedChanged += CheckChanged;
            _lvSound.ItemChecked += CheckChanged;            
            _lvSound.SelectedIndexChanged += ListSelectedIndexChanged;
            _btnTest.Click += ButtonClickHandler;
            _btnStop.Click += ButtonClickHandler;
            _btnDefault.Click += ButtonClickHandler;
            _btnSelect.Click += ButtonClickHandler;
            _btnNone.Click += ButtonClickHandler;
        }
                
        public void SaveSettings()
        {
            CurrentTheme.ThemeSounds.Enable = _chkEnable.Checked;
            /* Again, no real nice way of doing .. */
            for (var i = 0; i <= _lvSound.Items.Count - 1; i++)
            {
                CurrentTheme.ThemeSounds.SoundData[i].Enabled = _lvSound.Items[i].Checked;
            }            
        }

        /* Callbacks */
        private void CheckChanged(object sender, EventArgs e)
        {
            /* Penis... */
            if (ThemeChanged != null)
            {
                ThemeChanged();
            }
        }

        private void ListSelectedIndexChanged(object sender, EventArgs e)
        {
            var enable = _lvSound.SelectedObject != null;
            _btnTest.Enabled = enable;
            _btnStop.Enabled = enable;
            _btnDefault.Enabled = enable;
            _btnSelect.Enabled = enable;
            _btnNone.Enabled = enable;
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            var s = (ThemeSoundData) _lvSound.SelectedObject;
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
                    _lvSound.CheckObject(s);
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
                        _lvSound.CheckObject(s);
                    }                    
                    break;

                case "NONE":
                    s.SoundPath = string.Empty;
                    s.Type = ThemeSoundType.None;   
                    _lvSound.UncheckObject(s);
                    break;
            }
            _lvSound.RefreshObject(s);
            if (ThemeChanged != null)
            {
                ThemeChanged();
            }
        }
    }
}
