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

namespace ircScript.Controls.SyntaxHighlight.Helpers.Lines
{
    public class LinesAccessor : IList<string>
    {
        private readonly IList<Line> _ts;

        public LinesAccessor(IList<Line> ts)
        {
            _ts = ts;
        }

        public int IndexOf(string item)
        {
            for (var i = 0; i < _ts.Count; i++)
            {
                if (_ts[i].Text == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, string item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public string this[int index]
        {
            get
            {
                return _ts[index].Text;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(string item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(string item)
        {
            return _ts.Any(t => t.Text == item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            for (var i = 0; i < _ts.Count; i++)
            {
                array[i + arrayIndex] = _ts[i].Text;
            }
        }

        public int Count
        {
            get { return _ts.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(string item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _ts.Select(t => t.Text).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
