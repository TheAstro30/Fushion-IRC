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
using ircCore.Controls.ChildWindows.Input;
using ircCore.Controls.ChildWindows.Nicklist.Helpers;
using ircCore.Controls.ChildWindows.Nicklist.Structures;

namespace ircCore.Controls.ChildWindows.Nicklist
{
    /* IRC Nicklist control sorted by user mode */
    public class Nicklist : UserControl
    {
        private readonly List<NickData> _list = new List<NickData>();
        private readonly NicklistStyler _baseList;
        private readonly NickComparer _nickComparer = new NickComparer();
        private readonly BindingSource _dataSource = new BindingSource();
        private bool _ignoreFirstSelection;

        private readonly Regex _prefixCompare = new Regex("^~|!|\\.|@|\\+|%|\\&", RegexOptions.Compiled);

        private string _userModes;
        private string _userModeCharacters;

        private List<string> _userMode = new List<string>();
        private List<string> _userModeCharacter = new List<string>();

        private List<object> _selectedItems = new List<object>();

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
            _dataSource.DataSource = _list;
            _baseList = new NicklistStyler
                            {
                                IntegralHeight = false,
                                BorderStyle = BorderStyle.None,
                                DataSource = _dataSource,
                                SelectionMode = SelectionMode.MultiExtended
                            };
            Controls.Add(_baseList);
            _baseList.SelectedIndexChanged += OnSelectionChanged;
            _baseList.MouseDown += OnListMouseDown;
            _baseList.MouseDoubleClick += OnListMouseDoubleClick;

            _userModes = "ov";
            _userModeCharacters = "@+";

            _userMode.AddRange(new[] {"o", "v"});
            _userModeCharacter.AddRange(new[] { "@", "+" });
        }

        /* Public properties */
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
                    _userModeCharacters = "ov";
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
                return (from object sel in _baseList.SelectedItems select ((NickData) sel).Nick).ToList();
            }
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

        /* Overrides */
        protected override void OnResize(EventArgs e)
        {
            _baseList.SetBounds(0, 0, ClientRectangle.Width, ClientRectangle.Height);
            base.OnResize(e);
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                _baseList.Font = value;
                base.Font = value;
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
            BeginListUpdate();
            _list.Sort(_nickComparer);
            EndListUpdate("");
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
            BeginListUpdate();
            _list.Sort(_nickComparer);
            EndListUpdate("");
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
            var n = _list.FirstOrDefault(o => o.Nick == nick);
            if (n == null)
            {                
                return;
            }
            BeginListUpdate();
            _list.Remove(n);
            EndListUpdate(nick);
        }

        public void RenameNick(string nick, string newNick)
        {
            var nd = _list.FirstOrDefault(o => o.Nick.Equals(nick, StringComparison.InvariantCultureIgnoreCase));
            if (nd == null)
            {
                return;
            }
            nd.Nick = newNick;
            BeginListUpdate();
            _list.Sort(_nickComparer);
            EndListUpdate("");
        }

        public void AddUserMode(string nick, string modeChar)
        {
            var n = _list.FirstOrDefault(o => o.Nick == nick);
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
            BeginListUpdate();
            _list.Sort(_nickComparer);
            EndListUpdate(nick);
        }

        public void RemoveUserMode(string nick, string modeChar)
        {
            var n = _list.FirstOrDefault(o => o.Nick == nick);
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
            BeginListUpdate();
            _list.Sort(_nickComparer);
            EndListUpdate(nick);
        }

        public void Clear()
        {            
            _list.Clear();
            EndListUpdate("");            
        }

        /* List binding updating */
        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (!_ignoreFirstSelection) { return; }
            _ignoreFirstSelection = false;
            /* This fixes a stupid issue with binding to a list of it always selecting the first item as default */
            _baseList.ClearSelected();
            _baseList.SelectedIndex = -1;
        }

        private void BeginListUpdate()
        {
            _ignoreFirstSelection = true;
            _selectedItems = new List<object>();
            if (_baseList.Items.Count > 0)
            {
                _selectedItems.AddRange(_baseList.SelectedItems.Cast<object>());
            }
        }

        private void EndListUpdate(string nick)
        {
            _dataSource.ResetBindings(false);
            foreach (var sel in _selectedItems)
            {
                if (string.IsNullOrEmpty(nick))
                {
                    _baseList.SelectedItems.Add(sel);
                }
                else
                {
                    var n = _list.FirstOrDefault(o => o.Nick == ((NickData)sel).Nick);
                    if (n != null)
                    {
                        _baseList.SelectedItems.Add(sel);
                    }
                    else if(_selectedItems.Count == 1)
                    {
                        /* This fixes a stupid issue with binding to a list of it always selecting the first item as default */
                        _baseList.ClearSelected();
                        _baseList.SelectedIndex = -1;
                    }
                }                
            }
        }

        /* Mouse events */
        private void OnListMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) { return; }
            var sel = _baseList.SelectedItem;
            if (sel == null)
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
            if (e.Button != MouseButtons.Left) { return; }
            var sel = _baseList.SelectedItem;
            if (sel == null)
            {
                return;
            }
            if (OnNicklistDoubleClick != null)
            {
                OnNicklistDoubleClick();
            }
        }
    }    
}
