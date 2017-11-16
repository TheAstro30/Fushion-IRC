/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ircCore.Controls.ChildWindows.Nicklist.Helpers;
using ircCore.Controls.ChildWindows.Nicklist.Structures;
using libolv;
using libolv.Implementation.Events;

namespace ircCore.Controls.ChildWindows.Nicklist
{
    /* IRC Nicklist control sorted by user mode */
    public class Nicklist : UserControl
    {
        private readonly List<NickData> _list = new List<NickData>();
        
        private readonly ObjectListView _nickList;
        private readonly OlvColumn _nickColumn;
        private ImageList _images;

        private readonly NickComparer _nickComparer = new NickComparer();

        private readonly Regex _prefixCompare = new Regex("^~|!|\\.|@|\\+|%|\\&", RegexOptions.Compiled);

        private bool _showIcons;
        private bool _showPrefix;
        private string _userModes;
        private string _userModeCharacters;

        private List<string> _userMode = new List<string>();
        private List<string> _userModeCharacter = new List<string>();

        /* Nick tab completetion */
        private bool _keySearch;
        private int _keyPoint;
        private int _selPoint;
        private int _nickMatches;
        private string _keyWord;
        private string _rest;

        public event Action OnNicklistRightClick;
        public event Action OnNicklistDoubleClick;

        public Nicklist()
        {
            /* Note: this will be replaced by a public property set to an image list in Theme.cs */
            _images = new ImageList
                          {
                              ColorDepth = ColorDepth.Depth32Bit,
                              ImageSize = new Size(16, 16)
                          };

            _nickList = new ObjectListView
                            {
                                MultiSelect = true,
                                FullRowSelect = true,
                                HideSelection = false,
                                OwnerDraw = true,
                                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                                View = View.Details,
                                BorderStyle = BorderStyle.None
                            };

            _nickColumn = new OlvColumn("Members: 0", "Nick")
                              {
                                  Sortable = false,
                                  IsEditable = false,
                                  IsVisible = true,
                                  Width = 300
                              };

            _nickList.AllColumns.Add(_nickColumn);
            _nickList.Columns.Add(_nickColumn);
            _nickList.RebuildColumns();

            Controls.Add(_nickList);

            _nickList.MouseUp += OnListMouseUp;
            _nickList.MouseDoubleClick += OnListMouseDoubleClick;
            _nickList.CellToolTipShowing += OnCellToolTipShowing;

            _userModes = "ov";
            _userModeCharacters = "@+";

            _userMode.AddRange(new[] {"o", "v"});
            _userModeCharacter.AddRange(new[] { "@", "+" });
        }

        /* Overrides */
        protected override void OnResize(EventArgs e)
        {
            _nickList.SetBounds(0, 0, ClientRectangle.Width, ClientRectangle.Height);
            _nickColumn.Width = ClientRectangle.Width;
            base.OnResize(e);
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                _nickList.Font = value;
                base.Font = value;
            }
        }

        /* Public properties */
        public ImageList Images
        {
            get { return _images; }
            set
            {
                _images = value;
                _nickList.SmallImageList = _images;
                _nickList.RefreshObjects(_list);
            }
        }

        public bool ShowIcons
        {
            get { return _showIcons; }
            set
            {
                _showIcons = value;
                _nickColumn.ImageGetter = delegate(object row)
                                              {
                                                  if (!_showIcons)
                                                  {
                                                      return null;
                                                  }
                                                  switch (((NickData) row).GetUserMode())
                                                  {
                                                      case "!":
                                                      case "~":
                                                      case ".":
                                                          return 0;

                                                      case "&":
                                                          return 1;

                                                      case "@":
                                                          return 2;

                                                      case "%":
                                                          return 3;

                                                      case "+":
                                                          return 4;

                                                      default:
                                                          return null;
                                                  }
                                              };
                _nickList.RefreshObjects(_list);
            }
        }

        public bool ShowPrefix
        {
            get { return _showPrefix; }
            set
            {
                _showPrefix = value;
                _nickColumn.AspectName = _showPrefix ? "ToString" : "Nick";
                _nickList.RefreshObjects(_list);
            }
        }

        public string UserModes
        {
            get { return _userModes; }
            set
            {
                _userModes = value;
                _userMode = new List<string>();
                if (string.IsNullOrEmpty(_userModes))
                {
                    _userModes = "ov";
                }
                for (var i = 0; i <= _userModes.Length - 1; i++)
                {
                    _userMode.Add(_userModes[i].ToString());
                }
            }
        }
        
        public string UserModeCharacters
        {
            get { return _userModeCharacters; }
            set
            {
                _userModeCharacters = value;
                _userModeCharacter = new List<string>();
                if (string.IsNullOrEmpty(_userModeCharacters))
                {
                    _userModeCharacters = "@+";
                }
                for (var i = 0; i <= _userModeCharacters.Length - 1; i++)
                {
                    _userModeCharacter.Add(_userModeCharacters[i].ToString());
                }
            }
        }

        public List<string> SelectedNicks
        {
            get
            {
                return (from object sel in _nickList.SelectedObjects select ((NickData) sel).Nick).ToList();
            }
        }

        /* Public methods */               
        public void AddNicks(string nicks)
        {            
            var s = nicks.Split(' ');            
            foreach (var nick in s)
            {                
                /* Need to split user mode from front of the nick */
                var nd = new NickData();
                var n = _prefixCompare.Match(nick);
                if (!string.IsNullOrEmpty(n.Value))
                {                 
                    nd.Nick = nick.Replace(n.Value, "");
                    nd.AddUserMode(n.Value);                    
                }
                else
                {
                    nd.Nick = nick;
                }
                /* Double check the nick isn't already in the list */
                if (_list.FirstOrDefault(o => o.Nick == nd.Nick) == null)
                {
                    _list.Add(nd);
                }                
            }
            _list.Sort(_nickComparer);
            _nickList.SetObjects(_list);
            /* Update column header */
            _nickColumn.Text = string.Format("Members: {0}", _list.Count);
        }

        public void AddNick(string nick, string address)
        {
            var nd = new NickData
                         {
                             Nick = nick, Address = address
                         };
            /* Double check the nick isn't already in the list */
            if (_list.FirstOrDefault(o => o.Nick == nd.Nick) == null)
            {
                _list.Add(nd);
            }
            _list.Sort(_nickComparer);
            UpdateNicklist();
        }

        public void UpdateNickAddress(string nick, string address)
        {
            var nd = _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            if (nd != null)
            {
                nd.Address = address;                
            }
        }

        public void RemoveNick(string nick)
        {
            var n = _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            if (n == null)
            {                
                return;
            }
            _list.Remove(n);
            _nickList.RemoveObject(n);
            /* Update column header */
            _nickColumn.Text = string.Format("Members: {0}", _list.Count);
        }

        public void RenameNick(string nick, string newNick)
        {
            var nd = _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            if (nd == null)
            {
                return;
            }
            nd.Nick = newNick;
            _list.Sort(_nickComparer);
            UpdateNicklist();
        }

        public void AddUserMode(string nick, string modeChar)
        {
            var n = _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            if (n == null) { return; }
            var mode = _userMode.FindIndex(o => o == modeChar);
            if (mode == -1 || mode > _userModeCharacter.Count - 1)
            {                
                return;
            }            
            if (!n.AddUserMode(_userModeCharacter[mode]))
            {                       
                return;
            }            
            _list.Sort(_nickComparer);
            UpdateNicklist();
        }

        public void RemoveUserMode(string nick, string modeChar)
        {
            var n = _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            if (n == null) { return; }
            var mode = _userMode.FindIndex(o => o == modeChar);
            if (mode == -1 || mode > _userModeCharacter.Count - 1)
            {
                return;
            }
            if (!n.RemoveUserMode(_userModeCharacter[mode]))
            {
                return;
            }
            _list.Sort(_nickComparer);
            UpdateNicklist();
        }

        public void Clear()
        {            
            _list.Clear();
            _nickList.ClearObjects();
            /* Update column header */
            _nickColumn.Text = string.Format("Members: {0}", _list.Count);         
        }

        public bool ContainsNick(string nick)
        {
            var n = _prefixCompare.Match(nick);
            if (!string.IsNullOrEmpty(n.Value))
            {
                nick = nick.Replace(n.Value, "");
            }
            return _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase)) != null;
        }

        public string GetNickPrefix(string nick)
        {
            var n = _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            return n != null ? n.GetUserMode() : string.Empty;
        }

        public string GetAddress(string nick)
        {
            var n = _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            return n != null ? string.Format("{0}!{1}", n.Nick, n.Address) : string.Empty;
        }

        /* Tab completion */
        public string TabNextNick(string inputText, string childWindowCaption, int selectionStart, ref string restOfString)
        {
            /* This works just like mIRC's type a letter, press TAB, complete nick .. successive
               TAB presses returns the next nick beginning with that letter, etc */
            if (!string.IsNullOrEmpty(inputText))
            {
                int i;
                if (_keySearch == false)
                {
                    /* Search for a space */
                    _selPoint = selectionStart;
                    /* Are we in the middle of the line? */
                    i = inputText.LastIndexOf(' ', _selPoint - 1);
                    _keyWord = i != -1 ? inputText.Substring(i + 1).Trim() : inputText.Trim();
                    i = _keyWord.IndexOf(' ');
                    if (i != -1)
                    {
                        _rest = _keyWord.Substring(i + 1).Trim();
                        _keyWord = _keyWord.Substring(0, i).Trim();
                    }
                    _keySearch = true;
                }
                restOfString = _rest;
                if (childWindowCaption.Length >= _keyWord.Length)
                {
                    if (!_keyWord.Equals(childWindowCaption.Substring(0, _keyWord.Length), StringComparison.InvariantCultureIgnoreCase))
                    {
                        /* Nicklist (match) */
                        if (_keyPoint > _list.Count - 1)
                        {
                            _keyPoint = 1;
                            _nickMatches = 0;
                        }
                        /* NickSearch: */
                        for (i = _keyPoint; i <= _list.Count - 1; i++)
                        {
                            var nick = _list[i].Nick;
                            if (nick.Length < _keyWord.Length || !_keyWord.Equals(nick.Substring(0, _keyWord.Length), StringComparison.InvariantCultureIgnoreCase))
                            {
                                continue;
                            }
                            /* We found a match */
                            _keyPoint = i + 1;
                            _nickMatches += 1;
                            /* Now replace the text */
                            return string.Format("{0}{1}", _selPoint > 1 ? inputText.Substring(0, _selPoint - _keyWord.Length) : "", nick);
                        }
                    }
                    else if (_keyWord.Length > 0)
                    {
                        /* Channel name match */
                        if (_keyWord.Equals(childWindowCaption.Substring(0, _keyWord.Length), StringComparison.InvariantCultureIgnoreCase))
                        {
                            /* It matches */
                            return string.Format("{0}{1}", _selPoint > 1 ? inputText.Substring(0, _selPoint - _keyWord.Length) : "", childWindowCaption);
                        }
                    }
                    else
                    {
                        ClearTabNick();
                    }
                }
                else
                {
                    ClearTabNick();
                }
                /* Loop back to nick search if matches is greater than 1 */
                if (_nickMatches > 1)
                {
                    _keyPoint = 0;
                    _nickMatches = 0;
                    return TabNextNick(inputText, childWindowCaption, selectionStart, ref restOfString);
                }
            }
            /* Else nothing matches */
            ClearTabNick();
            SystemSounds.Beep.Play();
            return inputText;
        }

        public void ClearTabNick()
        {
            _keySearch = false;
            _keyPoint = 0;
            _selPoint = 0;
            _nickMatches = 0;
            _keyWord = string.Empty;
            _rest = string.Empty;
        }

        /* Private methods */
        private void UpdateNicklist()
        {
            /* Pain in the ass OLV doesn't refresh a sorted list >:/ ! */
            var sel = _nickList.SelectedObjects;
            _nickList.SetObjects(_list);
            /* Maintain any selected nicks */
            _nickList.SelectedObjects = sel;
            /* Update column header */
            _nickColumn.Text = string.Format("Members: {0}", _list.Count);
        }

        /* Mouse events */
        private void OnListMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || _nickList.SelectedObjects == null)
            {
                return;
            }
            if (OnNicklistRightClick != null)
            {
                OnNicklistRightClick();
            }
        }

        private void OnListMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _nickList.SelectedObjects == null)
            {
                return;
            }
            if (OnNicklistDoubleClick != null)
            {
                OnNicklistDoubleClick();
            }
        }

        private static void OnCellToolTipShowing(object sender, ToolTipShowingEventArgs e)
        {
            var nd = (NickData) e.Model;
            var um = nd.GetAllUserModes();
            e.Title = nd.Nick;
            e.Text = !string.IsNullOrEmpty(um)
                         ? string.Format("Address: {0}\r\n{1} ({2})", nd.Address, nd.GetUserModeString(), um)
                         : string.Format("Address: {0}\r\n{1}", nd.Address, nd.GetUserModeString());
            e.IsBalloon = true;
            e.Handled = true;
        }
    }    
}
