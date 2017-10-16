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
            lstWindowColors.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(ThemeColor)));
            lstEventColors.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(ThemeMessage)));
            for (var i = 0; i <= _theme.Colors.Length - 1; i++)
            {
                colorSelector.SetBoxColor(i, _theme.Colors[i]);
            }
        }
    }
}
