/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using ircCore.Controls;
using ircCore.Settings.Networks;

namespace FusionIRC.Forms.Misc
{
    public sealed class FrmNetwork : FormEx
    {
        private readonly Label _lblInfo;
        private readonly ListBox _lstNetwork;
        private readonly Button _btnOk;
        private readonly Button _btnCancel;

        public string Network { get; private set; }

        public FrmNetwork()
        {
            /* Simple form for display all currently stored networks */
            _lblInfo = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(4, 9),
                               Size = new Size(128, 15),
                               Text = @"Installed IRC Networks:"
                           };

            _lstNetwork = new ListBox
                              {
                                  Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                                  FormattingEnabled = true,
                                  IntegralHeight = false,
                                  ItemHeight = 15,
                                  Location = new Point(7, 27),
                                  Size = new Size(202, 161),
                                  TabIndex = 0
                              };

            _btnOk = new Button
                         {
                             DialogResult = DialogResult.OK,
                             Location = new Point(53, 199),
                             Size = new Size(75, 23),
                             TabIndex = 1,
                             Text = @"OK",
                             UseVisualStyleBackColor = true
                         };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(134, 199),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lblInfo, _lstNetwork, _btnOk, _btnCancel});

            AcceptButton = _btnOk;
            ClientSize = new Size(215, 234);
            
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Networks";

            /* Output all installed IRC servers to listbox */
            _lstNetwork.Items.AddRange(ServerManager.GetAllNetworks());
            _lstNetwork.SelectedIndexChanged += OnSelectedIndexChanged;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
            {
                Network = string.Empty;
            }
            base.OnFormClosing(e);
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            Network = _lstNetwork.SelectedItem.ToString();            
        }
    }
}
