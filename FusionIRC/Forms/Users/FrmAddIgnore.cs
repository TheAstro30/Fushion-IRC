/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Users;
using ircCore.Utils;

namespace FusionIRC.Forms.Users
{
    public sealed class FrmAddIgnore : FormEx
    {
        private User _user;

        private readonly Label _lblAddress;
        private readonly TextBox _txtAddress;
        private readonly Button _btnAdd;
        private readonly Button _btnCancel;

        public User User
        {
            get { return _user; }
            set 
            {
                _user = value;
                _txtAddress.Text = _user.Address;
            }
        }

        public FrmAddIgnore(UserEditType userEditType)
        {
            ClientSize = new Size(377, 99);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            _lblAddress = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(12, 9),
                                  Size = new Size(112, 15),
                                  Text = @"Nick/Address mask:"
                              };

            _txtAddress = new TextBox
                              {
                                  Location = new Point(15, 27),
                                  Size = new Size(350, 23),
                                  TabIndex = 0
                              };

            _btnAdd = new Button
                          {
                              DialogResult = DialogResult.OK,
                              Location = new Point(209, 64),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Text = userEditType == UserEditType.Add ? @"Add" : @"Edit",
                              UseVisualStyleBackColor = true
                          };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(290, 64),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblAddress, _txtAddress, _btnAdd, _btnCancel});

            AcceptButton = _btnAdd;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                _user.Nick = null;
                _user.Note = null;
                _user.Address = Functions.GetFirstWord(_txtAddress.Text);
            }
            base.OnFormClosing(e);
        }
    }
}
