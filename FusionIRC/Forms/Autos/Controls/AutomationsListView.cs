/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Forms.Autos.Editing;
using FusionIRC.Forms.Misc;
using ircCore.Autos;
using ircCore.Utils;
using libolv;

namespace FusionIRC.Forms.Autos.Controls
{
    public sealed class AutomationsListView : UserControl
    {
        private readonly CheckBox _chkEnable;
        private readonly Label _lblNetwork;
        private readonly ComboBox _cmbNetwork;
        private readonly Button _btnNew;
        private readonly Button _btnRemove;

        private readonly ObjectListView _olvData;
        private readonly OlvColumn _colItem;
        private readonly OlvColumn _colValue;

        private readonly Button _btnAdd;
        private readonly Button _btnClear;
        private readonly Button _btnDelete;
        private readonly Button _btnEdit;

        private readonly ImageList _imageList;

        private readonly AutomationsManager.AutomationType _autoType;

        public AutomationsListView(AutomationsManager.AutomationType autoType)
        {
            _autoType = autoType;
            /* Controls */
            _chkEnable = new CheckBox
                             {
                                 AutoSize = true,
                                 Location = new Point(3, 3),
                                 Size = new Size(61, 19),
                                 TabIndex = 0,
                                 Text = @"Enable",
                                 UseVisualStyleBackColor = true
                             };

            /* Network */
            _lblNetwork = new Label
                              {
                                  AutoSize = true,
                                  Location = new Point(3, 31),
                                  Size = new Size(55, 15),
                                  Text = @"Network:"
                              };

            _cmbNetwork = new ComboBox
                              {
                                  DropDownStyle = ComboBoxStyle.DropDownList,
                                  FormattingEnabled = true,
                                  Location = new Point(64, 28),
                                  Size = new Size(152, 23),
                                  TabIndex = 1
                              };

            _btnNew = new Button
                          {
                              Location = new Point(222, 28),
                              Size = new Size(75, 23),
                              TabIndex = 2,
                              Text = @"New",
                              UseVisualStyleBackColor = true
                          };

            _btnRemove = new Button
                             {
                                 Location = new Point(303, 28),
                                 Size = new Size(75, 23),
                                 TabIndex = 3,
                                 Text = @"Remove",
                                 UseVisualStyleBackColor = true,
                                 Enabled = false
                             };

            /* Listview */
            _olvData = new ObjectListView
                           {
                               FullRowSelect = true,
                               HeaderStyle = ColumnHeaderStyle.Nonclickable,
                               HideSelection = false,
                               Location = new Point(3, 65),
                               MultiSelect = false,
                               Size = new Size(294, 267),
                               TabIndex = 4,
                               UseCompatibleStateImageBehavior = false,
                               View = View.Details
                           };

            switch (_autoType)
            {
                case AutomationsManager.AutomationType.Join:
                    _chkEnable.Text = @"Enable auto-join";
                    _chkEnable.Checked = AutomationsManager.Automations.Join.Enable;
                    _colItem = new OlvColumn("Channel:", "Item")
                                   {
                                       CellPadding = null,
                                       IsEditable = false,
                                       Sortable = false,
                                       Width = 120,
                                       ImageGetter = delegate { return 0; }
                                   };          
                    break;

                case AutomationsManager.AutomationType.Identify:
                    _chkEnable.Text = @"Enable auto-identify";
                    _chkEnable.Checked = AutomationsManager.Automations.Identify.Enable;
                    _colItem = new OlvColumn("Nick:", "Item")
                                   {
                                       CellPadding = null,
                                       IsEditable = false,
                                       Sortable = false,
                                       Width = 120,
                                       ImageGetter = delegate { return 1; }
                                   };
                    break;
            }

            _colValue = new OlvColumn("Password:", "Value")
                            {
                                CellPadding = null,
                                IsEditable = false,
                                Sortable = false,
                                Width = 300
                            };

            _imageList = new ImageList
                             {
                                 ColorDepth = ColorDepth.Depth32Bit,
                                 ImageSize = new Size(16, 16)
                             };
            _imageList.Images.AddRange(new[]
                                           {
                                               Properties.Resources.autoJoin.ToBitmap(),
                                               Properties.Resources.identify.ToBitmap()
                                           });

            _olvData.SmallImageList = _imageList;

            _olvData.AllColumns.AddRange(new[] {_colItem, _colValue});
            _olvData.Columns.AddRange(new ColumnHeader[] { _colItem, _colValue });
            _olvData.RebuildColumns();

            /* Editing */
            _btnAdd = new Button
                          {
                              Location = new Point(303, 65),
                              Size = new Size(75, 23),
                              TabIndex = 5,
                              Text = @"Add",
                              UseVisualStyleBackColor = true,
                              Enabled = false
                          };

            _btnEdit = new Button
                           {
                               Location = new Point(303, 94),
                               Size = new Size(75, 23),
                               TabIndex = 6,
                               Text = @"Edit",
                               UseVisualStyleBackColor = true,
                               Enabled = false
                           };

            _btnDelete = new Button
                             {
                                 Location = new Point(303, 158),
                                 Size = new Size(75, 23),
                                 TabIndex = 7,
                                 Text = @"Delete",
                                 UseVisualStyleBackColor = true,
                                 Enabled = false
                             };

            _btnClear = new Button
                            {
                                Location = new Point(303, 187),
                                Size = new Size(75, 23),
                                TabIndex = 8,
                                Text = @"Clear",
                                UseVisualStyleBackColor = true,
                                Enabled = false
                            };

            Controls.AddRange(new Control[]
                                  {
                                      _chkEnable, _lblNetwork, _cmbNetwork, _btnNew, _btnRemove, _olvData, _btnAdd, _btnEdit,
                                      _btnDelete,
                                      _btnClear
                                  });

            BackColor = Color.Transparent;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            Size = new Size(381, 335);

            /* Handlers */
            _chkEnable.CheckedChanged += CheckBoxCheckChanged;
            _cmbNetwork.SelectedIndexChanged += ComboSelectedIndexChanged;
            _olvData.SelectionChanged += ListSelectionChanged;

            _btnNew.Click += ButtonClickHandler;
            _btnRemove.Click += ButtonClickHandler;
            _btnAdd.Click += ButtonClickHandler;
            _btnEdit.Click += ButtonClickHandler;
            _btnDelete.Click += ButtonClickHandler;
            _btnClear.Click += ButtonClickHandler;

            /* Fill combo with stored networks and update listview */
            _cmbNetwork.Items.AddRange(AutomationsManager.GetAllNetworks(_autoType));
            if (_cmbNetwork.Items.Count > 0)
            {
                _cmbNetwork.SelectedIndex = 0;                
            }
        }

        /* Callback handlers */
        private void CheckBoxCheckChanged(object sender, EventArgs e)
        {
            switch (_autoType)
            {
                case AutomationsManager.AutomationType.Join:
                    AutomationsManager.Automations.Join.Enable = _chkEnable.Checked;
                    break;

                case AutomationsManager.AutomationType.Identify:
                    AutomationsManager.Automations.Identify.Enable = _chkEnable.Checked;
                    break;
            }
        }

        private void ComboSelectedIndexChanged(object sender, EventArgs e)
        {
            BuildList();
        }

        private void ListSelectionChanged(object sender, EventArgs e)
        {
            var enable = _olvData.SelectedObject != null;
            _btnEdit.Enabled = enable;
            _btnDelete.Enabled = enable;
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
                case "NEW":
                    AddNetwork();
                    break;

                case "REMOVE":
                    RemoveNetwork();
                    break;

                case "ADD":
                    AddItem();
                    break;

                case "EDIT":
                    EditItem();
                    break;

                case "DELETE":
                    RemoveItem();
                    break;

                case "CLEAR":
                    /* Clear stored list + displayed list */
                    var nd = AutomationsManager.GetAutomationByNetwork(_autoType, _cmbNetwork.Text);
                    if (nd == null)
                    {
                        /* Should NOT be null... */
                        return;
                    }
                    nd.Data.Clear();
                    BuildList();
                    break;
            }
        }

        /* Private methods */
        private void BuildList()
        {
            _olvData.ClearObjects();
            if (_cmbNetwork.Items.Count == 0)
            {
                _btnRemove.Enabled = false;
                _btnAdd.Enabled = false;
                _btnClear.Enabled = false;
                return;
            }
            var nd = AutomationsManager.GetAutomationByNetwork(_autoType, _cmbNetwork.Text);
            _olvData.AddObjects(nd.Data);
            _btnRemove.Enabled = true;
            _btnAdd.Enabled = true;
            _btnClear.Enabled = nd.Data.Count > 0;
        }

        /* Add/remove network */
        private void AddNetwork()
        {
            using (var network = new FrmNetwork())
            {
                if (network.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                var nd = new AutoList.AutoNetworkData {Name = network.Network};
                if (AutomationsManager.GetAutomationByNetwork(_autoType, nd.Name) == null)
                {
                    /* If the network doesn't already exist, we add a new one */
                    switch (_autoType)
                    {
                        case AutomationsManager.AutomationType.Join:

                            AutomationsManager.Automations.Join.Network.Add(nd);
                            break;

                        case AutomationsManager.AutomationType.Identify:
                            AutomationsManager.Automations.Identify.Network.Add(nd);
                            break;
                    }
                    _cmbNetwork.Items.Add(nd.Name);
                    _cmbNetwork.SelectedIndex = _cmbNetwork.Items.Count - 1;
                }
                else
                {
                    /* No point adding it if it already exists, just select it in the combo box */
                    var index = _cmbNetwork.Items.Cast<object>().TakeWhile(n => !n.ToString().Equals(nd.Name, StringComparison.InvariantCultureIgnoreCase)).Count();
                    _cmbNetwork.SelectedIndex = index;
                }                
            }            
        }

        private void RemoveNetwork()
        {
            var nd = AutomationsManager.GetAutomationByNetwork(_autoType, _cmbNetwork.Text);
            if (nd == null)
            {
                /* It shouldn't be null... */
                return;
            }
            /* Remove network from stored list */
            switch (_autoType)
            {
                case AutomationsManager.AutomationType.Join:
                    AutomationsManager.Automations.Join.Network.Remove(nd);
                    break;

                case AutomationsManager.AutomationType.Identify:
                    AutomationsManager.Automations.Identify.Network.Remove(nd);
                    break;
            }
            /* Remove it from combobox */
            var index = _cmbNetwork.SelectedIndex - 1;
            if (index < 0)
            {
                index = 0;
            }
            _cmbNetwork.Items.RemoveAt(_cmbNetwork.SelectedIndex);
            if (_cmbNetwork.Items.Count > 0)
            {
                _cmbNetwork.SelectedIndex = index;
            }
            else
            {
                /* Combo list is empty... */             
                BuildList();
            }   
        }

        /* Add/edit/remove items */
        private void AddItem()
        {
            var nd = AutomationsManager.GetAutomationByNetwork(_autoType, _cmbNetwork.Text);
            if (nd == null)
            {
                /* It shouldn't be null... */
                return;
            }
            using (var edit = new FrmAutoEdit(DialogEditType.Add))
            {
                switch (_autoType)
                {
                    case AutomationsManager.AutomationType.Join:
                        edit.Text = @"Add channel to auto-join";
                        edit.ItemLabelText = @"Channel name:";
                        edit.ValueLabelText = @"Optional password:";
                        break;

                    case AutomationsManager.AutomationType.Identify:
                        edit.Text = @"Add nick name to auto-identify";
                        edit.ItemLabelText = @"Nick name:";
                        edit.ValueLabelText = @"Password:";
                        break;
                }
                if (edit.ShowDialog(this) == DialogResult.Cancel || string.IsNullOrEmpty(edit.Item))
                {
                    return;
                }
                /* We should now be able to add this data to "nd" */
                var data = new AutoList.AutoData
                               {
                                   Item = Functions.GetFirstWord(edit.Item),
                                   Value = Functions.GetFirstWord(edit.Value)
                               };
                nd.Data.Add(data);
                _olvData.AddObject(data);
                _btnClear.Enabled = true;
            }            
        }

        private void EditItem()
        {
            var data = _olvData.SelectedObject;
            var nd = AutomationsManager.GetAutomationByNetwork(_autoType, _cmbNetwork.Text);
            if (data == null || nd == null)
            {
                /* Shouldn't be null...*/
                return;
            }
            using (var edit = new FrmAutoEdit(DialogEditType.Edit))
            {
                switch (_autoType)
                {
                    case AutomationsManager.AutomationType.Join:
                        edit.Text = @"Edit channel to auto-join";
                        edit.ItemLabelText = @"Channel name:";
                        edit.ValueLabelText = @"Optional password:";
                        break;

                    case AutomationsManager.AutomationType.Identify:
                        edit.Text = @"Edit nick name to auto-identify";
                        edit.ItemLabelText = @"Nick name:";
                        edit.ValueLabelText = @"Password:";
                        break;
                }
                var d = (AutoList.AutoData) data;
                edit.Item = d.Item;
                edit.Value = d.Value;
                if (edit.ShowDialog(this) == DialogResult.Cancel || string.IsNullOrEmpty(edit.Item))
                {
                    return;
                }
                /* Update item */
                d.Item = Functions.GetFirstWord(edit.Item);
                d.Value = Functions.GetFirstWord(edit.Value);
                /* Update display */
                _olvData.RefreshObject(data);
                _btnEdit.Enabled = false;
                _btnDelete.Enabled = false;
            } 
        }

        private void RemoveItem()
        {
            var data = _olvData.SelectedObject;
            var nd = AutomationsManager.GetAutomationByNetwork(_autoType, _cmbNetwork.Text);
            if (data == null || nd == null)
            {
                /* Shouldn't be null...*/
                return;
            }
            nd.Data.Remove((AutoList.AutoData)data);
            _olvData.RemoveObject(data);
            _btnEdit.Enabled = false;
            _btnDelete.Enabled = false;
            _btnClear.Enabled = nd.Data.Count > 0;
        }
    }
}