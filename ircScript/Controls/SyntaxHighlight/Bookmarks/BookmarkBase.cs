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

namespace ircScript.Controls.SyntaxHighlight.Bookmarks
{
    public abstract class BookmarkBase : ICollection<Bookmark>, IDisposable
    {
        public abstract void Add(Bookmark item);
        public abstract void Clear();
        public abstract bool Contains(Bookmark item);
        public abstract void CopyTo(Bookmark[] array, int arrayIndex);
        public abstract int Count { get; }
        public abstract bool IsReadOnly { get; }
        public abstract bool Remove(Bookmark item);
        public abstract IEnumerator<Bookmark> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Dispose();

        public abstract void Add(int lineIndex, string bookmarkName);
        public abstract void Add(int lineIndex);
        public abstract bool Contains(int lineIndex);
        public abstract bool Remove(int lineIndex);
        public abstract Bookmark GetBookmark(int i);
    }
}
