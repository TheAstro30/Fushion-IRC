/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FusionIRC.Forms.Theming.Helpers;
using ircCore.Controls.ChildWindows.Input.ColorBox;
using ircCore.Controls.ChildWindows.OutputDisplay;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms.Theming.Controls
{
    public sealed class ThemeColors : UserControl, IThemeSetting
    {
        private readonly TreeView _tvColors;        
        private readonly Label _lblCurrent;
        private readonly ColorSelectionBox _colors;
        private readonly Label _lblInfo;
        private readonly Label _lblPreview;
        private readonly OutputWindow _preview;
        
        private Theme _theme;

        public event Action ThemeChanged;

        public Theme CurrentTheme
        {
            get { return _theme; }
            set
            {
                _theme = value;
                _tvColors.SelectedNode = _tvColors.Nodes[0].Nodes[0];
                BeginSelection(_tvColors.SelectedNode, false);
            }
        }      

        public ThemeColors(Theme theme)
        {
            _theme = theme;

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            Size = new Size(438, 360);

            _tvColors = new TreeView
                            {
                                BorderStyle = BorderStyle.FixedSingle,
                                HideSelection = false,
                                FullRowSelect = true,
                                Location = new Point(3, 3),
                                Size = new Size(233, 246),
                                TabIndex = 0
                            };

            _lblCurrent = new Label
                              {
                                  AutoSize = true,
                                  Location = new Point(3, 252),
                                  Size = new Size(80, 15),
                                  TabIndex = 2,
                                  Text = @"Current color:"
                              };

            _colors = new ColorSelectionBox
                          {
                              Location = new Point(6, 270),
                              SelectedColor = 0,
                              ShowFocusRectangle = true,
                              Size = new Size(337, 22)
                          };

            _lblInfo = new Label
                           {
                               AutoSize = true,
                               Location = new Point(3, 295),
                               Size = new Size(411, 15),
                               TabIndex = 1,
                               Text = @"(Note: to change the RGB values of each color, right-click its associated box)"
                           };

            _lblPreview = new Label
                              {
                                  AutoSize = true,
                                  Location = new Point(3, 315),
                                  Size = new Size(51, 15),
                                  TabIndex = 4,
                                  Text = @"Preview:"
                              };

            _preview = new OutputWindow
                           {
                               AllowCopySelection = false,
                               AllowSpecialWordDoubleClick = false,
                               BorderStyle = BorderStyle.FixedSingle,
                               LineSpacingStyle = LineSpacingStyle.Single,
                               Location = new Point(3, 333),
                               MaximumLines = 1,
                               ScrollTo = 0,
                               ShowLineMarker = false,
                               ShowScrollBar = false,
                               Size = new Size(424, 20),
                               UserResize = false,
                               WordWrap = false
                           };

            for (var i = 0; i <= 15; i++)
            {
                _colors.SetBoxColor(i, _theme.Colors[i]);
            }

            Controls.AddRange(new Control[] {_tvColors, _lblPreview, _preview, _lblCurrent, _colors, _lblInfo});

            /* Init treeview */
            _tvColors.Nodes.Add(BuildTreeview("Windows", Functions.EnumUtils.GetDescriptions(typeof (ThemeColor))));
            _tvColors.Nodes.Add(BuildTreeview("Messages", Functions.EnumUtils.GetDescriptions(typeof (ThemeMessage))));

            _tvColors.AfterSelect += TreeviewAfterSelect;
            _colors.MouseDown += ColorSelectionMouseDown;

            _tvColors.SelectedNode = _tvColors.Nodes[0].Nodes[0];
            BeginSelection(_tvColors.SelectedNode, false);
        }

        public void SaveSettings()
        {
            /* Empty by default */
        }

        /* Callbacks */
        private void TreeviewAfterSelect(object sender, EventArgs e)
        {
            BeginSelection(_tvColors.SelectedNode, false);
        }

        private void ColorSelectionMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    BeginSelection(_tvColors.SelectedNode, true);
                    if (ThemeChanged != null)
                    {
                        ThemeChanged();
                    }
                    break;

                case MouseButtons.Right:
                    using (var c = new ColorDialog { FullOpen = true, Color = _theme.Colors[_colors.SelectedColor] })
                    {
                        if (c.ShowDialog(this) == DialogResult.Cancel)
                        {
                            return;
                        }
                        _theme.Colors[_colors.SelectedColor] = c.Color;
                        _colors.SetBoxColor(_colors.SelectedColor, c.Color);
                        BeginSelection(_tvColors.SelectedNode, false);
                        if (ThemeChanged != null)
                        {
                            ThemeChanged();
                        }
                    }
                    break;
            }
        }

        private TreeNode BuildTreeview(string nodeText, IEnumerable<object> descriptions)
        {
            var node = new TreeNode {Text = nodeText};
            var count = 0;
            foreach (var v in descriptions)
            {
                var n = new TreeNode {Text = v.ToString(), Tag = count};
                node.Nodes.Add(n);
                if (count == 0 && nodeText == "Windows")
                {
                    _tvColors.SelectedNode = n;
                }
                count++;
            }
            node.Expand();
            return node;
        }

        private void BeginSelection(TreeNode n, bool edit)
        {
            if (n == null || n.Parent == null)
            {
                return;
            }
            int val;
            if (!int.TryParse(n.Tag.ToString(), out val))
            {
                val = 0;
            }
            var message = false;
            switch (n.Parent.Text)
            {
                case "Windows":
                    if (edit)
                    {
                        _theme.ThemeColors[(ThemeColor) val] = _colors.SelectedColor;
                    }
                    else
                    {
                        _colors.SelectedColor = _theme.ThemeColors[(ThemeColor) val];
                    }
                    break;

                case "Messages":
                    message = true;
                    if (edit)
                    {
                        _theme.Messages[(ThemeMessage) val].DefaultColor = _colors.SelectedColor;
                    }
                    else
                    {
                        _colors.SelectedColor = _theme.Messages[(ThemeMessage) val].DefaultColor;
                    }
                    break;
            }
            Preview.Show(_preview, _theme, message ? (ThemeMessage) val : ThemeMessage.ChannelJoinText);
        }
    }
}