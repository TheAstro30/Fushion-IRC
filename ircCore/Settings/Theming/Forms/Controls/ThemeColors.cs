/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Windows.Forms;
using ircCore.Utils;

namespace ircCore.Settings.Theming.Forms.Controls
{
    public partial class ThemeColors : UserControl
    {
        private readonly Theme _theme;

        public ThemeColors(Theme theme)
        {
            InitializeComponent();
            _theme = theme;
            for (var win = 0; win <= 7; win++)
            {
                lstWindowColors.Items.Add(Functions.EnumUtils.GetDescriptionFromEnumValue((ThemeColor) win));
            }

            for (var ev = 0; ev <= 38; ev++)
            {
                lstEventColors.Items.Add(Functions.EnumUtils.GetDescriptionFromEnumValue((ThemeMessage)ev));
            }

            for (var i = 0; i <= _theme.Colors.Length - 1; i++)
            {
                colorSelector.SetBoxColor(i, _theme.Colors[i]);
            }
        }
    }
}
