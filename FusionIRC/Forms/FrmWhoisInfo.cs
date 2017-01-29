/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Controls;
using ircClient.Classes;
using ircCore.Controls;

namespace FusionIRC.Forms
{
    public sealed class FrmWhoisInfo : FormEx
    {
        private readonly Label _lblNick;
        private readonly TextBox _txtNick;
        private readonly Label _lblAddress;
        private readonly TextBox _txtAddress;
        private readonly Label _lblRealName;
        private readonly TextBox _txtRealName;
        private readonly Label _lblAway;
        private readonly TextBox _txtAway;
        private readonly Label _lblServer;
        private readonly TextBox _txtServer;
        private readonly Label _lblChannels;
        private readonly TextBox _txtChannels;
        private readonly Label _lblInfo;
        private readonly TextBox _txtInfo;
        private readonly Button _btnClose;
        
        public FrmWhoisInfo(WhoisInfo info)
        {
            ClientSize = new Size(421, 420);
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
                               Location = new Point(12, 27),
                               ReadOnly = true,
                               Size = new Size(168, 23),
                               TabIndex = 0
                           };

            _lblAddress = new Label
                              {
                                  AutoSize = true,
                                  BackColor = Color.Transparent,
                                  Location = new Point(183, 9),
                                  Size = new Size(52, 15),
                                  Text = @"Address:"
                              };

            _txtAddress = new TextBox
                              {
                                  Location = new Point(186, 27),
                                  ReadOnly = true,
                                  Size = new Size(223, 23),
                                  TabIndex = 1
                              };

            _lblRealName = new Label
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(12, 60),
                                   Size = new Size(65, 15),
                                   Text = @"Real name:"
                               };

            _txtRealName = new TextBox
                               {
                                   Location = new Point(12, 78),
                                   ReadOnly = true,
                                   Size = new Size(168, 23),
                                   TabIndex = 2
                               };

            _lblAway = new Label
                                {
                                    AutoSize = true,
                                    BackColor = Color.Transparent,
                                    Location = new Point(183, 60),
                                    Size = new Size(88, 15),
                                    Text = @"Away message:"
                                };

            _txtAway = new TextBox
                           {
                               Location = new Point(186, 78),
                               ReadOnly = true,
                               Size = new Size(223, 23),
                               TabIndex = 3
                           };

            _lblServer = new Label
                             {
                                 BackColor = Color.Transparent,
                                 Location = new Point(12, 110),
                                 Size = new Size(42, 15),
                                 TabIndex = 13,
                                 Text = @"Server:"
                             };

            _txtServer = new TextBox
                             {
                                 Location = new Point(12, 128),
                                 ReadOnly = true,
                                 Size = new Size(397, 23),
                                 TabIndex = 4
                             };

            _lblChannels = new Label
                               {
                                   AutoSize = true,
                                   BackColor = Color.Transparent,
                                   Location = new Point(12, 162),
                                   Size = new Size(59, 15),
                                   Text = @"Channels:"
                               };

            _txtChannels = new TextBox
                               {
                                   Location = new Point(12, 180),
                                   Multiline = true,
                                   ReadOnly = true,
                                   ScrollBars = ScrollBars.Vertical,
                                   Size = new Size(397, 60),
                                   TabIndex = 5
                               };

            _lblInfo = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(12, 252),
                               Size = new Size(131, 15),
                               Text = @"Additional information:"
                           };

            _txtInfo = new TextBox
                           {
                               Location = new Point(12, 270),
                               Multiline = true,
                               ReadOnly = true,
                               ScrollBars = ScrollBars.Vertical,
                               Size = new Size(397, 100),
                               TabIndex = 6
                           };

            _btnClose = new Button
                            {
                                Location = new Point(334, 385),
                                Size = new Size(75, 23),
                                TabIndex = 7,
                                Tag = "CLOSE",
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };
            _btnClose.Click += ButtonClickHandler;

            Controls.AddRange(new Control[]
                                  {
                                      _lblNick,
                                      _txtNick,
                                      _lblAddress,
                                      _txtAddress,
                                      _lblRealName,
                                      _txtRealName,
                                      _lblAway,
                                      _txtAway,
                                      _lblServer,
                                      _txtServer,
                                      _lblChannels,
                                      _txtChannels,
                                      _lblInfo,
                                      _txtInfo,
                                      _btnClose
                                  });
                        
            AcceptButton = _btnClose;            
                                  
            var whoisInfo = new WhoisInfo(info);
            Text = string.Format("Whois Info: {0}", whoisInfo.Nick);
            _txtNick.Text = whoisInfo.Nick;
            _txtNick.SelectionStart = _txtNick.Text.Length;
            _txtAddress.Text = whoisInfo.Address;
            _txtRealName.Text = whoisInfo.Realname;
            _txtAway.Text = whoisInfo.AwayMessage;
            _txtServer.Text = whoisInfo.Server;
            _txtChannels.Text = whoisInfo.Channels;
            _txtInfo.Text = string.Join(Environment.NewLine, whoisInfo.OtherInfo.ToArray());

            CenterToParent();
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            if (btn.Tag.ToString() == "CLOSE")
            {
                Close();
            }
        }
    }
}
