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
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ircScript.Controls.SyntaxHighlight.Forms.Hotkeys
{
    /// <summary>
    /// Dictionary of shortcuts for FCTB
    /// </summary>
    public class HotkeysMapping : SortedDictionary<Keys, FctbAction>
    {
        public virtual void InitDefault()
        {
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G] = FctbAction.GoToDialog;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F] = FctbAction.FindDialog;
            this[System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F] = FctbAction.FindChar;
            this[System.Windows.Forms.Keys.F3] = FctbAction.FindNext;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H] = FctbAction.ReplaceDialog;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C] = FctbAction.Copy;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.C] = FctbAction.CommentSelected;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X] = FctbAction.Cut;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V] = FctbAction.Paste;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A] = FctbAction.SelectAll;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z] = FctbAction.Undo;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R] = FctbAction.Redo;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U] = FctbAction.UpperCase;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U] = FctbAction.LowerCase;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus] = FctbAction.NavigateBackward;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.OemMinus] = FctbAction.NavigateForward;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B] = FctbAction.BookmarkLine;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.B] = FctbAction.UnbookmarkLine;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N] = FctbAction.GoNextBookmark;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.N] = FctbAction.GoPrevBookmark;
            this[System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Back] = FctbAction.Undo;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Back] = FctbAction.ClearWordLeft;
            this[System.Windows.Forms.Keys.Insert] = FctbAction.ReplaceMode;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert] = FctbAction.Copy;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Insert] = FctbAction.Paste;
            this[System.Windows.Forms.Keys.Delete] = FctbAction.DeleteCharRight;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete] = FctbAction.ClearWordRight;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Delete] = FctbAction.Cut;
            this[System.Windows.Forms.Keys.Left] = FctbAction.GoLeft;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Left] = FctbAction.GoLeftWithSelection;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left] = FctbAction.GoWordLeft;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Left] = FctbAction.GoWordLeftWithSelection;
            this[System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Left] = FctbAction.GoLeft_ColumnSelectionMode;
            this[System.Windows.Forms.Keys.Right] = FctbAction.GoRight;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Right] = FctbAction.GoRightWithSelection;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right] = FctbAction.GoWordRight;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Right] = FctbAction.GoWordRightWithSelection;
            this[System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Right] = FctbAction.GoRight_ColumnSelectionMode;
            this[System.Windows.Forms.Keys.Up] = FctbAction.GoUp;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Up] = FctbAction.GoUpWithSelection;
            this[System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Up] = FctbAction.GoUp_ColumnSelectionMode;
            this[System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Up] = FctbAction.MoveSelectedLinesUp;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up] = FctbAction.ScrollUp;
            this[System.Windows.Forms.Keys.Down] = FctbAction.GoDown;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Down] = FctbAction.GoDownWithSelection;
            this[System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Down] = FctbAction.GoDown_ColumnSelectionMode;
            this[System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Down] = FctbAction.MoveSelectedLinesDown;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down] = FctbAction.ScrollDown;
            this[System.Windows.Forms.Keys.PageUp] = FctbAction.GoPageUp;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.PageUp] = FctbAction.GoPageUpWithSelection;
            this[System.Windows.Forms.Keys.PageDown] = FctbAction.GoPageDown;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.PageDown] = FctbAction.GoPageDownWithSelection;
            this[System.Windows.Forms.Keys.Home] = FctbAction.GoHome;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Home] = FctbAction.GoHomeWithSelection;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home] = FctbAction.GoFirstLine;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Home] = FctbAction.GoFirstLineWithSelection;
            this[System.Windows.Forms.Keys.End] = FctbAction.GoEnd;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.End] = FctbAction.GoEndWithSelection;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.End] = FctbAction.GoLastLine;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.End] = FctbAction.GoLastLineWithSelection;
            this[System.Windows.Forms.Keys.Escape] = FctbAction.ClearHints;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M] = FctbAction.MacroRecord;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E] = FctbAction.MacroExecute;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Space] = FctbAction.AutocompleteMenu;
            this[System.Windows.Forms.Keys.Tab] = FctbAction.IndentIncrease;
            this[System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Tab] = FctbAction.IndentDecrease;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Subtract] = FctbAction.ZoomOut;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Add] = FctbAction.ZoomIn;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D0] = FctbAction.ZoomNormal;
            this[System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I] = FctbAction.AutoIndentChars;   
        }

        public override string ToString()
        {
            var cult = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            StringBuilder sb = new StringBuilder();
            var kc = new KeysConverter();
            foreach (var pair in this)
            {
                sb.AppendFormat("{0}={1}, ", kc.ConvertToString(pair.Key), pair.Value);
            }

            if (sb.Length > 1)
                sb.Remove(sb.Length - 2, 2);
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
                var k = (Keys)kc.ConvertFromString(pp[0].Trim());
                var a = (FctbAction)Enum.Parse(typeof(FctbAction), pp[1].Trim());
                result[k] = a;
            }

            Thread.CurrentThread.CurrentUICulture = cult;

            return result;
        }
    }

    /// <summary>
    /// Actions for shortcuts
    /// </summary>
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
        GoDown_ColumnSelectionMode,
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
        GoLeft_ColumnSelectionMode,
        GoPageDown,
        GoPageDownWithSelection,
        GoPageUp,
        GoPageUpWithSelection,
        GoRight,
        GoRightWithSelection,
        GoRight_ColumnSelectionMode,
        GoToDialog,
        GoNextBookmark,
        GoPrevBookmark,
        GoUp,
        GoUpWithSelection,
        GoUp_ColumnSelectionMode,
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

    internal class HotkeysEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((provider != null) && (((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService))) != null))
            {
                var form = new HotkeysEditorForm(HotkeysMapping.Parse(value as string));

                if (form.ShowDialog() == DialogResult.OK)
                    value = form.GetHotkeys().ToString();
            }
            return value;
        }
    }
}
