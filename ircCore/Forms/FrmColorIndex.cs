/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.Input.ColorBox;
using ircCore.Settings.Theming;

namespace ircCore.Forms
{
    public sealed class FrmColorIndex : Form
    {
        private readonly ColorSelectionBox _ircColors;

        public Control Box { get; set; }

        public event Action<string> SelectedIndexChanged;

        /* Constructor */
        public FrmColorIndex()
        {
            _ircColors = new ColorSelectionBox
                            {
                                Font = new Font("Tahoma", 8.25F, FontStyle.Bold),
                                Location = new Point(2, 2),
                                SelectedColor = 0,
                                ShowFocusRectangle = true,
                                Size = new Size(337, 22),                                
                            };
            Controls.Add(_ircColors);

            ClientSize = new Size(342, 25);            
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = @"Color Palette";

            for (var i = 0; i <= 15; i++)
            {
                _ircColors.SetBoxColor(i, ThemeManager.CurrentTheme.Colors[i]);
            }
            _ircColors.ShowFocusRectangle = false;
            _ircColors.MouseDown += ColorSelect;
        }

        /* Handle color box click */
        private void ColorSelect(object sender, MouseEventArgs e)
        {            
            Close();
            if (SelectedIndexChanged != null)
            {
                SelectedIndexChanged(_ircColors.SelectedColor.ToString());
            }
        }
    }
}
