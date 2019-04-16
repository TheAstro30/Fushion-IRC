/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Utils;

namespace FusionIRC.Forms.Autos.Editing
{
    public sealed class FrmAutoEdit : FormEx
    {
        private readonly Button _btnAdd;
        private readonly Button _btnCancel;
        private readonly Label _lblItem;
        private readonly Label _lblValue;
        private readonly TextBox _txtItem;
        private readonly TextBox _txtValue;

        public string ItemLabelText
        {
            set { _lblItem.Text = value; }
        }

        public string ValueLabelText
        {
            set { _lblValue.Text = value; }
        }

        public string Item
        {
            get { return _txtItem.Text; }
            set { _txtItem.Text = value; }
        }

        public string Value
        {
            get { return _txtValue.Text; }
            set { _txtValue.Text = value; }
        }

        public FrmAutoEdit(DialogEditType dialogEditType)
        {
            ClientSize = new Size(260, 144);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Add";

            _lblItem = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(9, 9),
                               Size = new Size(34, 15),
                               Text = @"Item:"
                           };

            _txtItem = new TextBox
                           {
                               Location = new Point(12, 27), 
                               Size = new Size(236, 23), 
                               TabIndex = 0
                           };

            _lblValue = new Label
                            {
                                AutoSize = true,
                                BackColor = Color.Transparent,
                                Location = new Point(12, 53),
                                Size = new Size(38, 15),
                                Text = @"Value:"
                            };

            _txtValue = new TextBox
                            {
                                Location = new Point(12, 71), 
                                Size = new Size(236, 23), 
                                TabIndex = 1
                            };

            _btnAdd = new Button
                          {
                              Location = new Point(92, 109),
                              Size = new Size(75, 23),
                              TabIndex = 2,
                              Text = dialogEditType == DialogEditType.Add ? @"Add" : @"Edit",
                              DialogResult = DialogResult.OK,
                              UseVisualStyleBackColor = true
                          };

            _btnCancel = new Button
                             {
                                 Location = new Point(173, 109),
                                 Size = new Size(75, 23),
                                 TabIndex = 3,
                                 Text = @"Cancel",
                                 DialogResult = DialogResult.Cancel,
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblItem, _txtItem, _lblValue, _txtValue, _btnAdd, _btnCancel});

            AcceptButton = _btnAdd;
        }
    }
}