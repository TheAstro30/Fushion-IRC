/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Properties;
using ircCore.Forms;
using ircCore.Settings;
using ircCore.Utils;
using ircScript;
using ircScript.Classes;
using ircScript.Classes.Structures;
using ircScript.Controls;
using libolv;

namespace FusionIRC.Forms.Misc
{
    public sealed class FrmScript : Form
    {
        private readonly MenuStrip _menu;
        private readonly ToolStripMenuItem _mnuFile;
        private readonly ToolStripMenuItem _mnuEdit;
        private readonly ToolStripMenuItem _mnuView;
        private readonly TableLayoutPanel _tbButtons;
        private readonly TableLayoutPanel _tbLayout;
        private readonly TreeListView _tvFiles;
        private readonly OlvColumn _colFiles;
        private readonly ScriptEditor _txtEdit;
        private readonly Button _btnClose;

        private readonly List<ScriptData> _aliases = new List<ScriptData>();
        private readonly List<ScriptData> _popups = new List<ScriptData>();
        
        private readonly List<ScriptFileNode> _files = new List<ScriptFileNode>();
        private readonly bool _initialize;
        private readonly ScriptFileNode _varNode;
        private readonly ScriptData _variables;                
        private ScriptData _currentEditingScript;

        private bool _fileChanged;

        public FrmScript()
        {
            _initialize = true;
            /* Set window position and size */
            var w = SettingsManager.GetWindowByName("editor");
            Size = w.Size;
            Location = w.Position;
            WindowState = w.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            Icon = Resources.aliasEditor;            
            MinimumSize = new Size(512, 395);
            StartPosition = FormStartPosition.Manual;
            Text = @"FusionIRC - Script Editor";

            _menu = new MenuStrip
                       {
                           Location = new Point(0, 0),
                           RenderMode = ToolStripRenderMode.Professional,
                           Size = new Size(496, 24),
                           TabIndex = 3
                       };
            /* File menu */
            _mnuFile = new ToolStripMenuItem {Size = new Size(37, 20), Text = @"&File"};            
            _mnuFile.DropDownItems.AddRange(new ToolStripItem[]
                                               {
                                                   new ToolStripMenuItem("New...", null, MenuItemOnClick, Keys.None),
                                                   new ToolStripSeparator(),
                                                   new ToolStripMenuItem("Save", null, MenuItemOnClick,
                                                                         Keys.Control | Keys.S),
                                                   new ToolStripMenuItem("Save All", null, MenuItemOnClick,
                                                                         Keys.Control | Keys.Shift | Keys.S),
                                                   new ToolStripSeparator(),
                                                   new ToolStripMenuItem("Close", null, MenuItemOnClick,
                                                                         Keys.Alt | Keys.F4)
                                               });
            /* Edit menu */
            _mnuEdit = new ToolStripMenuItem { Size = new Size(39, 20), Text = @"&Edit" };
            _mnuEdit.DropDownItems.AddRange(new ToolStripItem[]
                                               {
                                                   new ToolStripMenuItem("Undo", null, MenuItemOnClick,
                                                                         Keys.Control | Keys.Z),
                                                   new ToolStripMenuItem("Redo", null, MenuItemOnClick,
                                                                         Keys.Control | Keys.Y),
                                                   new ToolStripSeparator(),
                                                   new ToolStripMenuItem("Cut", null, MenuItemOnClick,
                                                                         Keys.Control | Keys.X),
                                                   new ToolStripMenuItem("Copy", null, MenuItemOnClick,
                                                                         Keys.Control | Keys.C),
                                                   new ToolStripMenuItem("Paste", null, MenuItemOnClick,
                                                                         Keys.Control | Keys.V),
                                                   new ToolStripMenuItem("Delete", null, MenuItemOnClick, Keys.None)
                                               });
            /* View menu */
            _mnuView = new ToolStripMenuItem { Size = new Size(39, 20), Text = @"&View" };
            _mnuView.DropDownItems.AddRange(new ToolStripItem[]
                                                {
                                                    new ToolStripMenuItem("Font", null, MenuItemOnClick,
                                                                          Keys.None)
                                                });

            _menu.Items.AddRange(new ToolStripItem[]
                                    {
                                        _mnuFile,
                                        _mnuEdit,
                                        _mnuView
                                    });

            _tvFiles = new TreeListView
                          {
                              BorderStyle = BorderStyle.FixedSingle,
                              Dock = DockStyle.Fill,
                              FullRowSelect = true,
                              HideSelection = false,
                              HeaderStyle = ColumnHeaderStyle.Nonclickable,
                              Location = new Point(3, 3),
                              OwnerDraw = true,
                              ShowGroups = false,
                              Size = new Size(134, 290),
                              TabIndex = 1,
                              UseCompatibleStateImageBehavior = false,
                              View = View.Details,
                              VirtualMode = true
                          };

            /* Build treelist */
            _colFiles = new OlvColumn(@"Script Files:", "Name")
                            {
                                CellPadding = null,
                                IsEditable = false,
                                Sortable = false,
                                Width = 160,
                                FillsFreeSpace = true
                            };

            _tvFiles.AllColumns.Add(_colFiles);
            _tvFiles.Columns.Add(_colFiles);
            _tvFiles.TreeColumnRenderer.LinePen = new Pen(Color.FromArgb(190, 190, 190), 0.5F)
                                                      {
                                                          DashStyle = DashStyle.Dot
                                                      };
            /* Root item (network name) */
            _tvFiles.CanExpandGetter = x => x is ScriptFileNode;

            /* Children of each root item (server data) */
            _tvFiles.ChildrenGetter = delegate(object x)
                                          {
                                              var sd = (ScriptFileNode) x;
                                              return sd.Data;
                                          };

            _colFiles.ImageGetter = x => x is ScriptFileNode
                                             ? Resources.codeHeader.ToBitmap()
                                             : Resources.codeFile.ToBitmap();
            
            _txtEdit = new ScriptEditor
                          {
                              BackColor = SystemColors.Window,
                              BorderStyle = BorderStyle.FixedSingle,
                              Dock = DockStyle.Fill,
                              Font = SettingsManager.Settings.Editor.Font,
                              Location = new Point(143, 3),
                              Size = new Size(350, 290),
                              TabIndex = 0
                          };


            _btnClose = new Button
                            {
                                Location = new Point(119, 3),
                                Size = new Size(75, 23),
                                TabIndex = 2,
                                Text = @"Close",
                                UseVisualStyleBackColor = true
                            };
            /* Button layout table */
            _tbButtons = new TableLayoutPanel
                             {
                                 ColumnCount = 2,
                                 Dock = DockStyle.Right,
                                 Location = new Point(293, 299),
                                 RowCount = 1
                             };

            _tbButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tbButtons.Size = new Size(200, 30);

            _tbButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58.24742F));
            _tbButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 41.75258F));
            _tbButtons.Controls.Add(_btnClose, 1, 0);
            /* Main layout table */
            _tbLayout = new TableLayoutPanel
                           {
                               ColumnCount = 2,
                               Dock = DockStyle.Fill,
                               Location = new Point(0, 24),
                               RowCount = 2,
                               Size = new Size(496, 332)
                           };
            
            _tbLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            _tbLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 356F));

            _tbLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tbLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

            _tbLayout.Controls.Add(_txtEdit, 1, 0);
            _tbLayout.Controls.Add(_tvFiles, 0, 0);
            _tbLayout.Controls.Add(_tbButtons, 1, 1);

            Controls.AddRange(new Control[] {_tbLayout, _menu});
            MainMenuStrip = _menu;            
            /* Copy scripts to temporary arrays */
            _aliases = ScriptManager.AliasData.Clone();
            _popups = ScriptManager.PopupData.Clone();
            /* Here we can cheat with displaying variables by creating them as new script file */
            _variables = new ScriptData
                             {
                                 Name = "%variables",
                                 RawScriptData = ScriptManager.Variables.Variable.Select(data => data.ToString()).ToList()
                             };
            _varNode = new ScriptFileNode
                           {
                               Name = "Variables",
                               Type = ScriptFileNodeType.Variables
                           };
            _varNode.Data.Add(_variables);
            _files.AddRange(new[]
                                {
                                    new ScriptFileNode {Name = "Aliases", Data = _aliases, Type = ScriptFileNodeType.Aliases},
                                    new ScriptFileNode {Name = "Popups", Data = _popups, Type = ScriptFileNodeType.Popups},
                                    _varNode
                                });

            if (_aliases.Count == 0)
            {
                /* Create a new alias */
                var script = new ScriptData {Name = "aliases01", ContentsChanged = true};
                _aliases.Add(script);
                var file = new ScriptFileNode {Name = script.Name, Type = ScriptFileNodeType.Aliases};
                _files.Add(file);                
            }
            _tvFiles.AddObjects(_files);
            _currentEditingScript = _aliases[0];            
            _tvFiles.ExpandAll();
            _tvFiles.SelectObject(_currentEditingScript);
            /* Set current editing script file */
            SwitchFile(_currentEditingScript);
            /* Menubar */
            _menu.Items.Add(new ToolStripButton("{ }", null, MenuButtonClick)
                               {
                                   Alignment = ToolStripItemAlignment.Right
                               });
            
            _mnuEdit.DropDownOpening += MenuDropDownOpening;
            _tvFiles.SelectedIndexChanged += FilesSelectedIndexChanged;
            _txtEdit.EditBox.TextChanged += TextEditTextChanged;

            _btnClose.Click += ButtonClickHandler;

            _initialize = false;
        }

        /* Overrides */
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            /* This will change as stuff progresses, currently we only have an "OK" button */
            /* Check if any files need saving */
            if (_files.SelectMany(file => file.Data).Any(s => s.ContentsChanged))
            {
                var r = MessageBox.Show(@"Some files have changed. Do you wish to save changes?",
                                        @"Save Changes", MessageBoxButtons.YesNoCancel);
                if (r == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (r == DialogResult.Yes)
                {
                    SaveAll();
                }
            }
            base.OnFormClosing(e);
        }

        protected override void OnMove(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName("editor");
                if (WindowState == FormWindowState.Normal)
                {
                    w.Position = Location;
                }
            }
            base.OnMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName("editor");
                if (WindowState == FormWindowState.Normal)
                {
                    w.Size = Size;
                    w.Position = Location;
                }
                w.Maximized = WindowState == FormWindowState.Maximized;
            }
            base.OnResize(e);
        }

        /* Handler callbacks */
        private void FilesSelectedIndexChanged(object sender, EventArgs e)
        {
            var node = _tvFiles.SelectedObject;
            if (node == null || node.GetType() == typeof (ScriptFileNode))
            {
                return;
            }
            var s = (ScriptData) node;
            SwitchFile(s);
        }

        private void TextEditTextChanged(object sender, EventArgs e)
        {
            if (_fileChanged)
            {
                return;
            }
            _currentEditingScript.ContentsChanged = true;
        }

        private void MenuDropDownOpening(object sender, EventArgs e)
        {
            var dd = (ToolStripMenuItem) sender;
            if (dd == null)
            {
                return;
            }
            switch (dd.Text.ToUpper())
            {
                case "&EDIT":
                    /* Check ability to undo/redo */
                    _mnuEdit.DropDownItems[0].Enabled = _txtEdit.EditBox.CanUndo;
                    _mnuEdit.DropDownItems[1].Enabled = _txtEdit.EditBox.CanRedo;
                    _mnuEdit.DropDownItems[3].Enabled = _txtEdit.EditBox.CanCopy;
                    _mnuEdit.DropDownItems[4].Enabled = _txtEdit.EditBox.CanCopy;
                    _mnuEdit.DropDownItems[5].Enabled = Clipboard.ContainsText();
                    _mnuEdit.DropDownItems[6].Enabled = _txtEdit.EditBox.CanCopy;
                    break;
            }
        }

        private void MenuButtonClick(object sender, EventArgs e)
        {
            var btn = (ToolStripButton) sender;
            if (btn == null)
            {
                return;
            }
            /* Reformat text */
            _txtEdit.Indent();
        }

        private void MenuItemOnClick(object sender, EventArgs e)
        {
            var di = (ToolStripMenuItem) sender;
            if (di == null)
            {
                return;
            }
            switch (di.Text.ToUpper())
            {
                case "NEW...":
                    New();
                    break;

                case "SAVE":
                    Save();
                    break;

                case "SAVE ALL":
                    SaveAll();
                    break;

                case "CLOSE":
                    Close();
                    break;

                case "UNDO":
                    _txtEdit.EditBox.Undo();
                    break;

                case "REDO":
                    _txtEdit.EditBox.Redo();
                    break;

                case "CUT":
                    _txtEdit.EditBox.Cut();
                    break;

                case "COPY":
                    _txtEdit.EditBox.Copy();
                    break;

                case "PASTE":
                    _txtEdit.EditBox.Paste();
                    break;

                case "DELETE":
                    /* Copy contents of clipboard */
                    var clipText = Clipboard.GetText();
                    /* Remove selected text */
                    _txtEdit.EditBox.Cut();
                    /* Reset clipboard contents */
                    if (!string.IsNullOrEmpty(clipText))
                    {
                        Clipboard.SetText(clipText);
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                    break;

                case "FONT":
                    using (var f = new FrmFont { SelectedFont = SettingsManager.Settings.Editor.Font, ShowDefaultCheckbox = false })
                    {
                        if (f.ShowDialog(this) == DialogResult.OK)
                        {
                            var font = f.SelectedFont;
                            SettingsManager.Settings.Editor.Font = font;
                            _txtEdit.Font = font;
                            _txtEdit.EditBox.Lines = _currentEditingScript.RawScriptData.ToArray();
                        }
                    }
                    break;
            }
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
                case "CLOSE":
                    Close();
                    break;
            }
        }

        /* Private methods */
        private void SwitchFile(ScriptData file)
        {
            if (_currentEditingScript != null && _currentEditingScript.ContentsChanged)
            {
                /* Make sure to dump contents of edit window text */
                _currentEditingScript.RawScriptData = new List<string>(_txtEdit.EditBox.Lines);
            }
            if (file == null)
            {
                return;
            }
            _fileChanged = true;
            _txtEdit.EditBox.Lines = file.RawScriptData.Select(data => data.ToString()).ToArray();
            _currentEditingScript = file;
            _txtEdit.Indent();
            _fileChanged = false;            
        }

        private void New()
        {
            /* Create a new script file - we need to know the type of file to add and we can't rely on tvFiles
             * to always have a selection */
            var type = ScriptFileNodeType.Aliases;
            var node = _tvFiles.SelectedObject;
            if (node != null)
            {
                if (node.GetType() == typeof(ScriptFileNode))
                {
                    type = ((ScriptFileNode)node).Type;
                }
                else if (node.GetType() == typeof(ScriptData))
                {
                    type = GetNodeTypeFromScript((ScriptData)node);
                }
            }
            else
            {
                type = GetNodeTypeFromScript(_currentEditingScript);
            }
            if (type == ScriptFileNodeType.Variables)
            {
                /* Don't do ANYTHING */
                return;
            }
            /* We generate a random file name based on type (aliases01) */
            string name;
            switch (type)
            {
                case ScriptFileNodeType.Aliases:
                    name = string.Format("aliases{0}", _aliases.Count + 1);
                    break;

                default:
                    name = string.Format("popups{0}", _popups.Count + 1);
                    break;
            }
            var script = new ScriptData {Name = name, ContentsChanged = true};
            /* Don't need to find what FileNode this script belongs to, as when added to either list below, _files
             * is referenced to that list - any changes made on the lists is reflected by _files */
            switch (type)
            {
                case ScriptFileNodeType.Aliases:
                    _aliases.Add(script);
                    break;

                case ScriptFileNodeType.Popups:
                    System.Diagnostics.Debug.Print("should be here");
                    _popups.Add(script);
                    break;
            }
            _tvFiles.RefreshObjects(_files);
            _tvFiles.Expand(script);
            _tvFiles.SelectObject(script);
            RebuildAllScripts();
        }

        private void Save()
        {            
            if (_currentEditingScript == null)
            {
                return;
            }
            /* Make sure to dump contents of edit window text */
            if (_currentEditingScript.ContentsChanged)
            {
                _currentEditingScript.RawScriptData = new List<string>(_txtEdit.EditBox.Lines);
            }
            /* Save current editing file */
            switch (_currentEditingScript.Name.ToUpper())
            {
                case "VARIABLES":
                    RebuildVariables(_currentEditingScript);
                    break;

                default:
                    /* Renaming of scripts happens elsewhere and does not need this flag set to true
                     * nor the file deleted - just rename file and change the Name flag */
                    ScriptManager.SaveScript(_currentEditingScript, Functions.MainDir(string.Format(@"\scripts\{0}", _currentEditingScript), false));
                    RebuildAllScripts();
                    break;
            }
            _currentEditingScript.ContentsChanged = false;
        }

        private void SaveAll()
        {
            /* Make sure to dump contents of edit window text */
            if (_currentEditingScript != null && _currentEditingScript.ContentsChanged)
            {
                _currentEditingScript.RawScriptData = new List<string>(_txtEdit.EditBox.Lines);
            }            
            foreach (var file in _files)
            {
                foreach (var s in file.Data)
                {
                    switch (file.Type)
                    {
                        case ScriptFileNodeType.Variables:
                            if (s.ContentsChanged)
                            {
                                RebuildVariables(s);
                            }
                            break;

                        default:
                            if (s.ContentsChanged)
                            {
                                /* Renaming of scripts happens elsewhere and does not need this flag set to true
                                 * nor the file deleted - just rename file and change the Name flag */
                                ScriptManager.SaveScript(s, Functions.MainDir(string.Format(@"\scripts\{0}", s), false));                                
                            }                            
                            break;
                    }
                    s.ContentsChanged = false;
                }
            }            
            RebuildAllScripts();
        }

        private void RebuildAllScripts()
        {
            /* Make sure to clone these lists back to master lists (adding of new files/renaming) */
            ScriptManager.AliasData = _aliases.Clone();
            ScriptManager.PopupData = _popups.Clone();
            /* Build script data */
            ScriptManager.BuildScripts(ScriptManager.AliasData, ScriptManager.Aliases);
            ScriptManager.BuildScripts(ScriptManager.PopupData, ScriptManager.Popups);
            /* Update filesnames in settings */
            ScriptManager.BuildFileList(SettingsManager.Settings.Scripts.Aliases, ScriptManager.AliasData);
            ScriptManager.BuildFileList(SettingsManager.Settings.Scripts.Popups, ScriptManager.PopupData);
        }

        private static void RebuildVariables(ScriptData s)
        {
            /* A little more involved... */
            ScriptManager.Variables.Variable.Clear();
            foreach (var v in s.RawScriptData)
            {
                var globalVar = new ScriptVariable();
                var i = v.IndexOf('=');
                if (i != -1)
                {
                    globalVar.Name = Functions.GetFirstWord(v.Substring(0, i));
                    globalVar.Value = v.Substring(i + 1);
                }
                else
                {
                    globalVar.Name = v;
                }
                ScriptManager.Variables.Variable.Add(globalVar);
            }
            ScriptManager.SaveVariables(Functions.MainDir(@"\scripts\variables.xml", false));
        }

        private ScriptFileNodeType GetNodeTypeFromScript(ScriptData script)
        {
            return (from file in _files from s in file.Data where s == script select file.Type).FirstOrDefault();
        }
    }
}