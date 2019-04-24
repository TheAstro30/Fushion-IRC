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

namespace ircScript.Controls.SyntaxHighlight.Helpers
{
    public class LimitedStack<T>
    {
        private T[] _items;
        private int _start;

        public int MaxItemCount
        {
            get { return _items.Length; }
        }

        public int Count { get; private set; }

        public LimitedStack(int maxItemCount)
        {
            _items = new T[maxItemCount];
            Count = 0;
            _start = 0;
        }

        public T Pop()
        {
            if (Count == 0)
            {
                throw new Exception("Stack is empty");
            }
            var i = LastIndex;
            var item = _items[i];
            _items[i] = default(T);
            Count--;
            return item;
        }

        public int LastIndex
        {
            get { return (_start + Count - 1) % _items.Length; }
        }

        public T Peek()
        {
            return Count == 0 ? default(T) : _items[LastIndex];
        }

        public void Push(T item)
        {
            if (Count == _items.Length)
            {
                _start = (_start + 1) % _items.Length;
            }
            else
            {
                Count++;
            }
            _items[LastIndex] = item;
        }

        public void Clear()
        {
            _items = new T[_items.Length];
            Count = 0;
            _start = 0;
        }

        public T[] ToArray()
        {
            var result = new T[Count];
            for (var i = 0; i < Count; i++)
            {
                result[i] = _items[(_start + i) % _items.Length];
            }
            return result;
        }
    }
}