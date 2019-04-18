/* Syntax highlighter - by Uriel Guy
 * Original version 2005
 * This version 2019 - Jason James Newland
 */
using System;
using System.Collections;

namespace ircScript.Controls.SyntaxHightlight.Helpers
{
	public class SeperaratorCollection
	{
		private readonly ArrayList _innerList = new ArrayList();

		internal SeperaratorCollection()
		{
            /* Empty constructor */
		}

		internal char[] GetAsCharArray()
		{
			return (char[])_innerList.ToArray(typeof(char));
		}

        /* IList members */
		public bool IsReadOnly
		{
			get
			{
				return _innerList.IsReadOnly;
			}
		}

		public char this[int index]
		{
			get
			{
				return (char)_innerList[index];
			}
			set
			{
				_innerList[index] = value;
			}
		}

		public void RemoveAt(int index)
		{
			_innerList.RemoveAt(index);
		}

		public void Insert(int index, char value)
		{
			_innerList.Insert(index, value);
		}

		public void Remove(char value)
		{
			_innerList.Remove(value);
		}

		public bool Contains(char value)
		{
			return _innerList.Contains(value);
		}

		public void Clear()
		{
			_innerList.Clear();
		}

		public int IndexOf(char value)
		{
			return _innerList.IndexOf(value);
		}

		public int Add(char value)
		{
			return _innerList.Add(value);
		}

        public void AddRange(ICollection c)
        {
            _innerList.AddRange(c);
        }

		public bool IsFixedSize
		{
			get
			{
				return _innerList.IsFixedSize;
			}
		}

        /* ICollection members */
		public bool IsSynchronized
		{
			get
			{
				return _innerList.IsSynchronized;
			}
		}

		public int Count
		{
			get
			{
				return _innerList.Count;
			}
		}

		public void CopyTo(Array array, int index)
		{
			_innerList.CopyTo(array, index);
		}

		public object SyncRoot
		{
			get
			{
				return _innerList.SyncRoot;
			}
		}
		
        /* IEnumerable */
		public IEnumerator GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}
	}
}
