/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;

namespace ircCore.Settings.Theming.Forms
{
    public sealed class FrmNew : FormEx
    {
        private readonly Label _lblHeader;
        private readonly TextBox _txtName;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public FrmNew()
        {            
            ClientSize = new Size(378, 128);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;            
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Create new theme";

            _lblHeader = new Label
                             {
                                 AutoSize = true,
                                 BackColor = Color.Transparent,
                                 Location = new Point(12, 9),
                                 Size = new Size(249, 15),
                                 Text = @"To create a new theme, enter it's name below:"
                             };

            _txtName = new TextBox {Location = new Point(15, 44), Size = new Size(351, 23), TabIndex = 0};

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(210, 93),
                             Size = new Size(75, 23),
                             TabIndex = 1,
                             Text = @"Create",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                                  {
                                      DialogResult = DialogResult.Cancel,
                                      Location = new Point(291, 93),
                                      Size = new Size(75, 23),
                                      TabIndex = 2,
                                      Text = @"Cancel",
                                      UseVisualStyleBackColor = true
                                  };

            Controls.AddRange(new Control[] {_lblHeader, _txtName, _btnOk, _btnCancel});            

            AcceptButton = _btnOk;
        }

        public string ThemeName
        {
            get
            {
                return _txtName.Text;
            }
        }
    }
}
