/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Theming.Helpers;
using ircCore.Controls.ChildWindows.Input.ColorBox;
using ircCore.Controls.ChildWindows.OutputDisplay;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms.Theming.Controls
{
    public sealed class ThemeMessages : UserControl, IThemeSetting
    {
        private readonly ListBox _lstMessages;
        private readonly Label _lblTimestamp;
        private readonly TextBox _txtTimestamp;
        private readonly Label _lblString;
        private readonly ColorTextBox _txtString;
        private readonly Label _lblPreview;
        private readonly OutputWindow _preview;

        private bool _textSet;
        
        public event Action ThemeChanged;

        public Theme CurrentTheme { get; set; }        

        public ThemeMessages(Theme theme)
        {
            CurrentTheme = theme;

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            Size = new Size(438, 360);

            _lstMessages = new ListBox
                               {
                                   BorderStyle = BorderStyle.FixedSingle,
                                   FormattingEnabled = true,
                                   IntegralHeight = false,
                                   ItemHeight = 15,
                                   Location = new Point(3, 3),
                                   Size = new Size(233, 239),
                                   TabIndex = 0
                               };

            _lblTimestamp = new Label
                                {
                                    AutoSize = true,
                                    Location = new Point(246, 100),
                                    Size = new Size(120, 15),
                                    Text = @"Time stamp format:"
                                };

            _txtTimestamp = new TextBox
                                {
                                    Location = new Point(246, 118),
                                    Size = new Size(180, 23),
                                    TabIndex = 1
                                };

            _lblString = new Label
                             {
                                 AutoSize = true,
                                 Location = new Point(3, 258),
                                 Size = new Size(89, 15),
                                 Text = @"Message string:"
                             };

            _txtString = new ColorTextBox
                             {
                                 AllowMultiLinePaste = false,
                                 IsMultiLinePaste = false,
                                 IsNormalTextbox = false,
                                 Location = new Point(3, 276),
                                 ProcessCodes = true,
                                 Size = new Size(424, 23),
                                 TabIndex = 2
                             };

            _lblPreview = new Label
                              {
                                  AutoSize = true,
                                  Location = new Point(3, 315),
                                  Size = new Size(51, 15),
                                  Text = @"Preview:"
                              };

            _preview = new OutputWindow
                           {
                               AllowCopySelection = false,
                               AllowSpecialWordDoubleClick = false,
                               BorderStyle = BorderStyle.FixedSingle,
                               Location = new Point(3, 333),
                               MaximumLines = 500,
                               ScrollTo = 0,
                               ShowLineMarker = false,
                               ShowScrollBar = false,
                               Size = new Size(424, 20),
                               UserResize = false,
                               WordWrap = false
                           };

            Controls.AddRange(new Control[] {_lstMessages, _lblTimestamp, _txtTimestamp, _lblString, _txtString, _lblPreview, _preview});

            _lstMessages.Items.AddRange(Functions.EnumUtils.GetDescriptions(typeof (ThemeMessage)));
            _lstMessages.SelectedIndex = 0;

            _txtTimestamp.Text = theme.TimeStampFormat;

            _lstMessages.SelectedIndexChanged += ControlStateChanged;
            _txtString.TextChanged += ControlStateChanged;
            _txtTimestamp.TextChanged += ControlStateChanged;
            ControlStateChanged(_lstMessages, new EventArgs());
        }

        public void SaveSettings()
        {
            /* Empty by default */
        }

        /* Callbacks */
        private void ControlStateChanged(object sender, EventArgs e)
        {
            var c = (Control) sender;
            if (c == null || _lstMessages.SelectedItem == null)
            {
                return;
            }
            if (c.GetType() == typeof (ListBox))
            {
                _textSet = true;
                _txtString.Text = CurrentTheme.Messages[(ThemeMessage) _lstMessages.SelectedIndex].Message;
                return;
            }
            if (!_textSet)
            {
                CurrentTheme.TimeStampFormat = _txtTimestamp.Text;
                CurrentTheme.Messages[(ThemeMessage) _lstMessages.SelectedIndex].Message = _txtString.Text;
                if (ThemeChanged != null)
                {
                    ThemeChanged();
                }
            }
            _textSet = false;
            Preview.Show(_preview, CurrentTheme, (ThemeMessage) _lstMessages.SelectedIndex);
        }
    }
}