//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ircScript.Controls.SyntaxHighlight.Controls;

namespace ircScript.Controls.SyntaxHighlight.Helpers.Hints
{
    public class Hints : ICollection<Hint>, IDisposable
    {
        private readonly List<Hint> _items = new List<Hint>();
        private readonly FastColoredTextBox _tb;

        public Hints(FastColoredTextBox tb)
        {
            _tb = tb;
            tb.TextChanged += OnTextBoxTextChanged;
            tb.KeyDown += OnTextBoxKeyDown;
            tb.VisibleRangeChanged += OnTextBoxVisibleRangeChanged;
        }

        public IEnumerator<Hint> GetEnumerator()
        {
            return ((IEnumerable<Hint>) _items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _items.Clear();
            if (_tb.Controls.Count == 0)
            {
                return;
            }
            var toDelete = _tb.Controls.OfType<UnfocusablePanel>().Cast<Control>().ToList();
            foreach (var item in toDelete)
            {
                _tb.Controls.Remove(item);
            }
            for (var i = 0; i < _tb.LineInfos.Count; i++)
            {
                var li = _tb.LineInfos[i];
                li.BottomPadding = 0;
                _tb.LineInfos[i] = li;
            }
            _tb.NeedRecalc();
            _tb.Invalidate();
            _tb.Select();
            _tb.ActiveControl = null;
        }

        public void Add(Hint hint)
        {
            _items.Add(hint);
            if (hint.Inline)
            {
                var li = _tb.LineInfos[hint.Range.Start.Line];
                hint.TopPadding = li.BottomPadding;
                li.BottomPadding += hint.HostPanel.Height;
                _tb.LineInfos[hint.Range.Start.Line] = li;
                _tb.NeedRecalc(true);
            }
            LayoutHint(hint);
            _tb.OnVisibleRangeChanged();
            hint.HostPanel.Parent = _tb;
            _tb.Select();
            _tb.ActiveControl = null;
            _tb.Invalidate();
        }

        public bool Contains(Hint item)
        {
            return _items.Contains(item);
        }

        public void CopyTo(Hint[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Hint item)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _tb.TextChanged -= OnTextBoxTextChanged;
            _tb.KeyDown -= OnTextBoxKeyDown;
            _tb.VisibleRangeChanged -= OnTextBoxVisibleRangeChanged;
        }

        protected virtual void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && e.Modifiers == Keys.None)
            {
                Clear();
            }
        }

        protected virtual void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            Clear();
        }

        private void OnTextBoxVisibleRangeChanged(object sender, EventArgs e)
        {
            if (_items.Count == 0)
            {
                return;
            }
            _tb.NeedRecalc(true);
            foreach (var item in _items)
            {
                LayoutHint(item);
                item.HostPanel.Invalidate();
            }
        }

        private void LayoutHint(Hint hint)
        {
            if (hint.Inline)
            {
                if (hint.Range.Start.Line < _tb.LineInfos.Count - 1)
                {
                    hint.HostPanel.Top = _tb.LineInfos[hint.Range.Start.Line + 1].StartY - hint.TopPadding -
                                         hint.HostPanel.Height - _tb.VerticalScroll.Value;
                }
                else
                {
                    hint.HostPanel.Top = _tb.TextHeight + _tb.Paddings.Top - hint.HostPanel.Height -
                                         _tb.VerticalScroll.Value;
                }
            }
            else
            {
                if (hint.Range.Start.Line > _tb.LinesCount - 1) return;
                if (hint.Range.Start.Line == _tb.LinesCount - 1)
                {
                    var y = _tb.LineInfos[hint.Range.Start.Line].StartY - _tb.VerticalScroll.Value + _tb.CharHeight;
                    if (y + hint.HostPanel.Height + 1 > _tb.ClientRectangle.Bottom)
                    {
                        hint.HostPanel.Top = Math.Max(0,
                                                      _tb.LineInfos[hint.Range.Start.Line].StartY -
                                                      _tb.VerticalScroll.Value - hint.HostPanel.Height);
                    }
                    else
                    {
                        hint.HostPanel.Top = y;
                    }
                }
                else
                {
                    hint.HostPanel.Top = _tb.LineInfos[hint.Range.Start.Line + 1].StartY - _tb.VerticalScroll.Value;
                    if (hint.HostPanel.Bottom > _tb.ClientRectangle.Bottom)
                    {
                        hint.HostPanel.Top = _tb.LineInfos[hint.Range.Start.Line + 1].StartY - _tb.CharHeight -
                                             hint.TopPadding - hint.HostPanel.Height - _tb.VerticalScroll.Value;
                    }
                }
            }
            if (hint.Dock == DockStyle.Fill)
            {
                hint.Width = _tb.ClientSize.Width - _tb.LeftIndent - 2;
                hint.HostPanel.Left = _tb.LeftIndent;
            }
            else
            {
                var p1 = _tb.PlaceToPoint(hint.Range.Start);
                var p2 = _tb.PlaceToPoint(hint.Range.End);
                var cx = (p1.X + p2.X)/2;
                var x = cx - hint.HostPanel.Width/2;
                hint.HostPanel.Left = Math.Max(_tb.LeftIndent, x);
                if (hint.HostPanel.Right > _tb.ClientSize.Width)
                {
                    hint.HostPanel.Left = Math.Max(_tb.LeftIndent, x - (hint.HostPanel.Right - _tb.ClientSize.Width));
                }
            }
        }
    }
}