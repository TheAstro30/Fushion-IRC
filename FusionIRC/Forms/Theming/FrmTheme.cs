/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms.Theming.Controls;
using FusionIRC.Properties;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.SettingsBase.Structures.Misc;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms.Theming
{
    public sealed class FrmTheme : FormEx
    {
        private readonly ImageList _images;                
        private readonly Label _lblTheme;
        private readonly ComboBox _cmbTheme;
        private readonly Button _btnNew;
        private readonly Button _btnDelete;
        private readonly TabControl _tabTheme;
        private readonly TabPage _tabColors;       
        private readonly TabPage _tabMessages;
        private readonly TabPage _tabFonts;
        private readonly TabPage _tabBackgrounds;
        private readonly TabPage _tabSounds;

        private readonly Button _btnApply;
        private readonly Button _btnCancel;

        private ThemeColors _tColors;
        private ThemeMessages _tMessages;
        private ThemeFonts _tFonts;
        private ThemeBackgrounds _tBackgrounds;
        private ThemeSounds _tSounds;

        private Theme _theme;

        public FrmTheme()
        {
            ClientSize = new Size(473, 464);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = @"FusionIRC - Theme Manager";

            _images = new ImageList {ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16)};
            _images.Images.AddRange(new Image[]
                                        {
                                            Resources.themeColor.ToBitmap(),
                                            Resources.themeMessage.ToBitmap(),
                                            Resources.themeFont.ToBitmap(),
                                            Resources.themePicture.ToBitmap(),
                                            Resources.themeSound.ToBitmap()
                                        });

            _lblTheme = new Label
                            {
                                AutoSize = true,
                                BackColor = Color.Transparent,
                                Location = new Point(12, 9),
                                Size = new Size(47, 15),
                                Text = @"Theme:"
                            };

            _cmbTheme = new ComboBox
                            {
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                FormattingEnabled = true,
                                Location = new Point(65, 6),
                                Size = new Size(186, 23),
                                TabIndex = 0
                            };

            _btnNew = new Button
                          {
                              Location = new Point(257, 6),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Text = @"New",
                              UseVisualStyleBackColor = true
                          };

            _btnDelete = new Button
                             {
                                 Enabled = false,
                                 Location = new Point(338, 6),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Delete",
                                 UseVisualStyleBackColor = true
                             };

            _tabTheme = new TabControl
                            {
                                Location = new Point(15, 35),
                                SelectedIndex = 0,
                                Size = new Size(446, 388),
                                TabIndex = 3,
                                ImageList = _images
                            };


            _tabColors = new TabPage
                             {
                                 Location = new Point(4, 24),
                                 Padding = new Padding(3),
                                 Size = new Size(438, 360),
                                 TabIndex = 4,
                                 Text = @"Colors",
                                 UseVisualStyleBackColor = true,
                                 ImageIndex = 0
                             };

            _tabMessages = new TabPage
                               {
                                   Location = new Point(4, 24),
                                   Padding = new Padding(3),
                                   Size = new Size(438, 360),
                                   TabIndex = 5,
                                   Text = @"Messages",
                                   UseVisualStyleBackColor = true,
                                   ImageIndex = 1
                               };

            _tabFonts = new TabPage
                            {
                                Location = new Point(4, 24),
                                Size = new Size(438, 360),
                                TabIndex = 6,
                                Text = @"Fonts",
                                UseVisualStyleBackColor = true,
                                ImageIndex = 2
                            };

            _tabBackgrounds = new TabPage
                                  {
                                      Location = new Point(4, 24),
                                      Size = new Size(438, 360),
                                      TabIndex = 7,
                                      Text = @"Backgrounds",
                                      UseVisualStyleBackColor = true,
                                      ImageIndex = 3
                                  };

            _tabSounds = new TabPage
                             {
                                 Location = new Point(4, 24),
                                 Size = new Size(438, 360),
                                 TabIndex = 8,
                                 Text = @"Sounds",
                                 UseVisualStyleBackColor = true,
                                 ImageIndex = 4
                             };

            _btnApply = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Enabled = false,
                                Location = new Point(305, 429),
                                Size = new Size(75, 23),
                                TabIndex = 9,
                                Text = @"Apply",
                                UseVisualStyleBackColor = true
                            };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(386, 429),
                                 Size = new Size(75, 23),
                                 TabIndex = 10,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            _tabTheme.Controls.AddRange(new Control[] {_tabColors, _tabMessages, _tabFonts, _tabBackgrounds, _tabSounds});

            Controls.AddRange(new Control[] {_lblTheme, _cmbTheme, _btnNew, _btnDelete, _tabTheme, _btnApply, _btnCancel});

            AcceptButton = _btnApply;

            /* Import stored themes */
            for (var i = 0; i <= SettingsManager.Settings.Themes.Theme.Count - 1; i++)
            {
                _cmbTheme.Items.Add(SettingsManager.Settings.Themes.Theme[i]);
                if (i != SettingsManager.Settings.Themes.CurrentTheme)
                {
                    continue;
                }
                _cmbTheme.SelectedIndex = i;
                //LoadTheme((SettingsTheme.ThemeListData) _cmbTheme.SelectedItem);
            }
            _theme = new Theme(ThemeManager.CurrentTheme); /* This will always be the selected theme */
            _tabTheme.SelectedIndex = SettingsManager.Settings.Client.Tabs.Theme;

            _tabTheme.SelectedIndexChanged += TabSelectedIndexChanged;
            _cmbTheme.SelectedIndexChanged += ComboSelectedIndexChanged;
            _btnNew.Click += ButtonClickHandler;
            _btnDelete.Click += ButtonClickHandler;
            _btnApply.Click += ButtonClickHandler;
            /* Window position */
            var w = SettingsManager.GetWindowByName("theme");
            Location = w.Position;
        }

        protected override void OnLoad(EventArgs e)
        {
            _tColors = new ThemeColors(_theme) {Dock = DockStyle.Fill};
            _tabColors.Controls.Add(_tColors);

            _tMessages = new ThemeMessages(_theme) {Dock = DockStyle.Fill};
            _tabMessages.Controls.Add(_tMessages);

            _tFonts = new ThemeFonts(_theme) {Dock = DockStyle.Fill};
            _tabFonts.Controls.Add(_tFonts);

            _tBackgrounds = new ThemeBackgrounds(_theme) {Dock = DockStyle.Fill};
            _tabBackgrounds.Controls.Add(_tBackgrounds);

            _tSounds = new ThemeSounds(_theme) { Dock = DockStyle.Fill };
            _tabSounds.Controls.Add(_tSounds);

            _tColors.ThemeChanged += OnThemeChanged;
            _tMessages.ThemeChanged += OnThemeChanged;
            _tFonts.ThemeChanged += OnThemeChanged;
            _tBackgrounds.ThemeChanged += OnThemeChanged;
            _tSounds.ThemeChanged += OnThemeChanged;
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            var w = SettingsManager.GetWindowByName("theme");
            w.Position = Location;
            base.OnFormClosing(e);
        }

        /* Handlers */
        private void ComboSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cmbTheme.SelectedItem == null)
            {
                return;
            }
            foreach (
                var p in
                    SettingsManager.Settings.Themes.Theme.Where(
                        p => p.Name.Equals(_theme.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                SaveCurrentTheme(p);
            }
            LoadTheme((SettingsTheme.ThemeListData) _cmbTheme.SelectedItem);
            _tColors.CurrentTheme = _theme;
            _tMessages.CurrentTheme = _theme;
            /* Index 0 will always be the default theme which cannot be deleted */
            _btnDelete.Enabled = _cmbTheme.SelectedIndex != 0;
            _btnApply.Enabled = true;
        }

        private void TabSelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsManager.Settings.Client.Tabs.Theme = _tabTheme.SelectedIndex;
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
                case "NEW":
                    NewTheme();
                    break;

                case "DELETE":
                    DeleteTheme();
                    break;

                case "APPLY":
                    /* Apply the new theme and close */
                    var t = (SettingsTheme.ThemeListData) _cmbTheme.SelectedItem;
                    if (t == null)
                    {
                        return;
                    }
                    _tFonts.SaveSettings();
                    _tBackgrounds.SaveSettings();
                    _tSounds.SaveSettings();
                    ThemeManager.Save(Functions.MainDir(t.Path), _theme);
                    SettingsManager.Settings.Themes.CurrentTheme = _cmbTheme.SelectedIndex;
                    ThemeManager.Load(Functions.MainDir(t.Path));
                    break;
            }
        }

        private void OnThemeChanged()
        {
            _theme.ThemeChanged = true;
            _btnApply.Enabled = true;
        }

        /* Private methods */
        private void NewTheme()
        {
            using (var f = new FrmNew())
            {
                f.ShowDialog(this);
                if (string.IsNullOrEmpty(f.ThemeName))
                {
                    return;
                }
                /* Create a new theme */
                SaveCurrentTheme((SettingsTheme.ThemeListData) _cmbTheme.SelectedItem);
                var t = new SettingsTheme.ThemeListData
                            {
                                Name = f.ThemeName,
                                Path = string.Format(@"\themes\{0}.thm", Functions.CleanFileName(f.ThemeName))
                            };
                SettingsManager.Settings.Themes.Theme.Add(t);
                var theme = new Theme
                                {
                                    Name = t.Name
                                };
                ThemeManager.Save(Functions.MainDir(t.Path), theme);
                _cmbTheme.Items.Add(t);
                _cmbTheme.SelectedIndex = _cmbTheme.Items.Count - 1;
                _theme = theme;
                _tColors.CurrentTheme = _theme;
                _tMessages.CurrentTheme = _theme;
                _btnApply.Enabled = true;
            }
        }

        private void DeleteTheme()
        {
            /* Make sure to load a new theme if this theme deleted is the current selected theme */
            var sel = _cmbTheme.SelectedIndex - 1;
            var t = (SettingsTheme.ThemeListData) _cmbTheme.Items[sel];
            var deleted = (SettingsTheme.ThemeListData) _cmbTheme.SelectedItem;
            if (t == null)
            {
                return;
            }
            /* Delete theme data */
            SettingsManager.Settings.Themes.Theme.RemoveAt(_cmbTheme.SelectedIndex);
            var path = Functions.MainDir(deleted.Path);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            _cmbTheme.Items.RemoveAt(_cmbTheme.SelectedIndex);
            /* Select previous theme and load it */
            _cmbTheme.SelectedIndex = sel;
            LoadTheme(t);
            _tColors.CurrentTheme = _theme;
            _tMessages.CurrentTheme = _theme;
            SettingsManager.Settings.Themes.CurrentTheme = _cmbTheme.SelectedIndex;
            ThemeManager.Load(path); /* Make sure client is updated */
            _btnApply.Enabled = true;
        }

        private void LoadTheme(SettingsTheme.ThemeListData selectedItem)
        {
            var t = selectedItem;
            if (t == null)
            {
                return;
            }
            var theme = new Theme();
            ThemeManager.Load(Functions.MainDir(t.Path), ref theme, true);
            _theme = theme;
        }

        private void SaveCurrentTheme(SettingsTheme.ThemeListData selectedItem)
        {
            if (_theme == null || !_theme.ThemeChanged)
            {
                return;
            }
            var t = selectedItem;
            if (t == null)
            {
                return;
            }
            ThemeManager.Save(Functions.MainDir(t.Path), _theme);
        }
    }
}