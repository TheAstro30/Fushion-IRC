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
using System.Collections.Generic;
using System.Linq;

namespace ircScript.Controls.SyntaxHighlight.Bookmarks
{
    public class Bookmarks : BookmarkBase
    {
        protected int Counter;
        protected List<Bookmark> Items = new List<Bookmark>();
        protected FastColoredTextBox TextBox;

        public Bookmarks(FastColoredTextBox textBox)
        {
            TextBox = textBox;
            textBox.LineInserted += TextBoxLineInserted;
            textBox.LineRemoved += TextBoxLineRemoved;
        }

        public override int Count
        {
            get { return Items.Count; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        protected virtual void TextBoxLineRemoved(object sender, LineRemovedEventArgs e)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Items[i].LineIndex < e.Index)
                {
                    continue;
                }
                if (Items[i].LineIndex >= e.Index + e.Count)
                {
                    Items[i].LineIndex = Items[i].LineIndex - e.Count;
                    continue;
                }
                var was = e.Index <= 0;
                foreach (var b in Items)
                {
                    if (b.LineIndex == e.Index - 1)
                    {
                        was = true;
                    }
                }
                if (was)
                {
                    Items.RemoveAt(i);
                    i--;
                }
                else
                {
                    Items[i].LineIndex = e.Index - 1;
                }
            }
        }

        protected virtual void TextBoxLineInserted(object sender, LineInsertedEventArgs e)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Items[i].LineIndex >= e.Index)
                {
                    Items[i].LineIndex = Items[i].LineIndex + e.Count;
                }
                else if (Items[i].LineIndex == e.Index - 1 && e.Count == 1)
                {
                    if (TextBox[e.Index - 1].StartSpacesCount == TextBox[e.Index - 1].Count)
                    {
                        Items[i].LineIndex = Items[i].LineIndex + e.Count;
                    }
                }
            }
        }

        public override void Dispose()
        {
            TextBox.LineInserted -= TextBoxLineInserted;
            TextBox.LineRemoved -= TextBoxLineRemoved;
        }

        public override IEnumerator<Bookmark> GetEnumerator()
        {
            return ((IEnumerable<Bookmark>) Items).GetEnumerator();
        }

        public override void Add(int lineIndex, string bookmarkName)
        {
            Add(new Bookmark(TextBox, bookmarkName ?? "Bookmark " + Counter, lineIndex));
        }

        public override void Add(int lineIndex)
        {
            Add(new Bookmark(TextBox, "Bookmark " + Counter, lineIndex));
        }

        public override void Clear()
        {
            Items.Clear();
            Counter = 0;
        }

        public override void Add(Bookmark bookmark)
        {
            if (Items.Any(bm => bm.LineIndex == bookmark.LineIndex))
            {
                return;
            }
            Items.Add(bookmark);
            Counter++;
            TextBox.Invalidate();
        }

        public override bool Contains(Bookmark item)
        {
            return Items.Contains(item);
        }

        public override bool Contains(int lineIndex)
        {
            return Items.Any(item => item.LineIndex == lineIndex);
        }

        public override void CopyTo(Bookmark[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public override bool Remove(Bookmark item)
        {
            TextBox.Invalidate();
            return Items.Remove(item);
        }

        public override bool Remove(int lineIndex)
        {
            var was = false;
            for (var i = 0; i < Count; i++)
            {
                if (Items[i].LineIndex != lineIndex)
                {
                    continue;
                }
                Items.RemoveAt(i);
                i--;
                was = true;
            }
            TextBox.Invalidate();
            return was;
        }

        public override Bookmark GetBookmark(int i)
        {
            return Items[i];
        }
    }
}