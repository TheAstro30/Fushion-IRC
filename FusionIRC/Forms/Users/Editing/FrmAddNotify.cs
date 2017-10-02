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

namespace FusionIRC.Forms.Users.Editing
{
    public sealed class FrmAddNotify : FormEx
    {
        private User _user;
        
        private readonly Label _lblNick;
        private readonly TextBox _txtNick;
        private readonly Label _lblNote;
        private readonly TextBox _txtNote;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public User User
        {
            get { return _user; }
            set
            {
                _user = value;
                _txtNick.Text = _user.Nick;
                _txtNote.Text = _user.Note;
            }
        }

        public FrmAddNotify(UserEditType userEditType)
        {
            ClientSize = new Size(377, 188);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            ShowIcon = false;

            _lblNick = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(12, 9),
                               Size = new Size(34, 15),
                               Text = @"Nick:"
                           };

            _txtNick = new TextBox
                           {
                               Location = new Point(15, 27),
                               Size = new Size(350, 23),
                               TabIndex = 0
                           };

            _lblNote = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(12, 68),
                               Size = new Size(83, 15),
                               Text = @"Optional note:"
                           };

            _txtNote = new TextBox
                           {
                               Location = new Point(15, 86),
                               Multiline = true,
                               ScrollBars = ScrollBars.Vertical,
                               Size = new Size(350, 55),
                               TabIndex = 1
                           };

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(209, 153),
                             Size = new Size(75, 23),
                             TabIndex = 2,
                             Text = userEditType == UserEditType.Add ? @"Add" : @"Edit",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(290, 153),
                                 Size = new Size(75, 23),
                                 TabIndex = 3,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblNick, _txtNick, _lblNote, _txtNote, _btnOk, _btnCancel});

            AcceptButton = _btnOk;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                _user.Nick = Functions.GetFirstWord(_txtNick.Text);
                _user.Note = !string.IsNullOrEmpty(_txtNote.Text) ? _txtNote.Text : null;
            }
            base.OnFormClosing(e);
        }
    }
}
