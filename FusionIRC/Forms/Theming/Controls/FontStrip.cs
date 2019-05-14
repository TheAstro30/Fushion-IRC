/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using ircCore.Forms;

namespace FusionIRC.Forms.Theming.Controls
{
    public sealed class FontStrip : UserControl
    {        
        private readonly Label _lblHeader;
        private readonly TextBox _txtFont;
        private readonly Button _btnSelect;

        private Font _font = new Font("Tahoma", 9);

        public event Action SelectedFontChanged;

        public FontStrip()
        {
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            MaximumSize = new Size(339, 50);
            MinimumSize = new Size(339, 50);
            Size = new Size(339, 50);

            _lblHeader = new Label
                             {
                                 AutoSize = true, 
                                 Location = new Point(3, 0), 
                                 Size = new Size(45, 15), Text = @"Header"
                             };

            _txtFont = new TextBox
                           {
                               Location = new Point(6, 18),
                               ReadOnly = true,
                               Size = new Size(294, 23),
                               TabIndex = 0
                           };

            _btnSelect = new Button
                             {
                                 Location = new Point(306, 18),
                                 Size = new Size(30, 23),
                                 TabIndex = 1,
                                 Text = @"...",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblHeader, _txtFont, _btnSelect});

            _btnSelect.Click += SelectButtonClick;
        }

        public string Header
        {
            get { return _lblHeader.Text; }
            set { _lblHeader.Text = value; }
        }

        public Font SelectedFont
        {
            get { return _font; }
            set
            {
                _font = value;
                _txtFont.Text = FontText();
            }
        }

        /* Button click handler */
        private void SelectButtonClick(object sender, EventArgs e)
        {
            using (var f = new FrmFont {SelectedFont = _font, ShowDefaultCheckbox = false})
            {
                if (f.ShowDialog(this) == DialogResult.Cancel)
                {
                    return;
                }
                SelectedFont = f.SelectedFont;
                _txtFont.Text = FontText();
                if (SelectedFontChanged != null)
                {
                    SelectedFontChanged();
                }
            }
        }

        /* Helper function */

        private string FontText()
        {
            return string.Format("{0}, {1}", _font.Name, _font.Size);
        }
    }
}