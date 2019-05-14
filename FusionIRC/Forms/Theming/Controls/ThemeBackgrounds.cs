/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using FusionIRC.Forms.Theming.Helpers;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms.Theming.Controls
{
    public partial class ThemeBackgrounds : UserControl, IThemeSetting
    {
        public event Action ThemeChanged;

        public Theme CurrentTheme { get; set; }

        public ThemeBackgrounds(Theme theme)
        {
            InitializeComponent();
            CurrentTheme = theme;

            var bg = ThemeManager.GetBackground(ChildWindowType.Console, theme);
            bsConsole.SelectedImage = Functions.MainDir(bg.Path);
            bsConsole.LayoutStyle = bg.LayoutStyle;

            bg = ThemeManager.GetBackground(ChildWindowType.Channel);
            bsChannel.SelectedImage = Functions.MainDir(bg.Path);
            bsChannel.LayoutStyle = bg.LayoutStyle;

            bg = ThemeManager.GetBackground(ChildWindowType.Private);
            bsPrivate.SelectedImage = Functions.MainDir(bg.Path);
            bsPrivate.LayoutStyle = bg.LayoutStyle;

            bg = ThemeManager.GetBackground(ChildWindowType.DccChat);
            bsDcc.SelectedImage = Functions.MainDir(bg.Path);
            bsDcc.LayoutStyle = bg.LayoutStyle;

            bsConsole.SelectedBackgroundChanged += OnSelectedBackgroundChanged;
            bsChannel.SelectedBackgroundChanged += OnSelectedBackgroundChanged;
            bsPrivate.SelectedBackgroundChanged += OnSelectedBackgroundChanged;
            bsDcc.SelectedBackgroundChanged += OnSelectedBackgroundChanged;
        }

        public void SaveSettings()
        {
            SaveSelectedImageData(ChildWindowType.Console, bsConsole);
            SaveSelectedImageData(ChildWindowType.Channel, bsChannel);
            SaveSelectedImageData(ChildWindowType.Private, bsPrivate);
            SaveSelectedImageData(ChildWindowType.DccChat, bsDcc);
        }

        private void SaveSelectedImageData(ChildWindowType type, BackgroundStrip bs)
        {
            var bg = new Theme.ThemeBackgroundData {Path = bs.SelectedImage, LayoutStyle = bs.LayoutStyle};
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
