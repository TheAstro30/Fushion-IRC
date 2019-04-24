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
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Drawing;

namespace ircScript.Controls.SyntaxHighlight.Helpers.Lines
{
    public class Line : IList<Char>
    {
        protected List<Char> Chars;

        public string FoldingStartMarker { get; set; }
        public string FoldingEndMarker { get; set; }

        public bool IsChanged { get; set; }

        public DateTime LastVisit { get; set; }

        public Brush BackgroundBrush { get; set;}

        public int UniqueId { get; private set; }

        public int AutoIndentSpacesNeededCount { get; set; }

        internal Line(int uid)
        {
            UniqueId = uid;
            Chars = new List<Char>();
        }

        public void ClearStyle(StyleIndex styleIndex)
        {
            FoldingStartMarker = null;
            FoldingEndMarker = null;
            for (var i = 0; i < Count; i++)
            {
                var c = this[i];
                c.Style &= ~styleIndex;
                this[i] = c;
            }
        }

        public virtual string Text
        {
            get
            {
                var sb = new StringBuilder(Count);
                foreach (var c in this)
                {
                    sb.Append(c.C);
                }
                return sb.ToString();
            }
        }

        public void ClearFoldingMarkers()
        {
            FoldingStartMarker = null;
            FoldingEndMarker = null;
        }

        public int StartSpacesCount
        {
            get
            {
                var spacesCount = 0;
                for (var i = 0; i < Count; i++)
                {
                    if (this[i].C == ' ')
                    {
                        spacesCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                return spacesCount;
            }
        }

        public int IndexOf(Char item)
        {
            return Chars.IndexOf(item);
        }

        public void Insert(int index, Char item)
        {
            Chars.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Chars.RemoveAt(index);
        }

        public Char this[int index]
        {
            get
            {
                return Chars[index];
            }
            set
            {
                Chars[index] = value;
            }
        }

        public void Add(Char item)
        {
            Chars.Add(item);
        }

        public void Clear()
        {
            Chars.Clear();
        }

        public bool Contains(Char item)
        {
            return Chars.Contains(item);
        }

        public void CopyTo(Char[] array, int arrayIndex)
        {
            Chars.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Chars.Count; }
        }

        public bool IsReadOnly
        {
            get {  return false; }
        }

        public bool Remove(Char item)
        {
            return Chars.Remove(item);
        }

        public IEnumerator<Char> GetEnumerator()
        {
            return Chars.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Chars.GetEnumerator();
        }

        public virtual void RemoveRange(int index, int count)
        {
            if (index >= Count)
            {
                return;
            }
            Chars.RemoveRange(index, Math.Min(Count - index, count));
        }

        public virtual void TrimExcess()
        {
            Chars.TrimExcess();
        }

        public virtual void AddRange(IEnumerable<Char> collection)
        {
            Chars.AddRange(collection);
        }
    }
}
