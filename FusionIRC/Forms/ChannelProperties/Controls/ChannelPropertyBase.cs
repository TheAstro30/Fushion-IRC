/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FusionIRC.Helpers;
using libolv;

namespace FusionIRC.Forms.ChannelProperties.Controls
{
    public partial class ChannelPropertyBase : UserControl
    {
        private readonly OlvColumn _colMask;
        private readonly OlvColumn _colSetBy;
        private readonly OlvColumn _colDate;

        private readonly ChannelPropertyType _type;
        private readonly List<ChannelPropertyData> _list;

        public ChannelPropertyBase(ChannelPropertyType type, List<ChannelPropertyData> list)
        {
            InitializeComponent();

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

            lvList.Columns.AddRange(new ColumnHeader[] {_colMask, _colSetBy, _colDate});
            lvList.AllColumns.AddRange(new[] {_colMask, _colSetBy, _colDate});

            _type = type;
            _list = list;

            btnAdd.Click += ButtonClickHandler;
            btnEdit.Click += ButtonClickHandler;
            btnDelete.Click += ButtonClickHandler;
        }

        protected override void OnLoad(EventArgs e)
        {
            lvList.SetObjects(_list);
            base.OnLoad(e);
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
            
        }

        private void Edit()
        {
            
        }

        private void Delete()
        {
            
        }
    }
}
