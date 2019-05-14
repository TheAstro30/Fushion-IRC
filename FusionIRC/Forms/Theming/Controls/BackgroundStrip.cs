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
    public partial class BackgroundStrip : UserControl
    {
        private string _selectedImage;
        private BackgroundImageLayoutStyles _layoutStyle;

        public event Action SelectedBackgroundChanged;

        public string Header
        {
            get { return lblHeader.Text; }
            set { lblHeader.Text = value; }
        }

        public string SelectedImage
        {
            get { return _selectedImage; }
            set
            {                
                _selectedImage = value;
                txtImage.Text = value;
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
                for (var i = 0; i <= cmbLayout.Items.Count -1;i++)
                {
                    if ((BackgroundImageLayoutStyles)i == value)
                    {
                        cmbLayout.SelectedIndex = i;
                        break;
                    }
                }
                if (value == BackgroundImageLayoutStyles.None)
                {
                    pnlPreview.BackgroundImage = null;
                }
            }
        }

        public BackgroundStrip()
        {
            InitializeComponent();
            cmbLayout.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof(BackgroundImageLayoutStyles)));
            pnlPreview.BackgroundImageLayout = ImageLayout.Center;

            cmbLayout.SelectedIndexChanged += ComboSelect;
            btnSelect.Click += ButtonSelect;
        }

        private void ComboSelect(object sender, EventArgs e)
        {
            if (cmbLayout.SelectedIndex == -1 || string.IsNullOrEmpty(_selectedImage))
            {
                cmbLayout.SelectedIndex = 0;
                return;
            }
            _layoutStyle = (BackgroundImageLayoutStyles) cmbLayout.SelectedIndex;
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

        private void DrawPreview()
        {
            var f = Functions.MainDir(_selectedImage);
            if (File.Exists(f) && LayoutStyle != BackgroundImageLayoutStyles.None)
            {
                pnlPreview.BackgroundImage = Image.FromFile(f);
            }
            else
            {
                pnlPreview.BackgroundImage = null;
            }
        }
    }
}
