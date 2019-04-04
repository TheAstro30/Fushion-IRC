/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ircCore.Controls;

namespace FusionIRC.Forms.Misc
{
    public sealed class FrmAbout : FormEx
    {
        private readonly Panel _pnlIcon;
        private readonly Label _lblFusion;
        private readonly Label _lblCodeName;
        private readonly Label _lblVersion;
        private readonly Label _lblAuthor;
        private readonly Label _lblCopyright;
        private readonly Label _lblDisclaimer;
        private readonly Button _btnClose;

        public FrmAbout()
        {
            ClientSize = new Size(388, 280);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;            
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"About FusionIRC";

            _pnlIcon = new Panel
                           {
                               BackColor = Color.Transparent,
                               BackgroundImageLayout = ImageLayout.Center,
                               BorderStyle = BorderStyle.Fixed3D,
                               Location = new Point(12, 12),
                               Size = new Size(68, 68),
                               BackgroundImage = Properties.Resources.about_dialog
                           };

            _lblFusion = new Label
                             {
                                 AutoSize = true,
                                 BackColor = Color.Transparent,
                                 Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0),
                                 Location = new Point(86, 9),
                                 Size = new Size(112, 30),
                                 Text = @"FusionIRC"
                             };

            _lblCodeName = new Label
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0),
                                   Location = new Point(88, 39),
                                   Size = new Size(145, 20),
                                   Text = @"Code name: ""Nuke"""
                               };

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            _lblVersion = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(89, 63),
                                  Size = new Size(49, 15),
                                  Text =
                                      string.Format("Version: v{0}.{1}.{2} (build: {3})", version.Major, version.Minor,
                                                    version.MinorRevision, version.Build)
                              };

            _lblAuthor = new Label
                             {
                                 AutoSize = true,
                                 BackColor = Color.Transparent,
                                 Location = new Point(89, 92),
                                 Size = new Size(182, 15),
                                 Text = @"Written by: Jason James Newland"
                             };

            _lblCopyright = new Label
                                {
                                    BackColor = Color.Transparent,
                                    Location = new Point(89, 118),
                                    Size = new Size(287, 36),
                                    Text = @"Copyright © 2016 - 2019 KangaSoft Software, All Rights Reserved."
                                };

            _lblDisclaimer = new Label
                                 {
                                     BackColor = Color.Transparent,
                                     Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                                     Location = new Point(88, 176),
                                     Size = new Size(288, 59),
                                     Text = @"THIS SOFTWARE IS PROVIDED ""AS IS"" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS."
                                 };

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Location = new Point(301, 245),
                                Size = new Size(75, 23),
                                TabIndex = 0,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[]
                                  {
                                      _pnlIcon, _lblFusion, _lblCodeName, _lblVersion, _lblAuthor, _lblCopyright, _lblDisclaimer,
                                      _btnClose
                                  });

            AcceptButton = _btnClose;            
        }
    }
}
