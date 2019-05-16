/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;
using ircCore.Utils;

namespace FusionIRC.Forms.Theming.Controls
{
    public sealed class BackgroundStrip : UserControl
    {
        private readonly Label _lblHeader;
        private readonly TextBox _txtImage;
        private readonly Button _btnSelect;
        private readonly Panel _pnlPreview;
        private readonly Label _lblLayout;
        private readonly ComboBox _cmbLayout;
                        
        private BackgroundImageLayoutStyles _layoutStyle;
        private string _selectedImage;

        public event Action SelectedBackgroundChanged;

        /* Public properties */
        public string Header
        {
            get { return _lblHeader.Text; }
            set { _lblHeader.Text = value; }
        }

        public string SelectedImage
        {
            get { return _selectedImage; }
            set
            {
                _selectedImage = value;
                _txtImage.Text = value;
                DrawPreview();
                if (SelectedBackgroundChanged != null)
                {
                    SelectedBackgroundChanged();
                }
            }
        }

        public BackgroundImageLayoutStyles LayoutStyle
        {
            get { return _layoutStyle; }
            set
            {
                _layoutStyle = value;
                for (var i = 0; i <= _cmbLayout.Items.Count - 1; i++)
                {
                    if ((BackgroundImageLayoutStyles) i != value)
                    {
                        continue;
                    }
                    _cmbLayout.SelectedIndex = i;
                    break;
                }
                if (value == BackgroundImageLayoutStyles.None)
                {
                    _pnlPreview.BackgroundImage = null;
                }
            }
        }

        /* Constructor */
        public BackgroundStrip()
        {
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            MaximumSize = new Size(405, 78);
            MinimumSize = new Size(405, 78);
            Size = new Size(405, 78);

            _lblHeader = new Label
                             {
                                 AutoSize = true, 
                                 Location = new Point(3, 3),
                                 Size = new Size(45, 15), 
                                 Text = @"Header"
                             };

            _txtImage = new TextBox
                            {
                                Location = new Point(6, 21), 
                                ReadOnly = true,
                                Size = new Size(256, 23),
                                TabIndex = 0
                            };

            _btnSelect = new Button
                             {
                                 Location = new Point(268, 21),
                                 Size = new Size(30, 23),
                                 TabIndex = 1,
                                 Text = @"...",
                                 UseVisualStyleBackColor = true
                             };

            _pnlPreview = new Panel
                              {
                                  BorderStyle = BorderStyle.FixedSingle,
                                  Location = new Point(304, 3),
                                  Size = new Size(98, 68),
                                  BackgroundImageLayout = ImageLayout.Zoom
                              };

            _lblLayout = new Label
                             {
                                 AutoSize = true,
                                 Location = new Point(3, 51),
                                 Size = new Size(79, 15),
                                 Text = @"Image layout:"
                             };

            _cmbLayout = new ComboBox
                             {
                                 DropDownStyle = ComboBoxStyle.DropDownList,
                                 FormattingEnabled = true,
                                 Location = new Point(88, 48),
                                 Size = new Size(174, 23),
                                 TabIndex = 2
                             };

            Controls.AddRange(new Control[] {_lblHeader, _txtImage, _pnlPreview, _lblLayout, _cmbLayout});

            _cmbLayout.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof (BackgroundImageLayoutStyles)));

            _cmbLayout.SelectedIndexChanged += ComboSelect;
            _btnSelect.Click += ButtonSelect;
        }

        /* Handlers */
        private void ComboSelect(object sender, EventArgs e)
        {
            if (_cmbLayout.SelectedIndex == -1 || string.IsNullOrEmpty(_selectedImage))
            {
                _cmbLayout.SelectedIndex = 0;
                return;
            }
            _layoutStyle = (BackgroundImageLayoutStyles) _cmbLayout.SelectedIndex;
            if (_layoutStyle == BackgroundImageLayoutStyles.None)
            {
                SelectedImage = string.Empty;
            }
            DrawPreview();
            if (SelectedBackgroundChanged != null)
            {
                SelectedBackgroundChanged();
            }
        }

        private void ButtonSelect(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
                                 {
                                     Title = @"Select a background image to load",
                                     Filter =
                                         @"Picture files (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|" +
                                         @"PNG Images (*.png)|*.png|JPEG Images (*.jpg)|*.jpg|" +
                                         @"Bitmap Images (*.bmp)|*.bmp",
                                 })
            {
                if (ofd.ShowDialog(this) == DialogResult.Cancel)
                {
                    return;
                }
                SelectedImage = Functions.MainDir(ofd.FileName);
                LayoutStyle = BackgroundImageLayoutStyles.Center;
            }
        }

        /* Private helper method */
        private void DrawPreview()
        {
            var f = Functions.MainDir(_selectedImage);
            if (File.Exists(f) && LayoutStyle != BackgroundImageLayoutStyles.None)
            {
                _pnlPreview.BackgroundImage = Image.FromFile(f);
                return;
            }
            _pnlPreview.BackgroundImage = null;
        }
    }
}