/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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
                if (string.IsNullOrEmpty(_userModes))
                {
                    _userModes = "@+";
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
            return _list.FirstOrDefault(o => o.Nick == nick) != null;
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
            var nd = _list.FirstOrDefault(o => o.Nick.ToLower() == nick.ToLower());
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
