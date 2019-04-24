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
using System.Drawing;
using System.IO;
using System.Text;
using ircScript.Controls.SyntaxHighlight.Commands;
using ircScript.Controls.SyntaxHighlight.Helpers.Lines;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls.SyntaxHighlight.Helpers.TextSource
{
    public class TextSource : IList<Line>, IDisposable
    {
        public class TextChangedEventArgs : EventArgs
        {
            public int FromLine;
            public int ToLine;

            public TextChangedEventArgs(int fromLine, int toLine)
            {
                FromLine = fromLine;
                ToLine = toLine;
            }
        }

        protected readonly List<Line> Lines = new List<Line>();
        public readonly Style[] Styles;
        protected LinesAccessor LinesAccessor;

        private FastColoredTextBox _currentTextBox;
        private int _lastLineUniqueId;

        public TextSource(FastColoredTextBox currentTextBox)
        {
            Styles = Enum.GetUnderlyingType(typeof(StyleIndex)) == typeof(UInt32) ? new Style[32] : new Style[16];
            Init(currentTextBox);
        }

        public CommandManager Manager { get; set; }
        public TextStyle DefaultStyle { get; set; }

        public event EventHandler<LineInsertedEventArgs> LineInserted;
        public event EventHandler<LineRemovedEventArgs> LineRemoved;
        public event EventHandler<TextChangedEventArgs> TextChanged;
        public event EventHandler<TextChangedEventArgs> RecalcNeeded;
        public event EventHandler<TextChangedEventArgs> RecalcWordWrap;
        public event EventHandler<TextChangingEventArgs> TextChanging;
        public event EventHandler CurrentTextBoxChanged;

        public FastColoredTextBox CurrentTextBox
        {
            get { return _currentTextBox; }
            set
            {
                if (_currentTextBox == value)
                {
                    return;
                }
                _currentTextBox = value;
                OnCurrentTextBoxChanged();
            }
        }

        public virtual bool IsNeedBuildRemovedLineIds
        {
            get { return LineRemoved != null; }
        }

        public virtual void Dispose()
        {
            /* Empty */
        }

        public virtual Line this[int i]
        {
            get { return Lines[i]; }
            set { throw new NotImplementedException(); }
        }

        public IEnumerator<Line> GetEnumerator()
        {
            return Lines.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) Lines;
        }

        public virtual int IndexOf(Line item)
        {
            return Lines.IndexOf(item);
        }

        public virtual void Insert(int index, Line item)
        {
            InsertLine(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            RemoveLine(index);
        }

        public virtual void Add(Line item)
        {
            InsertLine(Count, item);
        }

        public virtual void Clear()
        {
            RemoveLine(0, Count);
        }

        public virtual bool Contains(Line item)
        {
            return Lines.Contains(item);
        }

        public virtual void CopyTo(Line[] array, int arrayIndex)
        {
            Lines.CopyTo(array, arrayIndex);
        }

        public virtual int Count
        {
            get { return Lines.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(Line item)
        {
            var i = IndexOf(item);
            if (i < 0)
            {
                return false;
            }
            RemoveLine(i);
            return true;
        }

        public virtual void ClearIsChanged()
        {
            foreach (var line in Lines)
            {
                line.IsChanged = false;
            }
        }

        public virtual Line CreateLine()
        {
            return new Line(GenerateUniqueLineId());
        }

        private void OnCurrentTextBoxChanged()
        {
            if (CurrentTextBoxChanged != null)
            {
                CurrentTextBoxChanged(this, EventArgs.Empty);
            }
        }

        public virtual void InitDefaultStyle()
        {
            DefaultStyle = new TextStyle(null, null, FontStyle.Regular);
        }

        public virtual bool IsLineLoaded(int iLine)
        {
            return Lines[iLine] != null;
        }

        public virtual IList<string> GetLines()
        {
            return LinesAccessor;
        }

        public virtual int BinarySearch(Line item, IComparer<Line> comparer)
        {
            return Lines.BinarySearch(item, comparer);
        }

        public virtual int GenerateUniqueLineId()
        {
            return _lastLineUniqueId++;
        }

        public virtual void InsertLine(int index, Line line)
        {
            Lines.Insert(index, line);
            OnLineInserted(index);
        }

        public virtual void OnLineInserted(int index)
        {
            OnLineInserted(index, 1);
        }

        public virtual void OnLineInserted(int index, int count)
        {
            if (LineInserted != null)
                LineInserted(this, new LineInsertedEventArgs(index, count));
        }

        public virtual void RemoveLine(int index)
        {
            RemoveLine(index, 1);
        }

        public virtual void RemoveLine(int index, int count)
        {
            var removedLineIds = new List<int>();
            if (count > 0)
            {
                if (IsNeedBuildRemovedLineIds)
                {
                    for (var i = 0; i < count; i++)
                    {
                        removedLineIds.Add(this[index + i].UniqueId);
                    }
                }
            }
            Lines.RemoveRange(index, count);
            OnLineRemoved(index, count, removedLineIds);
        }

        public virtual void OnLineRemoved(int index, int count, List<int> removedLineIds)
        {
            if (count <= 0)
            {
                return;
            }
            if (LineRemoved != null)
            {
                LineRemoved(this, new LineRemovedEventArgs(index, count, removedLineIds));
            }
        }

        public virtual void OnTextChanged(int fromLine, int toLine)
        {
            if (TextChanged != null)
            {
                TextChanged(this, new TextChangedEventArgs(Math.Min(fromLine, toLine), Math.Max(fromLine, toLine)));
            }
        }

        public virtual void NeedRecalc(TextChangedEventArgs args)
        {
            if (RecalcNeeded != null)
            {
                RecalcNeeded(this, args);
            }
        }

        public virtual void OnRecalcWordWrap(TextChangedEventArgs args)
        {
            if (RecalcWordWrap != null)
            {
                RecalcWordWrap(this, args);
            }
        }

        public virtual void OnTextChanging()
        {
            string temp = null;
            OnTextChanging(ref temp);
        }

        public virtual void OnTextChanging(ref string text)
        {
            if (TextChanging == null)
            {
                return;
            }
            var args = new TextChangingEventArgs {InsertingText = text};
            TextChanging(this, args);
            text = args.InsertingText;
            if (args.Cancel)
            {
                text = string.Empty;
            }
        }

        public virtual int GetLineLength(int i)
        {
            return Lines[i].Count;
        }

        public virtual bool LineHasFoldingStartMarker(int iLine)
        {
            return !string.IsNullOrEmpty(Lines[iLine].FoldingStartMarker);
        }

        public virtual bool LineHasFoldingEndMarker(int iLine)
        {
            return !string.IsNullOrEmpty(Lines[iLine].FoldingEndMarker);
        }

        public virtual void SaveToFile(string fileName, Encoding enc)
        {
            using (var sw = new StreamWriter(fileName, false, enc))
            {
                for (var i = 0; i < Count - 1; i++)
                {
                    sw.WriteLine(Lines[i].Text);
                }
                sw.Write(Lines[Count - 1].Text);
            }
        }

        private void Init(FastColoredTextBox currentTextBox)
        {
            CurrentTextBox = currentTextBox;
            LinesAccessor = new LinesAccessor(this);
            Manager = new CommandManager(this);            
            InitDefaultStyle();
        }
    }
}