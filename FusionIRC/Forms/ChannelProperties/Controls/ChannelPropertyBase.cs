/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FusionIRC.Forms.ChannelProperties.Editing;
using FusionIRC.Helpers;
using ircClient;
using ircCore.Utils;
using ircScript.Classes.ScriptFunctions;
using libolv;

namespace FusionIRC.Forms.ChannelProperties.Controls
{
    public sealed class ChannelPropertyBase : UserControl
    {
        /* In this control, we need to be sure to check (set a flag) that the user CAN actually change
         * channel modes */
        private readonly ObjectListView _lvList;        
        private readonly OlvColumn _colMask;
        private readonly OlvColumn _colSetBy;
        private readonly OlvColumn _colDate;

        private readonly Button _btnAdd;
        private readonly Button _btnEdit;
        private readonly Button _btnDelete;

        private readonly ClientConnection _client;
        private readonly string _chan;
        private readonly List<ChannelPropertyData> _list;
        private readonly char _mode;
        
        public ChannelPropertyBase(ClientConnection client, string channel, bool isOperator, List<ChannelPropertyData> list, char modeChar)
        {
            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Size = new Size(344, 159);

            _lvList = new ObjectListView
                          {
                              FullRowSelect = true,
                              HeaderStyle = ColumnHeaderStyle.Nonclickable,
                              HideSelection = false,
                              Location = new Point(3, 3),
                              Size = new Size(257, 153),
                              TabIndex = 0,
                              UseCompatibleStateImageBehavior = false,
                              View = View.Details
                          };

            _colMask = new OlvColumn("Hostmask:", "Address")
                           {
                               CellPadding = null,
                               IsEditable = false,
                               Sortable = false,
                               Width = 120,
                           };

            _colSetBy = new OlvColumn("Set by:", "SetByNick")
                            {
                                CellPadding = null,
                                IsEditable = false,
                                Sortable = false,
                                Width = 80,
                            };

            _colDate = new OlvColumn("Date:", "Date")
                           {
                               CellPadding = null,
                               IsEditable = false,
                               Sortable = false,
                               Width = 160,
                           };

            _lvList.Columns.AddRange(new ColumnHeader[] {_colMask, _colSetBy, _colDate});
            _lvList.AllColumns.AddRange(new[] {_colMask, _colSetBy, _colDate});

            _btnAdd = new Button
                          {
                              Location = new Point(266, 3),
                              Size = new Size(75, 23),
                              TabIndex = 1,
                              Text = @"Add",
                              UseVisualStyleBackColor = true,
                              Enabled = isOperator
                          };

            _btnEdit = new Button
                           {
                               Enabled = false,
                               Location = new Point(266, 104),
                               Size = new Size(75, 23),
                               TabIndex = 2,
                               Text = @"Edit",
                               UseVisualStyleBackColor = true
                           };

            _btnDelete = new Button
                             {
                                 Enabled = false,
                                 Location = new Point(266, 133),
                                 Size = new Size(75, 23),
                                 TabIndex = 3,
                                 Text = @"Delete",
                                 UseVisualStyleBackColor = true
                             };

            Controls.AddRange(new Control[] {_lvList, _btnAdd, _btnEdit, _btnDelete});

            _client = client;
            _chan = channel;
            _list = list;
            _mode = modeChar;
            
            _lvList.SelectedIndexChanged += SelectedIndexChanged;

            _btnAdd.Click += ButtonClickHandler;
            _btnEdit.Click += ButtonClickHandler;
            _btnDelete.Click += ButtonClickHandler;
        }

        protected override void OnLoad(EventArgs e)
        {
            _lvList.SetObjects(_list);
            base.OnLoad(e);
        }

        /* Callbacks */
        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            var b = _lvList.SelectedObjects != null;
            _btnEdit.Enabled = b;
            _btnDelete.Enabled = b;
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
                case "ADD":
                    Add();
                    break;

                case "EDIT":
                    Edit();
                    break;

                case "DELETE":
                    Delete();
                    break;
            }
        }

        /* Private helper methods */
        private void Add()
        {
            /* Add a new host mask to list - keep in mind to check it doesn't already exist */
            string mask;
            using (var d = new FrmEditChannelList(DialogEditType.Add) {Prompt = "Enter a host mask to add:"})
            {
                if (d.ShowDialog(this) == DialogResult.Cancel)
                {
                    return;
                }
                mask = Address.CheckIrcAddress(d.Mask);
            }
            if (GetChannelUserData(mask) != null)
            {
                return;
            }
            var data = new ChannelPropertyData
                           {
                               Address = mask,
                               SetByNick = _client.UserInfo.Nick,
                               Date = TimeFunctions.FormatAsciiTime(TimeFunctions.CTime(), "ddd dd/MM/yyyy h:nnt")
                           };
            _list.Add(data);
            _lvList.SetObjects(_list);
            /* We also need to send this mask to server */
            _client.Send(string.Format("MODE {0} +{1} {2}", _chan, _mode, mask));
        }

        private void Edit()
        {
            var data = (ChannelPropertyData) _lvList.SelectedObjects[0];
            if (data == null)
            {
                return;
            }
            string originalAddress = data.Address;
            string mask;
            using (var d = new FrmEditChannelList(DialogEditType.Add) {Prompt = "Change the host mask on current list:", Mask = originalAddress})
            {
                if (d.ShowDialog(this) == DialogResult.Cancel || d.Mask.Equals(originalAddress))
                {
                    /* Cancel or NO change */
                    return;
                }
                mask = d.Mask;
            }
            data.Address = mask;
            data.SetByNick = _client.UserInfo.Nick;
            data.Date = TimeFunctions.FormatAsciiTime(TimeFunctions.CTime(), "ddd dd/MM/yyyy h:nnt");
            _lvList.SetObjects(_list);
            /* Now we update address on server */
            _client.Send(string.Format("MODE {0} -{1}+{1} {2} {3}", _chan, _mode, originalAddress, mask));
        }

        private void Delete()
        {
            /* Delete each selected mask and also send mode <chan> -b to channel */
            var s = new StringBuilder();
            var count = _lvList.SelectedObjects.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var data = (ChannelPropertyData) _lvList.SelectedObjects[i];
                if (data == null)
                {
                    continue; /* It shouldn't be... */
                }
                s.Append(string.Format("{0} ", data.Address));
                _list.Remove(data);
            }
            _lvList.SetObjects(_list);
            /* Send string to server */
            _client.Send(string.Format("MODE {0} -{1} {2}", _chan, new String(_mode, count), s));
        }

        private ChannelPropertyData GetChannelUserData(string mask)
        {
            return _list.FirstOrDefault(m => m.Address.Equals(mask, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
