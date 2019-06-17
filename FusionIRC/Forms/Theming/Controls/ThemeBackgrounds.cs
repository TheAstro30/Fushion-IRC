/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Theming.Helpers;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;
using ircCore.Settings.Theming;
using ircCore.Settings.Theming.Structures;

namespace FusionIRC.Forms.Theming.Controls
{
    public sealed class ThemeBackgrounds : UserControl, IThemeSetting
    {        
        private readonly BackgroundStrip _bsConsole;
        private readonly BackgroundStrip _bsChannel;
        private readonly BackgroundStrip _bsPrivate;
        private readonly BackgroundStrip _bsDcc;

        public event Action ThemeChanged;

        public Theme CurrentTheme { get; set; }

        public ThemeBackgrounds(Theme theme)
        {
            CurrentTheme = theme;

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            Size = new Size(438, 360);

            _bsConsole = new BackgroundStrip
                             {
                                 BackColor = Color.Transparent,
                                 Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                 Header = "Console:",
                                 LayoutStyle = BackgroundImageLayoutStyles.None,
                                 Location = new Point(18, 3),
                                 MaximumSize = new Size(405, 78),
                                 MinimumSize = new Size(405, 78),
                                 SelectedImage = null,
                                 Size = new Size(405, 78),
                                 TabIndex = 0
                             };

            _bsChannel = new BackgroundStrip
                             {
                                 BackColor = Color.Transparent,
                                 Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                 Header = "Channel windows:",
                                 LayoutStyle = BackgroundImageLayoutStyles.None,
                                 Location = new Point(18, 87),
                                 MaximumSize = new Size(405, 78),
                                 MinimumSize = new Size(405, 78),
                                 SelectedImage = null,
                                 Size = new Size(405, 78),
                                 TabIndex = 1
                             };

            _bsPrivate = new BackgroundStrip
                             {
                                 BackColor = Color.Transparent,
                                 Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                 Header = "Private chat windows:",
                                 LayoutStyle = BackgroundImageLayoutStyles.None,
                                 Location = new Point(18, 171),
                                 MaximumSize = new Size(405, 78),
                                 MinimumSize = new Size(405, 78),
                                 SelectedImage = null,
                                 Size = new Size(405, 78),
                                 TabIndex = 2
                             };

            _bsDcc = new BackgroundStrip
                         {
                             BackColor = Color.Transparent,
                             Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                             Header = "DCC chat windows:",
                             LayoutStyle = BackgroundImageLayoutStyles.None,
                             Location = new Point(18, 255),
                             MaximumSize = new Size(405, 78),
                             MinimumSize = new Size(405, 78),
                             SelectedImage = null,
                             Size = new Size(405, 78),
                             TabIndex = 3
                         };

            Controls.AddRange(new Control[] {_bsConsole, _bsChannel, _bsPrivate, _bsDcc});

            var bg = ThemeManager.GetBackground(ChildWindowType.Console, theme);
            _bsConsole.SelectedImage = bg.Path;
            _bsConsole.LayoutStyle = bg.LayoutStyle;

            bg = ThemeManager.GetBackground(ChildWindowType.Channel);
            _bsChannel.SelectedImage = bg.Path;
            _bsChannel.LayoutStyle = bg.LayoutStyle;

            bg = ThemeManager.GetBackground(ChildWindowType.Private);
            _bsPrivate.SelectedImage = bg.Path;
            _bsPrivate.LayoutStyle = bg.LayoutStyle;

            bg = ThemeManager.GetBackground(ChildWindowType.DccChat);
            _bsDcc.SelectedImage = bg.Path;
            _bsDcc.LayoutStyle = bg.LayoutStyle;

            _bsConsole.SelectedBackgroundChanged += OnSelectedBackgroundChanged;
            _bsChannel.SelectedBackgroundChanged += OnSelectedBackgroundChanged;
            _bsPrivate.SelectedBackgroundChanged += OnSelectedBackgroundChanged;
            _bsDcc.SelectedBackgroundChanged += OnSelectedBackgroundChanged;
        }

        public void SaveSettings()
        {
            SaveSelectedImageData(ChildWindowType.Console, _bsConsole);
            SaveSelectedImageData(ChildWindowType.Channel, _bsChannel);
            SaveSelectedImageData(ChildWindowType.Private, _bsPrivate);
            SaveSelectedImageData(ChildWindowType.DccChat, _bsDcc);
        }

        private void SaveSelectedImageData(ChildWindowType type, BackgroundStrip bs)
        {
            var bg = new ThemeBackgroundData {Path = bs.SelectedImage, LayoutStyle = bs.LayoutStyle};
            CurrentTheme.ThemeBackgrounds[type] = bg;
        }

        private void OnSelectedBackgroundChanged()
        {
            if (ThemeChanged != null)
            {
                ThemeChanged();
            }
        }
    }
}