/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Theming.Helpers;
using ircCore.Settings.Theming;

namespace FusionIRC.Forms.Theming.Controls
{
    public sealed class ThemeFonts : UserControl, IThemeSetting
    {
        private readonly FontStrip _fsConsole;
        private readonly FontStrip _fsChannel;                
        private readonly FontStrip _fsPrivate;
        private readonly FontStrip _fsDcc;

        public Theme CurrentTheme { get; set; }

        public event Action ThemeChanged;

        public ThemeFonts(Theme theme)
        {
            CurrentTheme = theme;

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            Size = new Size(438, 360);

            _fsConsole = new FontStrip
                            {
                                BackColor = Color.Transparent,
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                Header = "Console:",
                                Location = new Point(43, 28),
                                MaximumSize = new Size(339, 50),
                                MinimumSize = new Size(339, 50),
                                SelectedFont = ThemeManager.GetFont(ChildWindowType.Console, theme),
                                Size = new Size(339, 50),
                                TabIndex = 0
                            };

            _fsChannel = new FontStrip
                            {
                                BackColor = Color.Transparent,
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                Header = "Channel windows:",
                                Location = new Point(43, 108),
                                MaximumSize = new Size(339, 50),
                                MinimumSize = new Size(339, 50),
                                Size = new Size(339, 50),
                                TabIndex = 1,
                                SelectedFont = ThemeManager.GetFont(ChildWindowType.Channel, theme)
                            };

            _fsPrivate = new FontStrip
                            {
                                BackColor = Color.Transparent,
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                Header = "Private chat windows:",
                                Location = new Point(43, 188),
                                MaximumSize = new Size(339, 50),
                                MinimumSize = new Size(339, 50),
                                Size = new Size(339, 50),
                                TabIndex = 2,
                                SelectedFont = ThemeManager.GetFont(ChildWindowType.Private, theme)
                            };

            _fsDcc = new FontStrip
                        {
                            BackColor = Color.Transparent,
                            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                            Header = "DCC chat windows",
                            Location = new Point(43, 266),
                            MaximumSize = new Size(339, 50),
                            MinimumSize = new Size(339, 50),
                            Size = new Size(339, 50),
                            TabIndex = 3,
                            SelectedFont = ThemeManager.GetFont(ChildWindowType.DccChat, theme)
                        };

            Controls.AddRange(new Control[] {_fsConsole, _fsChannel, _fsPrivate, _fsDcc});

            _fsConsole.SelectedFontChanged += OnSelectedFontChanged;
            _fsChannel.SelectedFontChanged += OnSelectedFontChanged;
            _fsPrivate.SelectedFontChanged += OnSelectedFontChanged;
            _fsDcc.SelectedFontChanged += OnSelectedFontChanged;
        }        

        public void SaveSettings()
        {
            CurrentTheme.ThemeFonts[ChildWindowType.Console] = _fsConsole.SelectedFont;
            CurrentTheme.ThemeFonts[ChildWindowType.Channel] = _fsChannel.SelectedFont;
            CurrentTheme.ThemeFonts[ChildWindowType.Private] = _fsPrivate.SelectedFont;
            CurrentTheme.ThemeFonts[ChildWindowType.DccChat] = _fsDcc.SelectedFont;
        }

        private void OnSelectedFontChanged()
        {
            if (ThemeChanged != null)
            {
                ThemeChanged();
            }
        }
    }
}