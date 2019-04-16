/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FusionIRC.Forms.Child;
using FusionIRC.Helpers;
using ircClient;
using ircCore.Controls;
using ircCore.Settings.Theming;
using libolv;

namespace FusionIRC.Forms.Misc
{
    public sealed class FrmPart : FormEx
    {
        private readonly Panel _pnlIcon;
        private readonly Label _lblInfo;
        private readonly ObjectListView _chanList;
        private readonly ImageList _imageList;
        private readonly OlvColumn _colChans;
        private readonly Button _btnPart;
        private readonly Button _btnClose;

        public string Channels
        {
            get
            {
                if (_chanList.CheckedObjects.Count == _chanList.GetItemCount())
                {
                    return "0";
                }
                var sb = new StringBuilder();
                foreach (var channel in _chanList.CheckedObjects)
                {
                    sb.Append(string.Format(sb.Length == 0 ? "{0}" : ",{0}", ((FrmChildWindow) channel).Tag));
                }
                return sb.ToString();
            }
        }

        public FrmPart(ClientConnection client)
        {
            ClientSize = new Size(439, 232);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = @"Part Channel(s)";

            _pnlIcon = new Panel
                           {
                               BackColor = Color.Transparent,
                               BackgroundImageLayout = ImageLayout.Center,
                               Location = new Point(12, 12),
                               Size = new Size(64, 64),
                               BackgroundImage = Properties.Resources.partChan.ToBitmap()
                           };

            _lblInfo = new Label
                           {
                               AutoSize = true,
                               BackColor = Color.Transparent,
                               Location = new Point(82, 12),
                               Size = new Size(207, 15),
                               Text = @"Select the channel(s) you wish to part"
                           };

            _chanList = new ObjectListView
                            {
                                CheckBoxes = true,
                                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                                Location = new Point(85, 30),
                                Size = new Size(342, 149),
                                TabIndex = 0,
                                UseCompatibleStateImageBehavior = false,
                                MultiSelect = false,
                                Sorting = SortOrder.Ascending,
                                View = View.Details
                            };

            _imageList = new ImageList {ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16)};
            _imageList.Images.Add(Properties.Resources.channel);
            _chanList.SmallImageList = _imageList;

            /* The column gets the Tag property of each channel to get it's "name" */
            _colChans = new OlvColumn("Channel(s) currently joined:", "Tag")
                            {
                                CellPadding = null,
                                IsEditable = false,
                                Sortable = false,
                                IsVisible = true,                                
                                Width = 340,
                                ImageGetter = delegate { return 0; }
                            };            

            _btnPart = new Button
                           {
                               DialogResult = DialogResult.OK,
                               Location = new Point(271, 197),
                               Size = new Size(75, 23),
                               TabIndex = 1,
                               Text = @"Part",
                               UseVisualStyleBackColor = true
                           };

            _btnClose = new Button
                            {
                                DialogResult = DialogResult.Cancel,
                                Location = new Point(352, 197),
                                Size = new Size(75, 23),
                                TabIndex = 2,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };

            Controls.AddRange(new Control[] {_pnlIcon, _lblInfo, _chanList, _btnPart, _btnClose});

            AcceptButton = _btnPart;

            /* Get channel window names and add them to the list */
            _chanList.AddObjects(WindowManager.Windows[client].Where(c => c.WindowType == ChildWindowType.Channel).ToList());

            _chanList.AllColumns.AddRange(new[] {_colChans});
            _chanList.Columns.AddRange(new ColumnHeader[] {_colChans});            
            _chanList.RebuildColumns();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if (_chanList.SelectedObjects == null)
                {
                    e.Cancel = true;
                    return;
                }                
            }
            base.OnFormClosing(e);
        }
    }
}
