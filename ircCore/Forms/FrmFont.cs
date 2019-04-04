/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;

namespace ircCore.Forms
{
    public sealed class FrmFont : FormEx
    {
        /* Font selection dialog (allows overcoming problems with normal dialog and some fonts)
           By: Jason James Newland
           ©2012 - KangaSoft Software
           All Rights Reserved
         */                                
        private readonly Label _lblFont;
        private readonly FontComboBox _cmbFont;
        private readonly Label _lblSize;
        private readonly ComboBox _cmbSizes;
        private readonly Label _lblStyle;
        private readonly ComboBox _cmbStyles;
        private readonly Label _lblSample;
        private readonly Panel _pnlSample;
        private readonly CheckBox _chkDefault;        
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        private readonly Timer _tmrFocus;

        private Font _initialFont = new Font("Tahoma", 9);
        private int _initialFontSize = 8;
        private FontStyle _initialFontStyle = FontStyle.Regular;

        public FrmFont()
        {
            ClientSize = new Size(276, 263);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Select font";

            _lblFont = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(10, 8),
                               Size = new Size(34, 15),
                               Text = @"Font:"
                           };

            _cmbFont = new FontComboBox
                           {
                               DrawMode = DrawMode.OwnerDrawFixed,
                               DropDownStyle = ComboBoxStyle.DropDownList,
                               FormattingEnabled = true,
                               IntegralHeight = false,
                               Location = new Point(13, 24),
                               MaxDropDownItems = 20,
                               Size = new Size(251, 24),
                               TabIndex = 0
                           };

            _lblSize = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(10, 58),
                               Size = new Size(30, 15),
                               Text = @"Size:"
                           };

            _cmbSizes = new ComboBox
                            {
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                FormattingEnabled = true,
                                Location = new Point(13, 74),
                                Size = new Size(77, 23),
                                TabIndex = 1
                            };

            _lblStyle = new Label
                            {
                                AutoSize = true,
                                BackColor = Color.Transparent,
                                Location = new Point(104, 58),
                                Size = new Size(35, 15),
                                Text = @"Style:"
                            };

            _cmbStyles = new ComboBox
                             {
                                 DropDownStyle = ComboBoxStyle.DropDownList,
                                 FormattingEnabled = true,
                                 Location = new Point(105, 74),
                                 Size = new Size(159, 23),
                                 TabIndex = 2
                             };

            _lblSample = new Label
                             {
                                 AutoSize = true,
                                 BackColor = Color.Transparent,
                                 Location = new Point(10, 106),
                                 Size = new Size(49, 15),
                                 Text = @"Sample:"
                             };

            _pnlSample = new Panel
                             {
                                 BackColor = Color.Transparent,
                                 BorderStyle = BorderStyle.Fixed3D,
                                 Location = new Point(13, 123),
                                 Size = new Size(251, 64)
                             };

            _chkDefault = new CheckBox
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(13, 197),
                                  Size = new Size(210, 19),
                                  Text = @"Set as default console window font",
                                  TabIndex = 3,
                                  UseVisualStyleBackColor = false
                              };

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(108, 228),
                             Size = new Size(75, 23),
                             TabIndex = 4,
                             Text = @"Select",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(189, 228),
                                 Size = new Size(75, 23),
                                 TabIndex = 5,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[]
                                  {
                                      _lblFont, _cmbFont, _lblSize, _cmbSizes, _lblStyle, _cmbStyles, _lblSample,
                                      _pnlSample, _chkDefault, _btnOk, _btnCancel
                                  });

            _tmrFocus = new Timer {Interval = 10};
            _tmrFocus.Tick += TmrFocusTick;

            AcceptButton = _btnOk;

            ShowDefaultCheckbox = true;
        }

        /* Properties */
        public Font SelectedFont
        {
            get
            {
                var fn = (FontComboBox.FontNames) _cmbFont.SelectedItem;
                return new Font(fn.Family, (int) _cmbSizes.SelectedItem, (FontStyle) _cmbStyles.SelectedItem);
            }
            set
            {
                _initialFontSize = (int) Math.Round(value.Size);
                _initialFont = new Font(value.Name, _initialFontSize);
                _initialFontStyle = value.Style;                
            }
        }

        public bool SelectedFontDefault
        {
            get { return _chkDefault.Checked; }
        }

        public string SelectedFontDefaultText
        {
            set { _chkDefault.Text = value; }
        }

        public bool ShowDefaultCheckbox
        {
            set { _chkDefault.Visible = value; }
        }

        /* Form events */
        protected override void OnLoad(EventArgs e)
        {
            CenterToParent();
            /* Initialize font sizes */
            int i;
            for (i = 0; i <= 28; i++)
            {
                if (i == 0)
                {
                    i = 8;
                }
                _cmbSizes.Items.Add(i);
                if (i >= 12)
                {
                    i += 1;
                }
            }
            /* Set font details */
            var fn = new FontComboBox.FontNames {Family = _initialFont.FontFamily};
            _cmbFont.SelectedItem = fn;
            i = _cmbSizes.Items.IndexOf(_initialFontSize);
            _cmbSizes.SelectedIndex = i > -1 ? i : 0;
            /* Handlers */
            _cmbFont.SelectedIndexChanged += CmbFontSelectedIndexChanged;
            _cmbSizes.SelectedIndexChanged += CmbSizesSelectedIndexChanged;
            _cmbStyles.SelectedIndexChanged += CmbStylesSelectedIndexChanged;
            _pnlSample.Paint += SampleOnPaint;
            /* Update combos */
            UpdateFontStyles();
            _tmrFocus.Enabled = true;
        }

        /* Callbacks */
        private void CmbFontSelectedIndexChanged(object sender, EventArgs e)
        {
            var fn = (FontComboBox.FontNames) _cmbFont.SelectedItem;
            _initialFont = new Font(fn.Family, _initialFontSize, _initialFontStyle);
            UpdateFontStyles();
            _pnlSample.Refresh();
        }

        private void CmbSizesSelectedIndexChanged(object sender, EventArgs e)
        {
            _pnlSample.Refresh();
        }

        private void CmbStylesSelectedIndexChanged(object sender, EventArgs e)
        {
            _initialFontStyle = (FontStyle) _cmbStyles.SelectedItem;
            _pnlSample.Refresh();
        }

        /* Private methods */
        private void UpdateFontStyles()
        {
            /* Update the styles combo box */
            var fn = (FontComboBox.FontNames) _cmbFont.SelectedItem;
            _cmbStyles.Items.Clear();

            if (fn.Family.IsStyleAvailable(FontStyle.Regular))
            {
                _cmbStyles.Items.Add(FontStyle.Regular);
            }
            if (fn.Family.IsStyleAvailable(FontStyle.Bold))
            {
                _cmbStyles.Items.Add(FontStyle.Bold);
            }
            if (fn.Family.IsStyleAvailable(FontStyle.Italic))
            {
                _cmbStyles.Items.Add(FontStyle.Italic);
            }
            if (fn.Family.IsStyleAvailable(FontStyle.Bold | FontStyle.Italic))
            {
                _cmbStyles.Items.Add((FontStyle.Bold | FontStyle.Italic));
            }

            _cmbStyles.Text = _cmbStyles.Items.Contains(_initialFontStyle)
                                 ? _initialFontStyle.ToString()
                                 : _cmbStyles.Items[0].ToString();
        }

        private void SampleOnPaint(object sender, PaintEventArgs e)
        {
            var fn = (FontComboBox.FontNames) _cmbFont.SelectedItem;
            var setFont = new Font(fn.Family, (int) _cmbSizes.SelectedItem, (FontStyle) _cmbStyles.SelectedItem);
            const string sSample = "AaBbYyZz";
            var sz = SizeF.Empty;
            sz = e.Graphics.MeasureString(sSample, setFont, sz, StringFormat.GenericTypographic);
            var iWidth = _pnlSample.ClientRectangle.Width/2;
            var iHeight = _pnlSample.ClientRectangle.Height/2;
            using (Brush b = new SolidBrush(Color.Black))
            {
                e.Graphics.DrawString(sSample, setFont, b, iWidth - (sz.Width/2), iHeight - (sz.Height/2),
                                      StringFormat.GenericTypographic);
            }
        }

        /* Timer event */
        private void TmrFocusTick(object sender, EventArgs e)
        {
            _tmrFocus.Enabled = false;
            _cmbFont.Focus();
        }
    }
}
