/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Utils;

namespace FusionIRC.Forms.Favorites.Editing
{
    public sealed class FrmChannelEdit : FormEx
    {        
        private readonly Label _lblChannel;        
        private readonly TextBox _txtChannel;
        private readonly Label _lblPassword;
        private readonly TextBox _txtPassword;
        private readonly Label _lblDescription;
        private readonly TextBox _txtDescription;
        private readonly Button _btnAdd;
        private readonly Button _btnCancel;

        public string Channel
        {
            get { return _txtChannel.Text; }
            set { _txtChannel.Text = value; }
        }

        public string Password
        {
            get { return _txtPassword.Text; }
            set { _txtPassword.Text = value; }
        }

        public string Description
        {
            get { return _txtDescription.Text; }
            set { _txtDescription.Text = value; }
        }

        public FrmChannelEdit(DialogEditType dialogEditType)
        {
            ClientSize = new Size(260, 186);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Add channel to favorites";

            _lblChannel = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(12, 9),
                                  Size = new Size(87, 15),
                                  Text = @"Channel name:"
                              };

            _txtChannel = new TextBox {Location = new Point(15, 27), Size = new Size(233, 23), TabIndex = 0};

            _lblPassword = new Label
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(12, 53),
                                   Size = new Size(70, 15),
                                   Text = @"Password:"
                               };

            _txtPassword = new TextBox { Location = new Point(15, 71), Size = new Size(233, 23), TabIndex = 1 };

            _lblDescription = new Label
                                  {
                                      AutoSize = true,
                                      BackColor = Color.Transparent,
                                      Location = new Point(12, 97),
                                      Size = new Size(70, 15),
                                      Text = @"Description:"
                                  };

            _txtDescription = new TextBox {Location = new Point(15, 115), Size = new Size(233, 23), TabIndex = 2};

            _btnAdd = new Button
                          {
                              DialogResult = DialogResult.OK,
                              Location = new Point(92, 151),
                              Size = new Size(75, 23),
                              TabIndex = 3,
                              Text = dialogEditType == DialogEditType.Add ? @"Add" : @"Edit",
                              UseVisualStyleBackColor = true
                          };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(173, 151),
                                 Size = new Size(75, 23),
                                 TabIndex = 4,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[]
                                  {
                                      _lblChannel, _txtChannel, _lblPassword, _txtPassword, _lblDescription,
                                      _txtDescription, _btnAdd, _btnCancel
                                  });

            AcceptButton = _btnAdd;
        }
    }
}