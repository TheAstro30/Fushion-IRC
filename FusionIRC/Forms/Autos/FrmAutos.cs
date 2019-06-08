/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Autos.Controls;
using FusionIRC.Properties;
using ircCore.Autos;
using ircCore.Controls;
using ircCore.Settings;

namespace FusionIRC.Forms.Autos
{
    public sealed class FrmAutos : FormEx
    {        
        private readonly TabControl _tabAutos;
        private readonly TabPage _tabConnect;
        private readonly TabPage _tabJoin;
        private readonly TabPage _tabIdentify;
        private readonly TabPage _tabPerform;

        private readonly AutomationsListView _autoConnect;
        private readonly AutomationsListView _autoJoin;
        private readonly AutomationsListView _autoIdentify;
        private readonly AutomationPerform _autoPerform;

        private readonly Button _btnClose;

        private readonly ImageList _images;

        public FrmAutos()
        {
            /* Form code */
            ClientSize = new Size(421, 421);
            Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Automations";

            _images = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };
            _images.Images.AddRange(new Image[]
                                        {
                                            Resources.autoConnect.ToBitmap(),
                                            Resources.autoJoin.ToBitmap(),
                                            Resources.identify.ToBitmap(),
                                            Resources.autoPerform.ToBitmap()
                                        });

            _tabAutos = new TabControl
                            {
                                Location = new Point(12, 12),
                                SelectedIndex = 0,
                                Size = new Size(397, 368),
                                ImageList = _images,
                                TabIndex = 0
                            };

            /* Auto connect tab */
            _tabConnect = new TabPage
                              {
                                  Location = new Point(4, 22),
                                  Padding = new Padding(3),
                                  Size = new Size(389, 342),
                                  TabIndex = 0,
                                  Text = @"Connect",
                                  UseVisualStyleBackColor = true,
                                  ImageIndex = 0
                              };
            _autoConnect = new AutomationsListView(AutomationsManager.AutomationType.Connect)
                               {
                                   Dock = DockStyle.Fill,
                                   Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                   Location = new Point(3, 3),
                                   Size = new Size(383, 336)
                               };
            _tabConnect.Controls.Add(_autoConnect);
            
            /* Auto join tab */
            _tabJoin = new TabPage
                           {
                               Location = new Point(4, 22),
                               Padding = new Padding(3),
                               Size = new Size(389, 342),
                               TabIndex = 1,
                               Text = @"Join",
                               UseVisualStyleBackColor = true,
                               ImageIndex = 1
                           };

            _autoJoin = new AutomationsListView(AutomationsManager.AutomationType.Join)
                            {
                                Dock = DockStyle.Fill,
                                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                Location = new Point(3, 3),
                                Size = new Size(383, 336)
                            };
            _tabJoin.Controls.Add(_autoJoin);

            /* Auto identify tab */
            _tabIdentify = new TabPage
                               {
                                   Location = new Point(4, 22),
                                   Padding = new Padding(3),
                                   Size = new Size(389, 342),
                                   TabIndex = 2,
                                   Text = @"Identify",
                                   UseVisualStyleBackColor = true,
                                   ImageIndex = 2
                               };

            _autoIdentify = new AutomationsListView(AutomationsManager.AutomationType.Identify)
                                {
                                    Dock = DockStyle.Fill,
                                    Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                    Location = new Point(3, 3),
                                    Size = new Size(383, 336)
                                };
            _tabIdentify.Controls.Add(_autoIdentify);

            /* Auto perform tab */
            _tabPerform = new TabPage
                              {
                                  Location = new Point(4, 22),
                                  Padding = new Padding(3),
                                  Size = new Size(389, 342),
                                  TabIndex = 3,
                                  Text = @"Perform",
                                  UseVisualStyleBackColor = true,
                                  ImageIndex = 3
                              };

            _autoPerform = new AutomationPerform
                               {
                                   Dock = DockStyle.Fill,
                                   Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0))),
                                   Location = new Point(3, 3),
                                   Size = new Size(383, 336)
                               };
            _tabPerform.Controls.Add(_autoPerform);

            /* Add tab pages to tab control */
            _tabAutos.Controls.AddRange(new Control[] {_tabConnect, _tabJoin, _tabIdentify, _tabPerform});
            _tabAutos.SelectedIndex = SettingsManager.Settings.Client.Tabs.AutoList;

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.OK,
                                Location = new Point(334, 386),
                                Size = new Size(75, 23),
                                TabIndex = 4,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_tabAutos, _btnClose});
            _tabAutos.SelectedIndexChanged += TabSelectedIndexChanged;
        }

        private void TabSelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsManager.Settings.Client.Tabs.AutoList = _tabAutos.SelectedIndex;
        }
    }
}