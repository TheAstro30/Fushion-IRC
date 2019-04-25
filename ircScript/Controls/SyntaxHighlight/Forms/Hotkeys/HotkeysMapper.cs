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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using KEYS = System.Windows.Forms.Keys;

namespace ircScript.Controls.SyntaxHighlight.Forms.Hotkeys
{
    public enum FctbAction
    {
        None,
        AutocompleteMenu,
        AutoIndentChars,
        BookmarkLine,
        ClearHints,
        ClearWordLeft,
        ClearWordRight,
        CommentSelected,
        Copy,
        Cut,
        DeleteCharRight,
        FindChar,
        FindDialog,
        FindNext,
        GoDown,
        GoDownWithSelection,
        GoDownColumnSelectionMode,
        GoEnd,
        GoEndWithSelection,
        GoFirstLine,
        GoFirstLineWithSelection,
        GoHome,
        GoHomeWithSelection,
        GoLastLine,
        GoLastLineWithSelection,
        GoLeft,
        GoLeftWithSelection,
        GoLeftColumnSelectionMode,
        GoPageDown,
        GoPageDownWithSelection,
        GoPageUp,
        GoPageUpWithSelection,
        GoRight,
        GoRightWithSelection,
        GoRightColumnSelectionMode,
        GoToDialog,
        GoNextBookmark,
        GoPrevBookmark,
        GoUp,
        GoUpWithSelection,
        GoUpColumnSelectionMode,
        GoWordLeft,
        GoWordLeftWithSelection,
        GoWordRight,
        GoWordRightWithSelection,
        IndentIncrease,
        IndentDecrease,
        LowerCase,
        MacroExecute,
        MacroRecord,
        MoveSelectedLinesDown,
        MoveSelectedLinesUp,
        NavigateBackward,
        NavigateForward,
        Paste,
        Redo,
        ReplaceDialog,
        ReplaceMode,
        ScrollDown,
        ScrollUp,
        SelectAll,
        UnbookmarkLine,
        Undo,
        UpperCase,
        ZoomIn,
        ZoomNormal,
        ZoomOut,
        CustomAction1,
        CustomAction2,
        CustomAction3,
        CustomAction4,
        CustomAction5,
        CustomAction6,
        CustomAction7,
        CustomAction8,
        CustomAction9,
        CustomAction10,
        CustomAction11,
        CustomAction12,
        CustomAction13,
        CustomAction14,
        CustomAction15,
        CustomAction16,
        CustomAction17,
        CustomAction18,
        CustomAction19,
        CustomAction20
    }

    public class HotkeysMapping : SortedDictionary<Keys, FctbAction>
    {
        public virtual void InitDefault()
        {
            this[KEYS.Control | KEYS.G] = FctbAction.GoToDialog;
            this[KEYS.Control | KEYS.F] = FctbAction.FindDialog;
            this[KEYS.Alt | KEYS.F] = FctbAction.FindChar;
            this[KEYS.F3] = FctbAction.FindNext;
            this[KEYS.Control | KEYS.H] = FctbAction.ReplaceDialog;
            this[KEYS.Control | KEYS.C] = FctbAction.Copy;
            this[KEYS.Control | KEYS.Shift | KEYS.C] = FctbAction.CommentSelected;
            this[KEYS.Control | KEYS.X] = FctbAction.Cut;
            this[KEYS.Control | KEYS.V] = FctbAction.Paste;
            this[KEYS.Control | KEYS.A] = FctbAction.SelectAll;
            this[KEYS.Control | KEYS.Z] = FctbAction.Undo;
            this[KEYS.Control | KEYS.R] = FctbAction.Redo;
            this[KEYS.Control | KEYS.U] = FctbAction.UpperCase;
            this[KEYS.Shift | KEYS.Control | KEYS.U] = FctbAction.LowerCase;
            this[KEYS.Control | KEYS.OemMinus] = FctbAction.NavigateBackward;
            this[KEYS.Control | KEYS.Shift | KEYS.OemMinus] = FctbAction.NavigateForward;
            this[KEYS.Control | KEYS.B] = FctbAction.BookmarkLine;
            this[KEYS.Control | KEYS.Shift | KEYS.B] = FctbAction.UnbookmarkLine;
            this[KEYS.Control | KEYS.N] = FctbAction.GoNextBookmark;
            this[KEYS.Control | KEYS.Shift | KEYS.N] = FctbAction.GoPrevBookmark;
            this[KEYS.Alt | KEYS.Back] = FctbAction.Undo;
            this[KEYS.Control | KEYS.Back] = FctbAction.ClearWordLeft;
            this[KEYS.Insert] = FctbAction.ReplaceMode;
            this[KEYS.Control | KEYS.Insert] = FctbAction.Copy;
            this[KEYS.Shift | KEYS.Insert] = FctbAction.Paste;
            this[KEYS.Delete] = FctbAction.DeleteCharRight;
            this[KEYS.Control | KEYS.Delete] = FctbAction.ClearWordRight;
            this[KEYS.Shift | KEYS.Delete] = FctbAction.Cut;
            this[KEYS.Left] = FctbAction.GoLeft;
            this[KEYS.Shift | KEYS.Left] = FctbAction.GoLeftWithSelection;
            this[KEYS.Control | KEYS.Left] = FctbAction.GoWordLeft;
            this[KEYS.Control | KEYS.Shift | KEYS.Left] = FctbAction.GoWordLeftWithSelection;
            this[KEYS.Alt | KEYS.Shift | KEYS.Left] = FctbAction.GoLeftColumnSelectionMode;
            this[KEYS.Right] = FctbAction.GoRight;
            this[KEYS.Shift | KEYS.Right] = FctbAction.GoRightWithSelection;
            this[KEYS.Control | KEYS.Right] = FctbAction.GoWordRight;
            this[KEYS.Control | KEYS.Shift | KEYS.Right] = FctbAction.GoWordRightWithSelection;
            this[KEYS.Alt | KEYS.Shift | KEYS.Right] = FctbAction.GoRightColumnSelectionMode;
            this[KEYS.Up] = FctbAction.GoUp;
            this[KEYS.Shift | KEYS.Up] = FctbAction.GoUpWithSelection;
            this[KEYS.Alt | KEYS.Shift | KEYS.Up] = FctbAction.GoUpColumnSelectionMode;
            this[KEYS.Alt | KEYS.Up] = FctbAction.MoveSelectedLinesUp;
            this[KEYS.Control | KEYS.Up] = FctbAction.ScrollUp;
            this[KEYS.Down] = FctbAction.GoDown;
            this[KEYS.Shift | KEYS.Down] = FctbAction.GoDownWithSelection;
            this[KEYS.Alt | KEYS.Shift | KEYS.Down] = FctbAction.GoDownColumnSelectionMode;
            this[KEYS.Alt | KEYS.Down] = FctbAction.MoveSelectedLinesDown;
            this[KEYS.Control | KEYS.Down] = FctbAction.ScrollDown;
            this[KEYS.PageUp] = FctbAction.GoPageUp;
            this[KEYS.Shift | KEYS.PageUp] = FctbAction.GoPageUpWithSelection;
            this[KEYS.PageDown] = FctbAction.GoPageDown;
            this[KEYS.Shift | KEYS.PageDown] = FctbAction.GoPageDownWithSelection;
            this[KEYS.Home] = FctbAction.GoHome;
            this[KEYS.Shift | KEYS.Home] = FctbAction.GoHomeWithSelection;
            this[KEYS.Control | KEYS.Home] = FctbAction.GoFirstLine;
            this[KEYS.Control | KEYS.Shift | KEYS.Home] = FctbAction.GoFirstLineWithSelection;
            this[KEYS.End] = FctbAction.GoEnd;
            this[KEYS.Shift | KEYS.End] = FctbAction.GoEndWithSelection;
            this[KEYS.Control | KEYS.End] = FctbAction.GoLastLine;
            this[KEYS.Control | KEYS.Shift | KEYS.End] = FctbAction.GoLastLineWithSelection;
            this[KEYS.Escape] = FctbAction.ClearHints;
            this[KEYS.Control | KEYS.M] = FctbAction.MacroRecord;
            this[KEYS.Control | KEYS.E] = FctbAction.MacroExecute;
            this[KEYS.Control | KEYS.Space] = FctbAction.AutocompleteMenu;
            this[KEYS.Tab] = FctbAction.IndentIncrease;
            this[KEYS.Shift | KEYS.Tab] = FctbAction.IndentDecrease;
            this[KEYS.Control | KEYS.Subtract] = FctbAction.ZoomOut;
            this[KEYS.Control | KEYS.Add] = FctbAction.ZoomIn;
            this[KEYS.Control | KEYS.D0] = FctbAction.ZoomNormal;
            this[KEYS.Control | KEYS.I] = FctbAction.AutoIndentChars;
        }

        public override string ToString()
        {
            var cult = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            var kc = new KeysConverter();
            foreach (var pair in this)
            {
                sb.AppendFormat("{0}={1}, ", kc.ConvertToString(pair.Key), pair.Value);
            }
            if (sb.Length > 1)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            Thread.CurrentThread.CurrentUICulture = cult;
            return sb.ToString();
        }

        public static HotkeysMapping Parse(string s)
        {
            var result = new HotkeysMapping();
            result.Clear();
            var cult = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            var kc = new KeysConverter();            
            foreach (var p in s.Split(','))
            {
                var pp = p.Split('=');
                var convertFromString = kc.ConvertFromString(pp[0].Trim());
                if (convertFromString == null)
                {
                    continue;
                }
                var k = (Keys)convertFromString;
                var a = (FctbAction)Enum.Parse(typeof(FctbAction), pp[1].Trim());
                result[k] = a;
            }
            Thread.CurrentThread.CurrentUICulture = cult;
            return result;
        }
    }
}
