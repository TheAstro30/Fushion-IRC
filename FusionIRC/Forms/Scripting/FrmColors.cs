/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Settings;

namespace FusionIRC.Forms.Scripting
{
    public sealed class FrmColors : FormEx
    {
        private readonly Label _lblInfo;
        private readonly Label _lblComment;
        private readonly ColorPickerComboBox _cmbComment;
        private readonly Label _lblCommand;
        private readonly ColorPickerComboBox _cmbCommand;
        private readonly Label _lblId1;
        private readonly ColorPickerComboBox _cmbId1;
        private readonly Label _lblId2;
        private readonly ColorPickerComboBox _cmbId2;
        private readonly Label _lblKeywords;
        private readonly ColorPickerComboBox _cmbKeywords;
        private readonly Label _lblMisc;
        private readonly ColorPickerComboBox _cmbMisc;
        private readonly Label _lblVar;
        private readonly ColorPickerComboBox _cmbVar;            
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public FrmColors()
        {
            ClientSize = new Size(309, 331);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Editor Highlighting Colors";

            _lblInfo = new Label
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(12, 9),
                               Size = new Size(285, 51),
                               Text =
                                   @"Here, you can change the colors used during highlighting of script code for different highlight styles:"
                           };

            _lblComment = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(12, 69),
                                  Size = new Size(106, 15),
                                  Text = @"Commented code:"
                              };

            _cmbComment = new ColorPickerComboBox
                              {Location = new Point(157, 63), Size = new Size(140, 26), TabIndex = 0};

            _lblKeywords = new Label
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(12, 101),
                                   Size = new Size(61, 15),
                                   Text = @"Keywords:"
                               };

            _cmbKeywords = new ColorPickerComboBox
                               {Location = new Point(157, 95), Size = new Size(140, 26), TabIndex = 1};

            _lblVar = new Label
                          {
                              AutoSize = true,
                              BackColor = Color.Transparent,
                              Location = new Point(12, 133),
                              Size = new Size(77, 15),
                              Text = @"Variables (%):"
                          };

            _cmbVar = new ColorPickerComboBox {Location = new Point(157, 127), Size = new Size(140, 26), TabIndex = 2};

            _lblId1 = new Label
                          {
                              AutoSize = true,
                              BackColor = Color.Transparent,
                              Location = new Point(12, 165),
                              Size = new Size(79, 15),
                              Text = @"Internal Identifiers ($):"
                          };

            _cmbId1 = new ColorPickerComboBox {Location = new Point(157, 159), Size = new Size(140, 26), TabIndex = 3};

            _lblId2 = new Label
                          {
                              AutoSize = true,
                              BackColor = Color.Transparent,
                              Location = new Point(12, 197),
                              Size = new Size(124, 15),
                              Text = @"Aliases As Identifiers ($):"
                          };

            _cmbId2 = new ColorPickerComboBox {Location = new Point(157, 191), Size = new Size(140, 26), TabIndex = 4};

            _lblCommand = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(12, 229),
                                  Size = new Size(72, 15),
                                  Text = @"Commands:"
                              };

            _cmbCommand = new ColorPickerComboBox
                              {Location = new Point(157, 223), Size = new Size(140, 26), TabIndex = 5};

            _lblMisc = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(12, 261),
                               Size = new Size(35, 15),
                               Text = @"Other Commands:"
                           };

            _cmbMisc = new ColorPickerComboBox {Location = new Point(157, 255), Size = new Size(140, 26), TabIndex = 6};

            _btnOk = new Button
                         {
                             Location = new Point(141, 296),
                             Size = new Size(75, 23),
                             TabIndex = 7,
                             Text = @"OK",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                             {
                                 Location = new Point(222, 296),
                                 Size = new Size(75, 23),
                                 TabIndex = 8,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[]
                                  {
                                      _lblInfo, _lblComment, _cmbComment, _lblCommand, _cmbCommand, _lblKeywords,
                                      _cmbKeywords, _lblId1, _cmbId1, _lblId2, _cmbId2, _lblVar, _cmbVar, _lblMisc,
                                      _cmbMisc, _btnOk, _btnCancel
                                  });

            AcceptButton = _btnOk;

            /* Init combo boxes */
            _cmbComment.SelectedText = SettingsManager.Settings.Editor.Colors.CommentColor.Name;
            _cmbKeywords.SelectedText = SettingsManager.Settings.Editor.Colors.KeyWordColor.Name;
            _cmbCommand.SelectedText = SettingsManager.Settings.Editor.Colors.CommandColor.Name;
            _cmbVar.SelectedText = SettingsManager.Settings.Editor.Colors.VariableColor.Name;
            _cmbId1.SelectedText = SettingsManager.Settings.Editor.Colors.IdentifierColor.Name;
            _cmbId2.SelectedText = SettingsManager.Settings.Editor.Colors.CustomIdentifierColor.Name;
            _cmbMisc.SelectedText = SettingsManager.Settings.Editor.Colors.MiscColor.Name;

            _btnOk.Click += ButtonClickHandler;
            _btnCancel.Click += ButtonClickHandler;
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            switch (btn.Text.ToUpper())
            {
                case "OK":
                    SettingsManager.Settings.Editor.Colors.CommentColor = _cmbComment.SelectedItem.Color;
                    SettingsManager.Settings.Editor.Colors.KeyWordColor = _cmbKeywords.SelectedItem.Color;
                    SettingsManager.Settings.Editor.Colors.CommandColor = _cmbCommand.SelectedItem.Color;
                    SettingsManager.Settings.Editor.Colors.VariableColor = _cmbVar.SelectedItem.Color;
                    SettingsManager.Settings.Editor.Colors.IdentifierColor = _cmbId1.SelectedItem.Color;
                    SettingsManager.Settings.Editor.Colors.CustomIdentifierColor = _cmbId2.SelectedItem.Color;
                    SettingsManager.Settings.Editor.Colors.MiscColor = _cmbMisc.SelectedItem.Color;
                    DialogResult = DialogResult.OK;
                    break;

                default:
                    DialogResult = DialogResult.Cancel;
                    break;
            }
            Close();
        }
    }
}