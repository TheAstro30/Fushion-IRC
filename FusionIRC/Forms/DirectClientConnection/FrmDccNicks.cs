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
using ircCore.Settings.SettingsBase.Structures;
using libolv;

namespace FusionIRC.Forms.DirectClientConnection
{
    public sealed class FrmDccNicks : FormEx
    {        
        private readonly TextBox _txtNick;
        private readonly Button _btnSelect;
        private readonly ObjectListView _lvNicks;
        private readonly OlvColumn _colNicks;
        private readonly Button _btnClear;
        private readonly Button _btnCancel;
        
        public string NickName
        {
            get { return _txtNick.Text; }            
        }

        public FrmDccNicks()
        {
            ClientSize = new Size(252, 247);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"FusionIRC - DCC recent nicks";

            _txtNick = new TextBox {Location = new Point(12, 12), Size = new Size(147, 23), TabIndex = 0};

            _btnSelect = new Button
                             {
                                 DialogResult = DialogResult.OK,
                                 Enabled = false,
                                 Location = new Point(165, 12),
                                 Size = new Size(75, 23),
                                 TabIndex = 1,
                                 Text = @"Select",
                                 UseVisualStyleBackColor = true
                             };

            _lvNicks = new ObjectListView
                           {
                               FullRowSelect = true,
                               HeaderStyle = ColumnHeaderStyle.None,
                               HideSelection = false,
                               Location = new Point(12, 41),
                               MultiSelect = false,
                               Size = new Size(147, 157),
                               TabIndex = 2,
                               UseCompatibleStateImageBehavior = false,
                               View = View.Details
                           };

            _colNicks = new OlvColumn("Nicks:", "Nick")
                            {
                                CellPadding = null,
                                IsEditable = false,
                                Sortable = false,
                                Width = 120,
                                FillsFreeSpace = true,
                            };
            _lvNicks.AllColumns.Add(_colNicks);
            _lvNicks.Columns.Add(_colNicks);

            _btnClear = new Button
                            {
                                Location = new Point(165, 175),
                                Size = new Size(75, 23),
                                TabIndex = 3,
                                Text = @"Clear",
                                UseVisualStyleBackColor = true
                            };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(165, 212),
                                 Size = new Size(75, 23),
                                 TabIndex = 4,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_txtNick, _btnSelect, _lvNicks, _btnClear, _btnCancel});
            
            _lvNicks.SetObjects(SettingsManager.Settings.Dcc.History.Data);

            _txtNick.TextChanged += TextNickTextChanged;
            _lvNicks.DoubleClick += ListDoubleClicked;
            _btnClear.Click += ButtonClickHandler;

            AcceptButton = _btnSelect;
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            /* Clear history */
            _lvNicks.ClearObjects();
            SettingsManager.Settings.Dcc.History.Data.Clear();
        }

        private void TextNickTextChanged(object sender, EventArgs e)
        {
            _btnSelect.Enabled = !string.IsNullOrEmpty(_txtNick.Text);
        }

        private void ListDoubleClicked(object sender, EventArgs e)
        {
            if (_lvNicks.SelectedObject == null)
            {
                return;
            }
            var d = (SettingsDcc.SettingsDccHistory.SettingsDccHistoryData) _lvNicks.SelectedObject;
            _txtNick.Text = d.ToString();
        }
    }
}
