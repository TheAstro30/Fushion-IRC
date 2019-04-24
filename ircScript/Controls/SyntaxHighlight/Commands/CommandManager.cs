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
using System;
using ircScript.Controls.SyntaxHighlight.Helpers;
using ircScript.Controls.SyntaxHighlight.Helpers.TextSource;

namespace ircScript.Controls.SyntaxHighlight.Commands
{
    public class CommandManager
    {
        private int _autoUndoCommands;

        private const int MaxHistoryLength = 200;
        private readonly LimitedStack<UndoableCommand> _history;
        private readonly Stack<UndoableCommand> _redoStack = new Stack<UndoableCommand>();

        public TextSource TextSource{ get; private set; }
        public bool UndoRedoStackIsEnabled { get; set; }

        protected int DisabledCommands;

        public event EventHandler RedoCompleted = delegate { };

        public bool UndoEnabled
        {
            get
            {
                return _history.Count > 0;
            }
        }

        public bool RedoEnabled
        {
            get
            {
                return _redoStack.Count > 0;
            }
        }

        public CommandManager(TextSource ts)
        {
            _history = new LimitedStack<UndoableCommand>(MaxHistoryLength);
            TextSource = ts;
            UndoRedoStackIsEnabled = true;
        }

        public virtual void ExecuteCommand(Command cmd)
        {
            if (DisabledCommands > 0)
            {
                return;
            }
            /* Multirange ? */
            if (cmd.TextSource.CurrentTextBox.Selection.ColumnSelectionMode)
            {
                if (cmd is UndoableCommand)
                {
                    /* Make wrapper */
                    cmd = new MultiRangeCommand((UndoableCommand) cmd);
                }
            }
            if (cmd is UndoableCommand)
            {
                /* If range is ColumnRange, then create wrapper */
                (cmd as UndoableCommand).AutoUndo = _autoUndoCommands > 0;
                _history.Push(cmd as UndoableCommand);
            }
            try
            {
                cmd.Execute();
            }
            catch (ArgumentOutOfRangeException)
            {
                /* OnTextChanging cancels enter of the text */
                if (cmd is UndoableCommand)
                {
                    _history.Pop();
                }
            }
            if (!UndoRedoStackIsEnabled)
            {
                ClearHistory();
            }
            _redoStack.Clear();
            TextSource.CurrentTextBox.OnUndoRedoStateChanged();
        }

        public void Undo()
        {
            if (_history.Count > 0)
            {
                var cmd = _history.Pop();
                BeginDisableCommands(); /* Prevent text changing into handlers */
                try
                {
                    cmd.Undo();
                }
                finally
                {
                    EndDisableCommands();
                }
                _redoStack.Push(cmd);
            }
            /* Undo next autoUndo command */
            if (_history.Count > 0)
            {
                if (_history.Peek().AutoUndo)
                {
                    Undo();
                }
            }
            TextSource.CurrentTextBox.OnUndoRedoStateChanged();
        }
        
        private void EndDisableCommands()
        {
            DisabledCommands--;
        }

        private void BeginDisableCommands()
        {
            DisabledCommands++;
        }

        public void EndAutoUndoCommands()
        {
            _autoUndoCommands--;
            if (_autoUndoCommands != 0)
            {
                return;
            }
            if (_history.Count > 0)
            {
                _history.Peek().AutoUndo = false;
            }
        }

        public void BeginAutoUndoCommands()
        {
            _autoUndoCommands++;
        }

        internal void ClearHistory()
        {
            _history.Clear();
            _redoStack.Clear();
            TextSource.CurrentTextBox.OnUndoRedoStateChanged();
        }

        internal void Redo()
        {
            if (_redoStack.Count == 0)
            {
                return;
            }
            UndoableCommand cmd;
            BeginDisableCommands();//prevent text changing into handlers
            try
            {
                cmd = _redoStack.Pop();
                if (TextSource.CurrentTextBox.Selection.ColumnSelectionMode)
                {
                    TextSource.CurrentTextBox.Selection.ColumnSelectionMode = false;
                }
                TextSource.CurrentTextBox.Selection.Start = cmd.Sel.Start;
                TextSource.CurrentTextBox.Selection.End = cmd.Sel.End;
                cmd.Execute();
                _history.Push(cmd);
            }
            finally
            {
                EndDisableCommands();
            }
            /* Call event */
            RedoCompleted(this, EventArgs.Empty);
            /* Redo command after autoUndoable command */
            if (cmd.AutoUndo)
            {
                Redo();
            }
            TextSource.CurrentTextBox.OnUndoRedoStateChanged();
        }
    }
}