/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;

namespace FusionIRC.Forms.Script
{
    public sealed class FrmRename : FormEx
    {
        private readonly Label _lblInfo;
        private readonly TextBox _txtName;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public string FileName
        {
            get { return _txtName.Text; }
            set { _txtName.Text = value; }
        }

        public FrmRename()
        {
            ClientSize = new Size(277, 100);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Rename Script";

            _lblInfo = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(12, 9),
                               Size = new Size(146, 15),
                               Text = @"Enter new script file name:"
                           };

            _txtName = new TextBox {Location = new Point(15, 27), Size = new Size(250, 23), TabIndex = 0};

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(109, 65),
                             Size = new Size(75, 23),
                             TabIndex = 1,
                             Text = @"Rename",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(190, 65),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblInfo, _txtName, _btnOk, _btnCancel});

            AcceptButton = _btnOk;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }
    }
}