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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ircScript.Controls.SyntaxHighlight.Helpers.Lines;
using ircScript.Controls.SyntaxHighlight.Helpers.TextSource;
using ircScript.Controls.SyntaxHighlight.Highlight;
using ircScript.Controls.SyntaxHighlight.Styles;

namespace ircScript.Controls.SyntaxHighlight.Helpers.TextRange
{
    public class Range : IEnumerable<Place>
    {
        private List<Place> _cachedCharIndexToPlace;
        private string _cachedText;
        private int _cachedTextVersion = -1;

        private bool _columnSelectionMode;
        private Place _end;
        private int _preferedPos = -1;
        private Place _start;
        private int _updating;

        public readonly FastColoredTextBox TextBox;

        public virtual bool IsEmpty
        {
            get
            {
                if (ColumnSelectionMode)
                    return Start.Char == End.Char;
                return Start == End;
            }
        }

        public bool ColumnSelectionMode
        {
            get { return _columnSelectionMode; }
            set { _columnSelectionMode = value; }
        }

        public Place Start
        {
            get { return _start; }
            set
            {
                _end = _start = value;
                _preferedPos = -1;
                OnSelectionChanged();
            }
        }

        public Place End
        {
            get { return _end; }
            set
            {
                _end = value;
                OnSelectionChanged();
            }
        }

        public virtual string Text
        {
            get
            {
                if (ColumnSelectionMode)
                {
                    return TextColumnSelectionMode;
                }
                var fromLine = Math.Min(_end.Line, _start.Line);
                var toLine = Math.Max(_end.Line, _start.Line);
                var fromChar = FromX;
                var toChar = ToX;
                if (fromLine < 0)
                {
                    return null;
                }
                var sb = new StringBuilder();
                for (var y = fromLine; y <= toLine; y++)
                {
                    var fromX = y == fromLine ? fromChar : 0;
                    var toX = y == toLine ? Math.Min(TextBox[y].Count - 1, toChar - 1) : TextBox[y].Count - 1;
                    for (var x = fromX; x <= toX; x++)
                    {
                        sb.Append(TextBox[y][x].C);
                    }
                    if (y != toLine && fromLine != toLine)
                    {
                        sb.AppendLine();
                    }
                }
                return sb.ToString();
            }
        }

        public int Length
        {
            get
            {
                if (ColumnSelectionMode)
                {
                    return Length_ColumnSelectionMode(false);
                }
                var fromLine = Math.Min(_end.Line, _start.Line);
                var toLine = Math.Max(_end.Line, _start.Line);
                var cnt = 0;
                if (fromLine < 0)
                {
                    return 0;
                }
                for (var y = fromLine; y <= toLine; y++)
                {
                    var fromX = y == fromLine ? FromX : 0;
                    var toX = y == toLine ? Math.Min(TextBox[y].Count - 1, ToX - 1) : TextBox[y].Count - 1;
                    cnt += toX - fromX + 1;
                    if (y != toLine && fromLine != toLine)
                    {
                        cnt += Environment.NewLine.Length;
                    }
                }
                return cnt;
            }
        }

        public int TextLength
        {
            get { return ColumnSelectionMode ? Length_ColumnSelectionMode(true) : Length; }
        }

        public char CharAfterStart
        {
            get { return Start.Char >= TextBox[Start.Line].Count ? '\n' : TextBox[Start.Line][Start.Char].C; }
        }

        public char CharBeforeStart
        {
            get
            {
                if (Start.Char > TextBox[Start.Line].Count)
                {
                    return '\n';
                }
                return Start.Char <= 0 ? '\n' : TextBox[Start.Line][Start.Char - 1].C;
            }
        }

        internal int FromX
        {
            get
            {
                if (_end.Line < _start.Line)
                {
                    return _end.Char;
                }
                return _end.Line > _start.Line ? _start.Char : Math.Min(_end.Char, _start.Char);
            }
        }

        internal int ToX
        {
            get
            {
                if (_end.Line < _start.Line)
                {
                    return _start.Char;
                }
                return _end.Line > _start.Line ? _end.Char : Math.Max(_end.Char, _start.Char);
            }
        }

        public int FromLine
        {
            get { return Math.Min(Start.Line, End.Line); }
        }

        public int ToLine
        {
            get { return Math.Max(Start.Line, End.Line); }
        }

        public IEnumerable<Char> Chars
        {
            get
            {
                if (ColumnSelectionMode)
                {
                    foreach (var p in GetEnumerator_ColumnSelectionMode())
                    {
                        yield return TextBox[p];
                    }
                    yield break;
                }
                var fromLine = Math.Min(_end.Line, _start.Line);
                var toLine = Math.Max(_end.Line, _start.Line);
                var fromChar = FromX;
                var toChar = ToX;
                if (fromLine < 0)
                {
                    yield break;
                }
                for (var y = fromLine; y <= toLine; y++)
                {
                    var fromX = y == fromLine ? fromChar : 0;
                    var toX = y == toLine ? Math.Min(toChar - 1, TextBox[y].Count - 1) : TextBox[y].Count - 1;
                    var line = TextBox[y];
                    for (var x = fromX; x <= toX; x++)
                    {
                        yield return line[x];
                    }
                }
            }
        }

        public RangeRect Bounds
        {
            get
            {
                var minX = Math.Min(Start.Char, End.Char);
                var minY = Math.Min(Start.Line, End.Line);
                var maxX = Math.Max(Start.Char, End.Char);
                var maxY = Math.Max(Start.Line, End.Line);
                return new RangeRect(minY, minX, maxY, maxX);
            }
        }

        public bool ReadOnly
        {
            get
            {
                if (TextBox.ReadOnly)
                {
                    return true;
                }
                var readonlyStyle = TextBox.Styles.OfType<ReadOnlyStyle>().FirstOrDefault();
                if (readonlyStyle != null)
                {
                    var si = ToStyleIndex(TextBox.GetStyleIndex(readonlyStyle));
                    if (IsEmpty)
                    {
                        /* check previous and next chars */
                        var line = TextBox[_start.Line];
                        if (_columnSelectionMode)
                        {
                            foreach (var sr in GetSubRanges(false))
                            {
                                line = TextBox[sr._start.Line];
                                if (sr._start.Char >= line.Count || sr._start.Char <= 0)
                                {
                                    continue;
                                }
                                var left = line[sr._start.Char - 1];
                                var right = line[sr._start.Char];
                                if ((left.Style & si) != 0 && (right.Style & si) != 0)
                                {
                                    return true; /* we are between readonly chars */
                                }
                            }
                        }
                        else if (_start.Char < line.Count && _start.Char > 0)
                        {
                            var left = line[_start.Char - 1];
                            var right = line[_start.Char];
                            if ((left.Style & si) != 0 && (right.Style & si) != 0)
                            {
                                return true; /* *we are between readonly chars */
                            }
                        }
                    }
                    else
                    {
                        /* found char with ReadonlyStyle */
                        return Chars.Any(c => (c.Style & si) != 0);
                    }
                }
                return false;
            }

            set
            {
                /* find exists ReadOnlyStyle of style buffer */
                var readonlyStyle = TextBox.Styles.OfType<ReadOnlyStyle>().FirstOrDefault() ?? new ReadOnlyStyle();
                if (value)
                {
                    SetStyle(readonlyStyle);
                }
                else
                {
                    ClearStyle(readonlyStyle);
                }
            }
        }

        public Range(FastColoredTextBox textBox)
        {
            TextBox = textBox;
        }

        public Range(FastColoredTextBox textBox, int iStartChar, int iStartLine, int iEndChar, int iEndLine) : this(textBox)
        {
            _start = new Place(iStartChar, iStartLine);
            _end = new Place(iEndChar, iEndLine);
        }

        public Range(FastColoredTextBox textBox, Place start, Place end) : this(textBox)
        {
            _start = start;
            _end = end;
        }

        public Range(FastColoredTextBox textBox, int iLine) : this(textBox)
        {
            _start = new Place(0, iLine);
            _end = new Place(textBox[iLine].Count, iLine);
        }

        IEnumerator<Place> IEnumerable<Place>.GetEnumerator()
        {
            if (ColumnSelectionMode)
            {
                foreach (var p in GetEnumerator_ColumnSelectionMode())
                {
                    yield return p;
                }
                yield break;
            }
            var fromLine = Math.Min(_end.Line, _start.Line);
            var toLine = Math.Max(_end.Line, _start.Line);
            var fromChar = FromX;
            var toChar = ToX;
            if (fromLine < 0)
            {
                yield break;
            }
            for (var y = fromLine; y <= toLine; y++)
            {
                var fromX = y == fromLine ? fromChar : 0;
                var toX = y == toLine ? Math.Min(toChar - 1, TextBox[y].Count - 1) : TextBox[y].Count - 1;
                for (var x = fromX; x <= toX; x++)
                {
                    yield return new Place(x, y);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Place>).GetEnumerator();
        }

        public bool Contains(Place place)
        {
            if (place.Line < Math.Min(_start.Line, _end.Line))
            {
                return false;
            }
            if (place.Line > Math.Max(_start.Line, _end.Line))
            {
                return false;
            }
            var s = _start;
            var e = _end;
            /* normalize start and end */
            if (s.Line > e.Line || (s.Line == e.Line && s.Char > e.Char))
            {
                var temp = s;
                s = e;
                e = temp;
            }
            if (_columnSelectionMode)
            {
                if (place.Char < s.Char || place.Char > e.Char) return false;
            }
            else
            {
                if (place.Line == s.Line && place.Char < s.Char) return false;
                if (place.Line == e.Line && place.Char > e.Char) return false;
            }
            return true;
        }

        public virtual Range GetIntersectionWith(Range range)
        {
            if (ColumnSelectionMode)
            {
                return GetIntersectionWith_ColumnSelectionMode(range);
            }
            var r1 = Clone();
            var r2 = range.Clone();
            r1.Normalize();
            r2.Normalize();
            var newStart = r1.Start > r2.Start ? r1.Start : r2.Start;
            var newEnd = r1.End < r2.End ? r1.End : r2.End;
            return newEnd < newStart ? new Range(TextBox, _start, _start) : TextBox.GetRange(newStart, newEnd);
        }

        public Range GetUnionWith(Range range)
        {
            var r1 = Clone();
            var r2 = range.Clone();
            r1.Normalize();
            r2.Normalize();
            var newStart = r1.Start < r2.Start ? r1.Start : r2.Start;
            var newEnd = r1.End > r2.End ? r1.End : r2.End;
            return TextBox.GetRange(newStart, newEnd);
        }

        public void SelectAll()
        {
            ColumnSelectionMode = false;
            Start = new Place(0, 0);
            if (TextBox.LinesCount == 0)
            {
                Start = new Place(0, 0);
            }
            else
            {
                _end = new Place(0, 0);
                _start = new Place(TextBox[TextBox.LinesCount - 1].Count, TextBox.LinesCount - 1);
            }
            if (this == TextBox.Selection)
            {
                TextBox.Invalidate();
            }
        }

        internal void GetText(out string text, out List<Place> charIndexToPlace)
        {
            /* try get cached text */
            if (TextBox.TextVersion == _cachedTextVersion)
            {
                text = _cachedText;
                charIndexToPlace = _cachedCharIndexToPlace;
                return;
            }
            var fromLine = Math.Min(_end.Line, _start.Line);
            var toLine = Math.Max(_end.Line, _start.Line);
            var fromChar = FromX;
            var toChar = ToX;
            var sb = new StringBuilder((toLine - fromLine)*50);
            charIndexToPlace = new List<Place>(sb.Capacity);
            if (fromLine >= 0)
            {
                for (var y = fromLine; y <= toLine; y++)
                {
                    var fromX = y == fromLine ? fromChar : 0;
                    var toX = y == toLine ? Math.Min(toChar - 1, TextBox[y].Count - 1) : TextBox[y].Count - 1;
                    for (var x = fromX; x <= toX; x++)
                    {
                        sb.Append(TextBox[y][x].C);
                        charIndexToPlace.Add(new Place(x, y));
                    }
                    if (y == toLine || fromLine == toLine)
                    {
                        continue;
                    }
                    foreach (var c in Environment.NewLine)
                    {
                        sb.Append(c);
                        charIndexToPlace.Add(new Place(TextBox[y].Count /*???*/, y));
                    }
                }
            }
            text = sb.ToString();
            charIndexToPlace.Add(End > Start ? End : Start);
            /* caching */
            _cachedText = text;
            _cachedCharIndexToPlace = charIndexToPlace;
            _cachedTextVersion = TextBox.TextVersion;
        }

        public string GetCharsBeforeStart(int charsCount)
        {
            var pos = TextBox.PlaceToPosition(Start) - charsCount;
            if (pos < 0)
            {
                pos = 0;
            }
            return new Range(TextBox, TextBox.PositionToPlace(pos), Start).Text;
        }

        public string GetCharsAfterStart(int charsCount)
        {
            return GetCharsBeforeStart(-charsCount);
        }

        public Range Clone()
        {
            return (Range) MemberwiseClone();
        }

        public bool GoRight()
        {
            var prevStart = _start;
            GoRight(false);
            return prevStart != _start;
        }

        public virtual bool GoRightThroughFolded()
        {
            if (ColumnSelectionMode)
            {
                return GoRightThroughFolded_ColumnSelectionMode();
            }
            if (_start.Line >= TextBox.LinesCount - 1 && _start.Char >= TextBox[TextBox.LinesCount - 1].Count)
            {
                return false;
            }
            if (_start.Char < TextBox[_start.Line].Count)
            {
                _start.Offset(1, 0);
            }
            else
            {
                _start = new Place(0, _start.Line + 1);
            }
            _preferedPos = -1;
            _end = _start;
            OnSelectionChanged();
            return true;
        }

        public bool GoLeft()
        {
            ColumnSelectionMode = false;
            var prevStart = _start;
            GoLeft(false);
            return prevStart != _start;
        }

        public bool GoLeftThroughFolded()
        {
            ColumnSelectionMode = false;
            if (_start.Char == 0 && _start.Line == 0)
            {
                return false;
            }
            if (_start.Char > 0)
            {
                _start.Offset(-1, 0);
            }
            else
            {
                _start = new Place(TextBox[_start.Line - 1].Count, _start.Line - 1);
            }
            _preferedPos = -1;
            _end = _start;
            OnSelectionChanged();
            return true;
        }

        public void GoLeft(bool shift)
        {
            ColumnSelectionMode = false;
            if (!shift)
            {
                if (_start > _end)
                {
                    Start = End;
                    return;
                }
            }
            if (_start.Char != 0 || _start.Line != 0)
            {
                if (_start.Char > 0 && TextBox.LineInfos[_start.Line].VisibleState == VisibleState.Visible)
                {
                    _start.Offset(-1, 0);
                }
                else
                {
                    var i = TextBox.FindPrevVisibleLine(_start.Line);
                    if (i == _start.Line)
                    {
                        return;
                    }
                    _start = new Place(TextBox[i].Count, i);
                }
            }
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
            _preferedPos = -1;
        }

        public void GoRight(bool shift)
        {
            ColumnSelectionMode = false;
            if (!shift)
            {
                if (_start < _end)
                {
                    Start = End;
                    return;
                }
            }
            if (_start.Line < TextBox.LinesCount - 1 || _start.Char < TextBox[TextBox.LinesCount - 1].Count)
            {
                if (_start.Char < TextBox[_start.Line].Count && TextBox.LineInfos[_start.Line].VisibleState == VisibleState.Visible)
                {
                    _start.Offset(1, 0);
                }
                else
                {
                    var i = TextBox.FindNextVisibleLine(_start.Line);
                    if (i == _start.Line)
                    {
                        return;
                    }
                    _start = new Place(0, i);
                }
            }
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
            _preferedPos = -1;
        }

        internal void GoUp(bool shift)
        {
            ColumnSelectionMode = false;
            if (!shift)
            {
                if (_start.Line > _end.Line)
                {
                    Start = End;
                    return;
                }
            }
            if (_preferedPos < 0)
            {
                _preferedPos = _start.Char -
                              TextBox.LineInfos[_start.Line].GetWordWrapStringStartPosition(
                                  TextBox.LineInfos[_start.Line].GetWordWrapStringIndex(_start.Char));
            }
            var iWw = TextBox.LineInfos[_start.Line].GetWordWrapStringIndex(_start.Char);
            if (iWw == 0)
            {
                if (_start.Line <= 0)
                {
                    return;
                }
                var i = TextBox.FindPrevVisibleLine(_start.Line);
                if (i == _start.Line)
                {
                    return;
                }
                _start.Line = i;
                iWw = TextBox.LineInfos[_start.Line].WordWrapStringsCount;
            }
            if (iWw > 0)
            {
                var finish = TextBox.LineInfos[_start.Line].GetWordWrapStringFinishPosition(iWw - 1, TextBox[_start.Line]);
                _start.Char = TextBox.LineInfos[_start.Line].GetWordWrapStringStartPosition(iWw - 1) + _preferedPos;
                if (_start.Char > finish + 1)
                {
                    _start.Char = finish + 1;
                }
            }
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
        }

        internal void GoPageUp(bool shift)
        {
            ColumnSelectionMode = false;
            if (_preferedPos < 0)
            {
                _preferedPos = _start.Char -
                              TextBox.LineInfos[_start.Line].GetWordWrapStringStartPosition(
                                  TextBox.LineInfos[_start.Line].GetWordWrapStringIndex(_start.Char));
            }
            var pageHeight = TextBox.ClientRectangle.Height/TextBox.CharHeight - 1;
            for (var i = 0; i < pageHeight; i++)
            {
                var iWw = TextBox.LineInfos[_start.Line].GetWordWrapStringIndex(_start.Char);
                if (iWw == 0)
                {
                    if (_start.Line <= 0)
                    {
                        break;
                    }
                    /* pass hidden */
                    var newLine = TextBox.FindPrevVisibleLine(_start.Line);
                    if (newLine == _start.Line)
                    {
                        break;
                    }
                    _start.Line = newLine;
                    iWw = TextBox.LineInfos[_start.Line].WordWrapStringsCount;
                }
                if (iWw <= 0)
                {
                    continue;
                }
                var finish = TextBox.LineInfos[_start.Line].GetWordWrapStringFinishPosition(iWw - 1, TextBox[_start.Line]);
                _start.Char = TextBox.LineInfos[_start.Line].GetWordWrapStringStartPosition(iWw - 1) + _preferedPos;
                if (_start.Char > finish + 1)
                {
                    _start.Char = finish + 1;
                }
            }
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
        }

        internal void GoDown(bool shift)
        {
            ColumnSelectionMode = false;
            if (!shift)
            {
                if (_start.Line < _end.Line)
                {
                    Start = End;
                    return;
                }
            }
            if (_preferedPos < 0)
            { _preferedPos = _start.Char -
                              TextBox.LineInfos[_start.Line].GetWordWrapStringStartPosition(
                                  TextBox.LineInfos[_start.Line].GetWordWrapStringIndex(_start.Char));
            }
            var iWw = TextBox.LineInfos[_start.Line].GetWordWrapStringIndex(_start.Char);
            if (iWw >= TextBox.LineInfos[_start.Line].WordWrapStringsCount - 1)
            {
                if (_start.Line >= TextBox.LinesCount - 1)
                {
                    return;
                }
                /* pass hidden */
                var i = TextBox.FindNextVisibleLine(_start.Line);
                if (i == _start.Line)
                {
                    return;
                }
                _start.Line = i;
                iWw = -1;
            }
            if (iWw < TextBox.LineInfos[_start.Line].WordWrapStringsCount - 1)
            {
                var finish = TextBox.LineInfos[_start.Line].GetWordWrapStringFinishPosition(iWw + 1, TextBox[_start.Line]);
                _start.Char = TextBox.LineInfos[_start.Line].GetWordWrapStringStartPosition(iWw + 1) + _preferedPos;
                if (_start.Char > finish + 1)
                {
                    _start.Char = finish + 1;
                }
            }
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
        }

        internal void GoPageDown(bool shift)
        {
            ColumnSelectionMode = false;
            if (_preferedPos < 0)
            {
                _preferedPos = _start.Char -
                              TextBox.LineInfos[_start.Line].GetWordWrapStringStartPosition(
                                  TextBox.LineInfos[_start.Line].GetWordWrapStringIndex(_start.Char));
            }
            var pageHeight = TextBox.ClientRectangle.Height/TextBox.CharHeight - 1;
            for (var i = 0; i < pageHeight; i++)
            {
                var iWw = TextBox.LineInfos[_start.Line].GetWordWrapStringIndex(_start.Char);
                if (iWw >= TextBox.LineInfos[_start.Line].WordWrapStringsCount - 1)
                {
                    if (_start.Line >= TextBox.LinesCount - 1)
                    {
                        break;
                    }
                    /* pass hidden */
                    var newLine = TextBox.FindNextVisibleLine(_start.Line);
                    if (newLine == _start.Line)
                    {
                        break;
                    }
                    _start.Line = newLine;
                    iWw = -1;
                }
                if (iWw >= TextBox.LineInfos[_start.Line].WordWrapStringsCount - 1)
                {
                    continue;
                }
                var finish = TextBox.LineInfos[_start.Line].GetWordWrapStringFinishPosition(iWw + 1, TextBox[_start.Line]);
                _start.Char = TextBox.LineInfos[_start.Line].GetWordWrapStringStartPosition(iWw + 1) + _preferedPos;
                if (_start.Char > finish + 1)
                {
                    _start.Char = finish + 1;
                }
            }
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
        }

        internal void GoHome(bool shift)
        {
            ColumnSelectionMode = false;
            if (_start.Line < 0)
            {
                return;
            }
            if (TextBox.LineInfos[_start.Line].VisibleState != VisibleState.Visible)
            {
                return;
            }
            _start = new Place(0, _start.Line);
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
            _preferedPos = -1;
        }

        internal void GoEnd(bool shift)
        {
            ColumnSelectionMode = false;
            if (_start.Line < 0)
            {
                return;
            }
            if (TextBox.LineInfos[_start.Line].VisibleState != VisibleState.Visible)
            {
                return;
            }
            _start = new Place(TextBox[_start.Line].Count, _start.Line);
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
            _preferedPos = -1;
        }

        public void SetStyle(Style style)
        {
            /* search code for style */
            var code = TextBox.GetOrSetStyleLayerIndex(style);
            /* set code to chars */
            SetStyle(ToStyleIndex(code));
            TextBox.Invalidate();
        }

        public void SetStyle(Style style, string regexPattern)
        {
            /* search code for style */
            var layer = ToStyleIndex(TextBox.GetOrSetStyleLayerIndex(style));
            SetStyle(layer, regexPattern, RegexOptions.None);
        }

        public void SetStyle(Style style, Regex regex)
        {
            /* search code for style */
            var layer = ToStyleIndex(TextBox.GetOrSetStyleLayerIndex(style));
            SetStyle(layer, regex);
        }

        public void SetStyle(Style style, string regexPattern, RegexOptions options)
        {
            /* search code for style */
            var layer = ToStyleIndex(TextBox.GetOrSetStyleLayerIndex(style));
            SetStyle(layer, regexPattern, options);
        }

        public void SetStyle(StyleIndex styleLayer, string regexPattern, RegexOptions options)
        {
            if (Math.Abs(Start.Line - End.Line) > 1000)
            {
                options |= SyntaxHighlighter.RegexCompiledOption;
            }
            foreach (var range in GetRanges(regexPattern, options))
            {
                range.SetStyle(styleLayer);
            }
            TextBox.Invalidate();
        }

        public void SetStyle(StyleIndex styleLayer, Regex regex)
        {
            foreach (var range in GetRanges(regex))
            {
                range.SetStyle(styleLayer);
            }
            TextBox.Invalidate();
        }

        public void SetStyle(StyleIndex styleIndex)
        {
            /* set code to chars */
            var fromLine = Math.Min(End.Line, Start.Line);
            var toLine = Math.Max(End.Line, Start.Line);
            var fromChar = FromX;
            var toChar = ToX;
            if (fromLine < 0)
            {
                return;
            }
            for (var y = fromLine; y <= toLine; y++)
            {
                var fromX = y == fromLine ? fromChar : 0;
                var toX = y == toLine ? Math.Min(toChar - 1, TextBox[y].Count - 1) : TextBox[y].Count - 1;
                for (var x = fromX; x <= toX; x++)
                {
                    var c = TextBox[y][x];
                    c.Style |= styleIndex;
                    TextBox[y][x] = c;
                }
            }
        }

        public void SetFoldingMarkers(string startFoldingPattern, string finishFoldingPattern)
        {
            SetFoldingMarkers(startFoldingPattern, finishFoldingPattern, SyntaxHighlighter.RegexCompiledOption);
        }

        public void SetFoldingMarkers(string startFoldingPattern, string finishFoldingPattern, RegexOptions options)
        {
            if (startFoldingPattern == finishFoldingPattern)
            {
                SetFoldingMarkers(startFoldingPattern, options);
                return;
            }
            foreach (var range in GetRanges(startFoldingPattern, options))
            {
                TextBox[range.Start.Line].FoldingStartMarker = startFoldingPattern;
            }
            foreach (var range in GetRanges(finishFoldingPattern, options))
            {
                TextBox[range.Start.Line].FoldingEndMarker = startFoldingPattern;
            }
            TextBox.Invalidate();
        }

        public void SetFoldingMarkers(string foldingPattern, RegexOptions options)
        {
            foreach (var range in GetRanges(foldingPattern, options))
            {
                if (range.Start.Line > 0)
                    TextBox[range.Start.Line - 1].FoldingEndMarker = foldingPattern;
                TextBox[range.Start.Line].FoldingStartMarker = foldingPattern;
            }
            TextBox.Invalidate();
        }

        public IEnumerable<Range> GetRanges(string regexPattern)
        {
            return GetRanges(regexPattern, RegexOptions.None);
        }

        public IEnumerable<Range> GetRanges(string regexPattern, RegexOptions options)
        {
            /* get text */
            string text;
            List<Place> charIndexToPlace;
            GetText(out text, out charIndexToPlace);
            /* create regex */
            var regex = new Regex(regexPattern, options);
            foreach (Match m in regex.Matches(text))
            {
                var r = new Range(TextBox);
                /* try get 'range' group, otherwise use group 0 */
                var group = m.Groups["range"];
                if (!group.Success)
                {
                    group = m.Groups[0];
                }
                r.Start = charIndexToPlace[group.Index];
                r.End = charIndexToPlace[group.Index + group.Length];
                yield return r;
            }
        }

        public IEnumerable<Range> GetRangesByLines(string regexPattern, RegexOptions options)
        {
            var regex = new Regex(regexPattern, options);
            return GetRangesByLines(regex);
        }

        public IEnumerable<Range> GetRangesByLines(Regex regex)
        {
            Normalize();
            var fts = TextBox.TextSource as FileTextSource; //<----!!!! ugly
            /* enumaerate lines */
            for (var iLine = Start.Line; iLine <= End.Line; iLine++)
            {
                var isLineLoaded = fts == null || fts.IsLineLoaded(iLine);
                var r = new Range(TextBox, new Place(0, iLine), new Place(TextBox[iLine].Count, iLine));
                if (iLine == Start.Line || iLine == End.Line)
                {
                    r = r.GetIntersectionWith(this);
                }
                foreach (var foundRange in r.GetRanges(regex))
                {
                    yield return foundRange;
                }
                if (!isLineLoaded)
                {
                    fts.UnloadLine(iLine);
                }
            }
        }

        public IEnumerable<Range> GetRangesByLinesReversed(string regexPattern, RegexOptions options)
        {
            Normalize();
            /* create regex */
            var regex = new Regex(regexPattern, options);
            var fts = TextBox.TextSource as FileTextSource; //<----!!!! ugly
            /* enumaerate lines */
            for (var iLine = End.Line; iLine >= Start.Line; iLine--)
            {
                var isLineLoaded = fts == null || fts.IsLineLoaded(iLine);
                var r = new Range(TextBox, new Place(0, iLine), new Place(TextBox[iLine].Count, iLine));
                if (iLine == Start.Line || iLine == End.Line)
                {
                    r = r.GetIntersectionWith(this);
                }
                var list = r.GetRanges(regex).ToList();
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    yield return list[i];
                }
                if (!isLineLoaded)
                {
                    fts.UnloadLine(iLine);
                }
            }
        }

        public IEnumerable<Range> GetRanges(Regex regex)
        {
            /* get text */
            string text;
            List<Place> charIndexToPlace;
            GetText(out text, out charIndexToPlace);
            foreach (Match m in regex.Matches(text))
            {
                var r = new Range(TextBox);
                /* try get 'range' group, otherwise use group 0 */
                var group = m.Groups["range"];
                if (!group.Success)
                {
                    group = m.Groups[0];
                }
                r.Start = charIndexToPlace[group.Index];
                r.End = charIndexToPlace[group.Index + group.Length];
                yield return r;
            }
        }

        public void ClearStyle(params Style[] styles)
        {
            try
            {
                ClearStyle(TextBox.GetStyleIndexMask(styles));
            }
            catch
            {
                Debug.Assert(true);
            }
        }

        public void ClearStyle(StyleIndex styleIndex)
        {
            /* set code to chars */
            var fromLine = Math.Min(End.Line, Start.Line);
            var toLine = Math.Max(End.Line, Start.Line);
            var fromChar = FromX;
            var toChar = ToX;
            if (fromLine < 0)
            {
                return;
            }
            for (var y = fromLine; y <= toLine; y++)
            {
                var fromX = y == fromLine ? fromChar : 0;
                var toX = y == toLine ? Math.Min(toChar - 1, TextBox[y].Count - 1) : TextBox[y].Count - 1;
                for (var x = fromX; x <= toX; x++)
                {
                    var c = TextBox[y][x];
                    c.Style &= ~styleIndex;
                    TextBox[y][x] = c;
                }
            }
            TextBox.Invalidate();
        }

        public void ClearFoldingMarkers()
        {
            /* set code to chars */
            var fromLine = Math.Min(End.Line, Start.Line);
            var toLine = Math.Max(End.Line, Start.Line);
            if (fromLine < 0)
            {
                return;
            }
            for (var y = fromLine; y <= toLine; y++)
            {
                TextBox[y].ClearFoldingMarkers();
            }
            TextBox.Invalidate();
        }

        private void OnSelectionChanged()
        {
            /* clear cache */
            _cachedTextVersion = -1;
            _cachedText = null;
            _cachedCharIndexToPlace = null;
            if (TextBox.Selection != this)
            {
                return;
            }
            if (_updating == 0)
            {
                TextBox.OnSelectionChanged();
            }
        }

        public void BeginUpdate()
        {
            _updating++;
        }

        public void EndUpdate()
        {
            _updating--;
            if (_updating == 0)
            {
                OnSelectionChanged();
            }
        }

        public override string ToString()
        {
            return string.Format("Start: {0} End: {1}", Start, End);
        }

        public void Normalize()
        {
            if (Start > End)
            {
                Inverse();
            }
        }

        public void Inverse()
        {
            var temp = _start;
            _start = _end;
            _end = temp;
        }

        public void Expand()
        {
            Normalize();
            _start = new Place(0, _start.Line);
            _end = new Place(TextBox.GetLineLength(_end.Line), _end.Line);
        }

        public Range GetFragment(string allowedSymbolsPattern)
        {
            return GetFragment(allowedSymbolsPattern, RegexOptions.None);
        }

        public Range GetFragment(Style style, bool allowLineBreaks)
        {
            var mask = TextBox.GetStyleIndexMask(new[] {style});
            var r = new Range(TextBox) {Start = Start};
            /* go left, check style */
            while (r.GoLeftThroughFolded())
            {
                if (!allowLineBreaks && r.CharAfterStart == '\n')
                {
                    break;
                }
                if (r.Start.Char >= TextBox.GetLineLength(r.Start.Line) || (TextBox[r.Start].Style & mask) != 0)
                {
                    continue;
                }
                r.GoRightThroughFolded();
                break;
            }
            var startFragment = r.Start;
            r.Start = Start;
            /* go right, check style */
            do
            {
                if (!allowLineBreaks && r.CharAfterStart == '\n')
                {
                    break;
                }
                if (r.Start.Char >= TextBox.GetLineLength(r.Start.Line))
                {
                    continue;
                }
                if ((TextBox[r.Start].Style & mask) == 0)
                {
                    break;
                }
            } 
            while (r.GoRightThroughFolded());
            var endFragment = r.Start;
            return new Range(TextBox, startFragment, endFragment);
        }

        public Range GetFragment(string allowedSymbolsPattern, RegexOptions options)
        {
            var r = new Range(TextBox) {Start = Start};
            var regex = new Regex(allowedSymbolsPattern, options);
            /* go left, check symbols */
            while (r.GoLeftThroughFolded())
            {
                if (regex.IsMatch(r.CharAfterStart.ToString()))
                {
                    continue;
                }
                r.GoRightThroughFolded();
                break;
            }
            var startFragment = r.Start;
            r.Start = Start;
            /* go right, check symbols */
            do
            {
                if (!regex.IsMatch(r.CharAfterStart.ToString()))
                {
                    break;
                }
            } 
            while (r.GoRightThroughFolded());
            var endFragment = r.Start;
            return new Range(TextBox, startFragment, endFragment);
        }

        private static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private static bool IsSpaceChar(char c)
        {
            return c == ' ' || c == '\t';
        }

        public void GoWordLeft(bool shift)
        {
            ColumnSelectionMode = false;
            if (!shift && _start > _end)
            {
                Start = End;
                return;
            }
            var range = Clone(); /* to OnSelectionChanged disable */
            var wasSpace = false;
            while (IsSpaceChar(range.CharBeforeStart))
            {
                wasSpace = true;
                range.GoLeft(shift);
            }
            var wasIdentifier = false;
            while (IsIdentifierChar(range.CharBeforeStart))
            {
                wasIdentifier = true;
                range.GoLeft(shift);
            }
            if (!wasIdentifier && (!wasSpace || range.CharBeforeStart != '\n'))
            {
                range.GoLeft(shift);
            }
            Start = range.Start;
            End = range.End;
            if (TextBox.LineInfos[Start.Line].VisibleState != VisibleState.Visible)
            {
                GoRight(shift);
            }
        }

        public void GoWordRight(bool shift, bool goToStartOfNextWord = false)
        {
            ColumnSelectionMode = false;
            if (!shift && _start < _end)
            {
                Start = End;
                return;
            }
            var range = Clone(); //to OnSelectionChanged disable
            var wasNewLine = false;
            if (range.CharAfterStart == '\n')
            {
                range.GoRight(shift);
                wasNewLine = true;
            }
            var wasSpace = false;
            while (IsSpaceChar(range.CharAfterStart))
            {
                wasSpace = true;
                range.GoRight(shift);
            }
            if (!((wasSpace || wasNewLine) && goToStartOfNextWord))
            {
                var wasIdentifier = false;
                while (IsIdentifierChar(range.CharAfterStart))
                {
                    wasIdentifier = true;
                    range.GoRight(shift);
                }
                if (!wasIdentifier)
                {
                    range.GoRight(shift);
                }
                if (goToStartOfNextWord)
                {
                    while (IsSpaceChar(range.CharAfterStart))
                    {
                        range.GoRight(shift);
                    }
                }
            }
            Start = range.Start;
            End = range.End;
            if (TextBox.LineInfos[Start.Line].VisibleState != VisibleState.Visible)
            {
                GoLeft(shift);
            }
        }

        internal void GoFirst(bool shift)
        {
            ColumnSelectionMode = false;
            _start = new Place(0, 0);
            if (TextBox.LineInfos[Start.Line].VisibleState != VisibleState.Visible)
            {
                TextBox.ExpandBlock(Start.Line);
            }
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
        }

        internal void GoLast(bool shift)
        {
            ColumnSelectionMode = false;
            _start = new Place(TextBox[TextBox.LinesCount - 1].Count, TextBox.LinesCount - 1);
            if (TextBox.LineInfos[Start.Line].VisibleState != VisibleState.Visible)
            {
                TextBox.ExpandBlock(Start.Line);
            }
            if (!shift)
            {
                _end = _start;
            }
            OnSelectionChanged();
        }

        public static StyleIndex ToStyleIndex(int i)
        {
            return (StyleIndex) (1 << i);
        }

        public IEnumerable<Range> GetSubRanges(bool includeEmpty)
        {
            if (!ColumnSelectionMode)
            {
                yield return this;
                yield break;
            }
            var rect = Bounds;
            for (var y = rect.StartLine; y <= rect.EndLine; y++)
            {
                if (rect.StartChar > TextBox[y].Count && !includeEmpty)
                {
                    continue;
                }
                var r = new Range(TextBox, rect.StartChar, y, Math.Min(rect.EndChar, TextBox[y].Count), y);
                yield return r;
            }
        }

        public bool IsReadOnlyLeftChar()
        {
            if (TextBox.ReadOnly)
            {
                return true;
            }
            var r = Clone();
            r.Normalize();
            if (r._start.Char == 0)
            {
                return false;
            }
            if (ColumnSelectionMode)
            {
                r.GoLeft_ColumnSelectionMode();
            }
            else
            {
                r.GoLeft(true);
            }
            return r.ReadOnly;
        }

        public bool IsReadOnlyRightChar()
        {
            if (TextBox.ReadOnly)
            {
                return true;
            }
            var r = Clone();
            r.Normalize();
            if (r._end.Char >= TextBox[_end.Line].Count)
            {
                return false;
            }
            if (ColumnSelectionMode)
            {
                r.GoRight_ColumnSelectionMode();
            }
            else
            {
                r.GoRight(true);
            }
            return r.ReadOnly;
        }

        public IEnumerable<Place> GetPlacesCyclic(Place startPlace, bool backward = false)
        {
            if (backward)
            {
                var r = new Range(TextBox, startPlace, startPlace);
                while (r.GoLeft() && r._start >= Start)
                {
                    if (r.Start.Char < TextBox[r.Start.Line].Count)
                    {
                        yield return r.Start;
                    }
                }
                r = new Range(TextBox, End, End);
                while (r.GoLeft() && r._start >= startPlace)
                {
                    if (r.Start.Char < TextBox[r.Start.Line].Count)
                    {
                        yield return r.Start;
                    }
                }
            }
            else
            {
                var r = new Range(TextBox, startPlace, startPlace);
                if (startPlace < End)
                {
                    do
                    {
                        if (r.Start.Char < TextBox[r.Start.Line].Count)
                        {
                            yield return r.Start;
                        }
                    }
                    while (r.GoRight());
                }
                r = new Range(TextBox, Start, Start);
                if (r.Start < startPlace)
                {
                    do
                    {
                        if (r.Start.Char < TextBox[r.Start.Line].Count)
                        {
                            yield return r.Start;
                        }
                    } 
                    while (r.GoRight() && r.Start < startPlace);
                }
            }
        }

        private string TextColumnSelectionMode
        {
            get
            {
                var sb = new StringBuilder();
                var bounds = Bounds;
                if (bounds.StartLine < 0)
                {
                    return "";
                }
                for (var y = bounds.StartLine; y <= bounds.EndLine; y++)
                {
                    for (var x = bounds.StartChar; x < bounds.EndChar; x++)
                    {
                        if (x < TextBox[y].Count)
                        {
                            sb.Append(TextBox[y][x].C);
                        }
                    }
                    if (bounds.EndLine != bounds.StartLine && y != bounds.EndLine)
                    {
                        sb.AppendLine();
                    }
                }
                return sb.ToString();
            }
        }

        private Range GetIntersectionWith_ColumnSelectionMode(Range range)
        {
            if (range.Start.Line != range.End.Line)
            {
                return new Range(TextBox, Start, Start);
            }
            var rect = Bounds;
            if (range.Start.Line < rect.StartLine || range.Start.Line > rect.EndLine)
            {
                return new Range(TextBox, Start, Start);
            }
            return
                new Range(TextBox, rect.StartChar, range.Start.Line, rect.EndChar, range.Start.Line).GetIntersectionWith(
                    range);
        }

        private bool GoRightThroughFolded_ColumnSelectionMode()
        {
            var boundes = Bounds;
            var endOfLines = true;
            for (var iLine = boundes.StartLine; iLine <= boundes.EndLine; iLine++)
            {
                if (boundes.EndChar >= TextBox[iLine].Count)
                {
                    continue;
                }
                endOfLines = false;
                break;
            }
            if (endOfLines)
            {
                return false;
            }
            var start = Start;
            var end = End;
            start.Offset(1, 0);
            end.Offset(1, 0);
            BeginUpdate();
            Start = start;
            End = end;
            EndUpdate();
            return true;
        }

        private IEnumerable<Place> GetEnumerator_ColumnSelectionMode()
        {
            var bounds = Bounds;
            if (bounds.StartLine < 0) yield break;
            for (var y = bounds.StartLine; y <= bounds.EndLine; y++)
            {
                for (var x = bounds.StartChar; x < bounds.EndChar; x++)
                {
                    if (x < TextBox[y].Count)
                    {
                        yield return new Place(x, y);
                    }
                }
            }
        }

        private int Length_ColumnSelectionMode(bool withNewLines)
        {
            var bounds = Bounds;
            if (bounds.StartLine < 0) return 0;
            var cnt = 0;
            for (var y = bounds.StartLine; y <= bounds.EndLine; y++)
            {
                for (var x = bounds.StartChar; x < bounds.EndChar; x++)
                {
                    if (x < TextBox[y].Count)
                    {
                        cnt++;
                    }
                }
                if (withNewLines && bounds.EndLine != bounds.StartLine && y != bounds.EndLine)
                {
                    cnt += Environment.NewLine.Length;
                }
            }
            return cnt;
        }

        internal void GoDown_ColumnSelectionMode()
        {
            var iLine = TextBox.FindNextVisibleLine(End.Line);
            End = new Place(End.Char, iLine);
        }

        internal void GoUp_ColumnSelectionMode()
        {
            var iLine = TextBox.FindPrevVisibleLine(End.Line);
            End = new Place(End.Char, iLine);
        }

        internal void GoRight_ColumnSelectionMode()
        {
            End = new Place(End.Char + 1, End.Line);
        }

        internal void GoLeft_ColumnSelectionMode()
        {
            if (End.Char > 0)
            {
                End = new Place(End.Char - 1, End.Line);
            }
        }
    }
}