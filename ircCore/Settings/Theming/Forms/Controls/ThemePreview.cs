/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.OutputDisplay;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;

namespace ircCore.Settings.Theming.Forms.Controls
{
    public sealed class ThemePreview : UserControl
    {
        private readonly Label _lblTheme;
        private readonly ComboBox _cmbThemes;
        private readonly Button _btnNew;
        private readonly Button _btnDelete;
        private readonly Label _lblPreview;
        private readonly OutputWindow _preview;

        private Theme _theme;

        public ThemePreview(Theme theme)
        {            
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);            
            Size = new Size(571, 408);

            _lblTheme = new Label
                            {
                                AutoSize = true, Location = new Point(8, 8), Size = new Size(47, 15), Text = @"Theme:"
                            };

            _cmbThemes = new ComboBox
                             {
                                 DropDownStyle = ComboBoxStyle.DropDownList,
                                 FormattingEnabled = true,
                                 Location = new Point(11, 26),
                                 Size = new Size(266, 23),
                                 TabIndex = 0
                             };

            _btnNew = new Button
                          {
                              Location = new Point(283, 26),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Text = @"New...",
                              Tag = "NEW",
                              UseVisualStyleBackColor = true
                          };

            _btnDelete = new Button
                             {
                                 Location = new Point(364, 26),
                                 Size = new Size(75, 23),
                                 TabIndex = 1,
                                 Text = @"Delete",
                                 Tag = "DELETE",
                                 UseVisualStyleBackColor = true,
                                 Enabled = false
                             };

            _lblPreview = new Label
                              {
                                  AutoSize = true,
                                  Location = new Point(8, 64),
                                  Size = new Size(51, 15),
                                  Text = @"Preview:"
                              };

            _preview = new OutputWindow
                                {
                                    Location = new Point(11, 82),
                                    Size = new Size(548, 318),
                                    ShowScrollBar = false,
                                    AllowCopySelection = false,
                                    LineSpacingStyle = LineSpacingStyle.Paragraph,
                                    BorderStyle = BorderStyle.Fixed3D
                                };

            Controls.AddRange(new Control[] {_lblTheme, _cmbThemes, _btnNew, _btnDelete, _lblPreview, _preview});
            /* Import stored themes */
            for (var i = 0; i <= SettingsManager.Settings.Themes.Theme.Count -1; i++)
            {
                _cmbThemes.Items.Add(SettingsManager.Settings.Themes.Theme[i]);
                if (i == SettingsManager.Settings.Themes.CurrentTheme)
                {
                    _cmbThemes.SelectedIndex = i;
                }
            }
            _btnDelete.Enabled = _cmbThemes.SelectedIndex != 0;
            _cmbThemes.SelectedIndexChanged += ComboSelectedIndexChanged;
            _btnNew.Click += ButtonClickHandler;
            _btnDelete.Click += ButtonClickHandler;
            /* Preview */
            _theme = theme;
            BuildTextPreview();
        }

        public int ThemeIndex
        {
            get
            {
                return _cmbThemes.SelectedIndex;
            }
        }

        public SettingsTheme.ThemeListData ThemeData
        {
            get
            {
                return (SettingsTheme.ThemeListData)_cmbThemes.SelectedItem;
            }
        }

        private void ComboSelectedIndexChanged(object sender, EventArgs e)
        {
            BuildTextPreview();
            /* Index 0 will always be the default theme which cannot be deleted */
            _btnDelete.Enabled = _cmbThemes.SelectedIndex != 0;
        }

        private void ButtonClickHandler(object sender, EventArgs e)
        {
            var btn = (Button) sender;
            if (btn == null)
            {
                return;
            }
            SettingsTheme.ThemeListData t;
            switch (btn.Tag.ToString())
            {
                case "NEW":
                    using (var f = new FrmNew())
                    {
                        f.ShowDialog(this);
                        if (!string.IsNullOrEmpty(f.ThemeName))
                        {
                            /* Create a new theme */
                            t = new SettingsTheme.ThemeListData
                                    {
                                        Name = f.ThemeName,
                                        Path = string.Format(@"\themes\{0}.thm", Utils.Functions.CleanFileName(f.ThemeName))
                                    };                            
                            SettingsManager.Settings.Themes.Theme.Add(t);
                            _theme = new Theme
                                         {
                                             Name = t.Name
                                         };
                            ThemeManager.Save(Utils.Functions.MainDir(t.Path, false), _theme);
                            _cmbThemes.Items.Add(t);
                            _cmbThemes.SelectedIndex = _cmbThemes.Items.Count - 1;
                            BuildTextPreview();
                        }
                    }
                    break;

                case "DELETE":
                    /* Make sure to load a new theme if this theme deleted is the current selected theme */
                    var sel = _cmbThemes.SelectedIndex - 1;
                    t = (SettingsTheme.ThemeListData) _cmbThemes.Items[sel];
                    var deleted = (SettingsTheme.ThemeListData)_cmbThemes.SelectedItem;
                    if (t == null)
                    {
                        return;
                    }                    
                    /* Delete theme data */
                    SettingsManager.Settings.Themes.Theme.RemoveAt(_cmbThemes.SelectedIndex);
                    var path = Utils.Functions.MainDir(deleted.Path, false);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    _cmbThemes.Items.RemoveAt(_cmbThemes.SelectedIndex);
                    /* Select previous theme and load it */
                    var theme = new Theme();
                    path = Utils.Functions.MainDir(t.Path, false);
                    ThemeManager.Load(path, ref theme, true);
                    _theme = theme;                    
                    _cmbThemes.SelectedIndex = sel;
                    BuildTextPreview();                    
                    SettingsManager.Settings.Themes.CurrentTheme = _cmbThemes.SelectedIndex;
                    ThemeManager.Load(path); /* Make sure client is updated */
                    break;
            }
        }

        private void BuildTextPreview()
        {
            /* Copy the selected theme */
            var t = (SettingsTheme.ThemeListData)_cmbThemes.SelectedItem;
            if (t == null)
            {
                return;
            }
            var theme = new Theme();
            ThemeManager.Load(Utils.Functions.MainDir(t.Path, false), ref theme, true);
            _theme = theme;
            _preview.Clear();
            _preview.Font = new Font(ThemeManager.GetFont(ChildWindowType.Channel).Name, 10); /* We keep the size static so it fits the preview */
            _preview.BackColor = ThemeManager.GetColor(ThemeColor.OutputWindowBackColor);
            _preview.ForeColor = ThemeManager.GetColor(ThemeColor.OutputWindowForeColor);
            /* Set some text data */
            ThemePreviewText(ThemeMessage.ChannelSelfJoinText, "", "", "", "", "", "#themePreview", "");
            ThemePreviewText(ThemeMessage.ChannelTopic, "", "", "", "", "", "", "Preview of the current selected theme");
            ThemePreviewText(ThemeMessage.ChannelTopicSet, "", "", "", "", "", "", string.Format("FusionIRC {0}", DateTime.Now));
            ThemePreviewText(ThemeMessage.ChannelSelfText, "yourself", "", "", "", "", "", "hi, what's up guys?");
            ThemePreviewText(ThemeMessage.ChannelText, "someGuy", "@", "~fusion@fusion.com", "", "", "", "yourself: the sky, dude...");
            ThemePreviewText(ThemeMessage.ChannelActionText, "someGuy", "@", "~fusion@fusion.com", "", "", "", "thinks he is funny :P");
            ThemePreviewText(ThemeMessage.ChannelSelfActionText, "yourself", "", "", "", "", "", "lols at someGuy");
            ThemePreviewText(ThemeMessage.ChannelJoinText, "aNewDude", "", "~new@new.com", "", "", "#themePreview", "");
            ThemePreviewText(ThemeMessage.ModeChannelText, "someGuy", "@", "~fusion@fusion.com", "", "", "", "+b *!*@new.com");
            ThemePreviewText(ThemeMessage.ChannelKickText, "someGuy", "@", "~fusion@fusion.com", "", "aNewDude", "#themePreview", "You can leave now");
            ThemePreviewText(ThemeMessage.NickChangeUserText, "someGuy", "@", "~fusion@fusion.com", "NotHere", "", "", "");
            ThemePreviewText(ThemeMessage.QuitText, "NotHere", "@", "~fusion@fusion.com", "", "", "", "Quit: Leaving.");
        }

        private void ThemePreviewText(ThemeMessage message, string nick, string prefix, string address, string newNick, string kickedNick, string target, string text)
        {
            var tmd = new IncomingMessageData
                          {
                              Message = message,
                              TimeStamp = DateTime.Now,
                              Nick = nick,
                              Prefix = prefix,
                              NewNick = newNick,
                              KickedNick = kickedNick,
                              Address = address,
                              Target = target,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            _preview.AddLine(pmd.DefaultColor, pmd.Message);
        }
    }
}
