/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Theming.Controls;
using ircCore.Controls;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms.Theming
{
    public sealed class FrmTheme : FormEx
    {
        private readonly Theme _theme = new Theme();

        private readonly ThemePreview _themePreview;
        private readonly ThemeColors _themeColors;

        private readonly TabControl _tab;
        private readonly TabPage _tabTheme;
        private readonly TabPage _tabColors;        
        private readonly TabPage _tabFonts;
        private readonly TabPage _tabMessages;
        private readonly TabPage _tabBackgrounds;

        private readonly Button _btnApply;
        private readonly Button _btnClose;

        public FrmTheme()
        {            
            ClientSize = new Size(609, 505);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"FusionIRC Theme Manager";

            _themePreview = new ThemePreview(_theme)
                                {
                                    BackColor = Color.Transparent,
                                    Dock = DockStyle.Fill,
                                    Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                    Location = new Point(3, 3),
                                    Size = new Size(571, 408)
                                };

            _themeColors = new ThemeColors(_theme)
                                {
                                    BackColor = Color.Transparent,
                                    Dock = DockStyle.Fill,
                                    Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                    Location = new Point(3, 3),
                                    Size = new Size(571, 408)
                                };

            _tab = new TabControl {Location = new Point(12, 12), SelectedIndex = 0, Size = new Size(585, 442)};

            _tabTheme = new TabPage
                            {
                                Location = new Point(4, 24),
                                Padding = new Padding(3),
                                Size = new Size(577, 414),
                                TabIndex = 0,
                                Text = @"Current Theme",
                                UseVisualStyleBackColor = true
                            };

            _tabTheme.Controls.Add(_themePreview);

            _tabColors = new TabPage
                             {
                                 Location = new Point(4, 22),
                                 Padding = new Padding(3),
                                 Size = new Size(577, 416),
                                 TabIndex = 1,
                                 Text = @"Theme Colors",
                                 UseVisualStyleBackColor = true
                             };

            _tabColors.Controls.Add(_themeColors);

            _tabFonts = new TabPage
                            {
                                Location = new Point(4, 22),
                                Size = new Size(577, 416),
                                TabIndex = 2,
                                Text = @"Theme Fonts",
                                UseVisualStyleBackColor = true
                            };

            _tabMessages = new TabPage
                               {
                                   Location = new Point(4, 22),
                                   Size = new Size(577, 416),
                                   TabIndex = 3,
                                   Text = @"Theme Messages",
                                   UseVisualStyleBackColor = true
                               };

            _tabBackgrounds = new TabPage
                                  {
                                      Location = new Point(4, 22),
                                      Size = new Size(577, 416),
                                      TabIndex = 4,
                                      Text = @"Backgrounds",
                                      UseVisualStyleBackColor = true
                                  };

            _tab.Controls.AddRange(new Control[] {_tabTheme, _tabColors, _tabFonts, _tabMessages, _tabBackgrounds});

            _btnApply = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Location = new Point(441, 470),
                                Size = new Size(75, 23),
                                TabIndex = 5,
                                Tag = "APPLY",
                                Text = @"Apply",
                                UseVisualStyleBackColor = true
                            };

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.Cancel,
                                Location = new Point(522, 470),
                                Size = new Size(75, 23),
                                TabIndex = 6,
                                Tag = "CLOSE",
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            AcceptButton = _btnApply;
            _btnApply.Click += ButtonClickHandler;
            Controls.AddRange(new Control[] {_tab, _btnApply, _btnClose});            
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Tag.ToString())
            {
                case "APPLY":
                    /* Apply the new theme and close */                    
                    var t = _themePreview.ThemeData;
                    if (t == null)
                    {
                        return;
                    }
                    SettingsManager.Settings.Themes.CurrentTheme = _themePreview.ThemeIndex;
                    ThemeManager.Load(Functions.MainDir(t.Path));
                    break;
            }
        }
    }
}
