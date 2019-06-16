/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FusionIRC.Properties;
using ircClient.Tcp;
using ircCore.Controls;

namespace FusionIRC.Forms.DirectClientConnection
{
    public sealed class FrmDccExists : FormEx
    {        
        private readonly Panel _pnlIcon;
        private readonly Label _lblTitle;
        private readonly Label _lblFilename;
        private readonly Label _lblInfo;
        private readonly Button _btnSave;
        private readonly Button _btnOverwrite;
        private readonly Button _btnCancel;

        public string FileName { get; private set; }

        public DccWriteMode WriteMode { get; private set; }

        public FrmDccExists(DccWriteMode mode, string fileName)
        {
            ClientSize = new Size(364, 158);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"FusionIRC - DCC Get";

            _pnlIcon = new Panel
                           {
                               BackColor = Color.Transparent,
                               BackgroundImageLayout = ImageLayout.Center,
                               BackgroundImage = Resources.dccManager.ToBitmap(),
                               Location = new Point(12, 12),
                               Size = new Size(64, 64)
                           };

            _lblTitle = new Label
                            {
                                AutoSize = true,
                                BackColor = Color.Transparent,
                                Location = new Point(82, 12),
                                Size = new Size(174, 15),
                                Text = @"The following file already exists:"
                            };

            _lblFilename = new Label
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                                   Location = new Point(82, 43),
                                   Size = new Size(57, 15)
                               };

            _lblInfo = new Label
                           {
                               BackColor = Color.Transparent,
                               Location = new Point(82, 75),
                               Size = new Size(270, 33),
                               Text = @"You can overwrite the file or save the file as another name"
                           };

            _btnSave = new Button
                           {
                               Location = new Point(115, 123),
                               Size = new Size(75, 23),
                               TabIndex = 0,
                               UseVisualStyleBackColor = true
                           };

            _btnOverwrite = new Button
                                {
                                    DialogResult = DialogResult.OK,
                                    Location = new Point(196, 123),
                                    Size = new Size(75, 23),
                                    TabIndex = 1,
                                    Text = @"Overwrite",
                                    UseVisualStyleBackColor = true
                                };

            _btnCancel = new Button
                             {
                                 DialogResult = DialogResult.Cancel,
                                 Location = new Point(277, 123),
                                 Size = new Size(75, 23),
                                 TabIndex = 2,
                                 Text = @"Cancel",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_pnlIcon, _lblTitle, _lblFilename, _lblInfo, _btnSave, _btnOverwrite, _btnCancel});

            AcceptButton = _btnCancel;

            FileName = fileName;            
            switch (mode)
            {
                case DccWriteMode.SaveAs:
                    _lblInfo.Text = @"You can overwrite the file or save the file as another name";
                    _btnSave.Text = @"Save As";
                    break;

                case DccWriteMode.Resume:
                    _lblInfo.Text = @"You can overwrite the file or you can attempt to resume it";
                    _btnSave.Text = @"Resume";
                    break;
            }
            _lblFilename.Text = Path.GetFileName(fileName);

            _btnOverwrite.Click += ButtonClickHandler;
            _btnSave.Click += ButtonClickHandler;
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
                case "OVERWRITE":
                    WriteMode = DccWriteMode.Overwrite;
                    break;

                case "SAVE AS":
                    WriteMode = DccWriteMode.Overwrite;
                    using (var sfd = new SaveFileDialog
                                         {
                                             Title = @"Select a new file name to save to",
                                             Filter = @"All Files (*.*)|*.*",
                                             FileName = Path.GetFileName(FileName),
                                             CheckFileExists = true
                                         })
                    {
                        if (sfd.ShowDialog(this) == DialogResult.Cancel)
                        {
                            DialogResult = DialogResult.Cancel;
                            Close();
                            return;
                        }
                        FileName = sfd.FileName;
                    }
                    break;

                case "RESUME":
                    WriteMode = DccWriteMode.Resume;
                    break;
            }       
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
