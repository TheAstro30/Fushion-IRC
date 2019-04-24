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

namespace ircScript.Controls.SyntaxHighlight.Helpers.Lines
{
    public enum VisibleState : byte
    {
        Visible, StartOfHiddenBlock, Hidden
    }

    public enum IndentMarker
    {
        None,
        Increased,
        Decreased
    }

    public struct LineInfo
    {
        private List<int> _cutOffPositions;
        /* Y coordinate of line on screen */
        internal int StartY;
        internal int BottomPadding;
        /* indent of secondary wordwrap strings (in chars) */
        internal int WordWrapIndent;

        public VisibleState VisibleState;

        public LineInfo(int startY)
        {
            _cutOffPositions = null;
            VisibleState = VisibleState.Visible;
            StartY = startY;
            BottomPadding = 0;
            WordWrapIndent = 0;
        }

        public List<int> CutOffPositions
        {
            get { return _cutOffPositions ?? (_cutOffPositions = new List<int>()); }
        }

        public int WordWrapStringsCount
        {
            get
            {
                switch (VisibleState)
                {
                    case VisibleState.Visible:
                        return _cutOffPositions == null ? 1 : _cutOffPositions.Count + 1;

                    case VisibleState.Hidden:
                        return 0;

                    case VisibleState.StartOfHiddenBlock:
                        return 1;
                }
                return 0;
            }
        }

        internal int GetWordWrapStringStartPosition(int iWordWrapLine)
        {
            return iWordWrapLine == 0 ? 0 : CutOffPositions[iWordWrapLine - 1];
        }

        internal int GetWordWrapStringFinishPosition(int iWordWrapLine, Line line)
        {
            if (WordWrapStringsCount <= 0)
                return 0;
            return iWordWrapLine == WordWrapStringsCount - 1 ? line.Count - 1 : CutOffPositions[iWordWrapLine] - 1;
        }

        public int GetWordWrapStringIndex(int iChar)
        {
            if (_cutOffPositions == null || _cutOffPositions.Count == 0)
            {
                return 0;
            }
            for (var i = 0; i < _cutOffPositions.Count; i++)
            {
                if (_cutOffPositions[i] >/*>=*/ iChar)
                    return i;
            }
            return _cutOffPositions.Count;
        }
    }
}
