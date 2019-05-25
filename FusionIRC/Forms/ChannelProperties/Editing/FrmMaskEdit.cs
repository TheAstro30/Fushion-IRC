/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Utils;

namespace FusionIRC.Forms.ChannelProperties.Editing
{
    public sealed class FrmEditChannelList : FormEx
    {        
        private readonly Label _lblPrompt;
        private readonly TextBox _txtMask;
        private readonly Button _btnAdd;
        private readonly Button _btnCancel;

        public string Prompt
        {
            set { _lblPrompt.Text = value; }
        }

        public string Mask
        {
            get { return _txtMask.Text; }
            set { _txtMask.Text = value; }
        }

        public FrmEditChannelList(DialogEditType dialogEditType)
        {
            ClientSize = new Size(306, 108);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = dialogEditType == DialogEditType.Add ? @"Add Host Mask" : @"Edit Host Mask";

            _lblPrompt = new Label
                             {
                                 AutoSize = true,
                                 BackColor = Color.Transparent,
                                 Location = new Point(9, 9),
                                 Size = new Size(50, 15)
                             };

            _txtMask = new TextBox {Location = new Point(12, 27), Size = new Size(282, 23), TabIndex = 0};

            _btnAdd = new Button
                          {
                              DialogResult = DialogResult.OK,
                              Location = new Point(138, 73),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Text = dialogEditType == DialogEditType.Add ? @"Add" : "Edit",
                              UseVisualStyleBackColor = true
                          };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(219, 73),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblPrompt, _txtMask, _btnAdd, _btnCancel});

            AcceptButton = _btnAdd;            
        }
    }
}
