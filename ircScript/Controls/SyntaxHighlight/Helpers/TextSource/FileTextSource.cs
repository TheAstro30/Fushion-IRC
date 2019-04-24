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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ircScript.Controls.SyntaxHighlight.Helpers.Lines;

namespace ircScript.Controls.SyntaxHighlight.Helpers.TextSource
{
    public class FileTextSource : TextSource
    {
        private readonly Timer _timer = new Timer();
        private Encoding _fileEncoding;
        private FileStream _fs;
        private List<int> _sourceFileLinePositions = new List<int>();

        public event EventHandler<LineNeededEventArgs> LineNeeded;
        public event EventHandler<LinePushedEventArgs> LinePushed;

        public FileTextSource(FastColoredTextBox currentTextBox) : base(currentTextBox)
        {
            _timer.Interval = 10000;
            _timer.Tick += TimerTick;
            _timer.Enabled = true;
            SaveEol = Environment.NewLine;
        }

        public string SaveEol { get; set; }

        public override Line this[int i]
        {
            get
            {
                if (Lines[i] != null)
                {
                    return Lines[i];
                }
                LoadLineFromSourceFile(i);
                return Lines[i];
            }
            set { throw new NotImplementedException(); }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _timer.Enabled = false;
            try
            {
                UnloadUnusedLines();
            }
            finally
            {
                _timer.Enabled = true;
            }
        }

        private void UnloadUnusedLines()
        {
            const int margin = 2000;
            var iFinishVisibleLine = CurrentTextBox.VisibleRange.End.Line;
            for (var i = 0; i < Count; i++)
            {
                if (Lines[i] == null || Lines[i].IsChanged || Math.Abs(i - iFinishVisibleLine) <= margin)
                {
                    continue;
                }
                Lines[i] = null;
            }
        }

        public void OpenFile(string fileName, Encoding enc)
        {
            Clear();
            if (_fs != null)
            {
                _fs.Dispose();
            }
            SaveEol = Environment.NewLine;
            /* Read lines of file */
            _fs = new FileStream(fileName, FileMode.Open);
            var length = _fs.Length;
            /* Read signature */
            enc = DefineEncoding(enc, _fs);
            /* First line */
            _sourceFileLinePositions.Add((int) _fs.Position);
            Lines.Add(null);
            /* Other lines */
            _sourceFileLinePositions.Capacity = (int) (length/7 + 1000);
            var prev = 0;
            var prevPos = 0;
            var br = new BinaryReader(_fs, enc);
            while (_fs.Position < length)
            {
                prevPos = (int) _fs.Position;
                var b = br.ReadChar();
                if (b == 10) /* \n */
                {
                    _sourceFileLinePositions.Add((int) _fs.Position);
                    Lines.Add(null);
                }
                else if (prev == 13) /* \r (Mac format) */
                {
                    _sourceFileLinePositions.Add(prevPos);
                    Lines.Add(null);
                    SaveEol = "\r";
                }
                prev = b;
            }
            if (prev == 13)
            {
                _sourceFileLinePositions.Add(prevPos);
                Lines.Add(null);
            }
            if (length > 2000000)
            {
                GC.Collect();
            }
            var temp = new Line[100];
            var c = Lines.Count;
            Lines.AddRange(temp);
            Lines.TrimExcess();
            Lines.RemoveRange(c, temp.Length);
            var temp2 = new int[100];
            c = Lines.Count;
            _sourceFileLinePositions.AddRange(temp2);
            _sourceFileLinePositions.TrimExcess();
            _sourceFileLinePositions.RemoveRange(c, temp.Length);
            _fileEncoding = enc;
            OnLineInserted(0, Count);
            /* Load first lines for calc width of the text */
            var linesCount = Math.Min(Lines.Count, CurrentTextBox.ClientRectangle.Height/CurrentTextBox.CharHeight);
            for (var i = 0; i < linesCount; i++)
            {
                LoadLineFromSourceFile(i);
            }
            NeedRecalc(new TextChangedEventArgs(0, linesCount - 1));
            if (CurrentTextBox.WordWrap)
            {
                OnRecalcWordWrap(new TextChangedEventArgs(0, linesCount - 1));
            }
        }

        private static Encoding DefineEncoding(Encoding enc, FileStream fs)
        {
            var bytesPerSignature = 0;
            var signature = new byte[4];
            var c = fs.Read(signature, 0, 4);
            if (signature[0] == 0xFF && signature[1] == 0xFE && signature[2] == 0x00 && signature[3] == 0x00 && c >= 4)
            {
                enc = Encoding.UTF32; //UTF32 LE
                bytesPerSignature = 4;
            }
            else if (signature[0] == 0x00 && signature[1] == 0x00 && signature[2] == 0xFE && signature[3] == 0xFF)
            {
                enc = new UTF32Encoding(true, true); //UTF32 BE
                bytesPerSignature = 4;
            }
            else if (signature[0] == 0xEF && signature[1] == 0xBB && signature[2] == 0xBF)
            {
                enc = Encoding.UTF8; //UTF8
                bytesPerSignature = 3;
            }
            else if (signature[0] == 0xFE && signature[1] == 0xFF)
            {
                enc = Encoding.BigEndianUnicode; //UTF16 BE
                bytesPerSignature = 2;
            }
            else if (signature[0] == 0xFF && signature[1] == 0xFE)
            {
                enc = Encoding.Unicode; //UTF16 LE
                bytesPerSignature = 2;
            }
            fs.Seek(bytesPerSignature, SeekOrigin.Begin);
            return enc;
        }

        public void CloseFile()
        {
            if (_fs != null)
            {
                try
                {
                    _fs.Dispose();
                }
                catch
                {
                    Debug.Assert(true);
                }
            }
            _fs = null;
        }

        public override void SaveToFile(string fileName, Encoding enc)
        {
            var newLinePos = new List<int>(Count);
            /* Create temp file */
            var dir = Path.GetDirectoryName(fileName);
            if (dir != null)
            {
                var tempFileName = Path.Combine(dir, Path.GetFileNameWithoutExtension(fileName) + ".tmp");
                var sr = new StreamReader(_fs, _fileEncoding);
                using (var tempFs = new FileStream(tempFileName, FileMode.Create))
                using (var sw = new StreamWriter(tempFs, enc))
                {
                    sw.Flush();
                    for (var i = 0; i < Count; i++)
                    {
                        newLinePos.Add((int) tempFs.Length);
                        var sourceLine = ReadLine(sr, i); /* Read line from source file */
                        string line;
                        var lineIsChanged = Lines[i] != null && Lines[i].IsChanged;
                        line = lineIsChanged ? Lines[i].Text : sourceLine;
                        /* Call event handler */
                        if (LinePushed != null)
                        {
                            var args = new LinePushedEventArgs(sourceLine, i, lineIsChanged ? line : null);
                            LinePushed(this, args);
                            if (args.SavedText != null)
                            {
                                line = args.SavedText;
                            }
                        }
                        /* Save line to file */
                        sw.Write(line);
                        if (i < Count - 1)
                        {
                            sw.Write(SaveEol);
                        }
                        sw.Flush();
                    }
                }
                /* Clear lines buffer */
                for (int i = 0; i < Count; i++)
                {
                    Lines[i] = null;
                }
                /* Deattach from source file */
                sr.Dispose();
                _fs.Dispose();
                /* Delete target file */
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                /* Rename temp file */
                File.Move(tempFileName, fileName);
            }
            /* Binding to new file */
            _sourceFileLinePositions = newLinePos;
            _fs = new FileStream(fileName, FileMode.Open);
            _fileEncoding = enc;
        }

        private string ReadLine(StreamReader sr, int i)
        {
            var filePos = _sourceFileLinePositions[i];
            if (filePos < 0)
            {
                return "";
            }
            _fs.Seek(filePos, SeekOrigin.Begin);
            sr.DiscardBufferedData();
            var line = sr.ReadLine();
            return line;
        }

        public override void ClearIsChanged()
        {
            foreach (var line in Lines.Where(line => line != null))
            {
                line.IsChanged = false;
            }
        }

        private void LoadLineFromSourceFile(int i)
        {
            var line = CreateLine();
            _fs.Seek(_sourceFileLinePositions[i], SeekOrigin.Begin);
            var sr = new StreamReader(_fs, _fileEncoding);
            var s = sr.ReadLine() ?? "";
            /* Call event handler */
            if (LineNeeded != null)
            {
                var args = new LineNeededEventArgs(s, i);
                LineNeeded(this, args);
                s = args.DisplayedLineText;
                if (s == null)
                {
                    return;
                }
            }
            foreach (var c in s)
            {
                line.Add(new Char(c));
            }
            Lines[i] = line;
            if (CurrentTextBox.WordWrap)
            {
                OnRecalcWordWrap(new TextChangedEventArgs(i, i));
            }
        }

        public override void InsertLine(int index, Line line)
        {
            _sourceFileLinePositions.Insert(index, -1);
            base.InsertLine(index, line);
        }

        public override void RemoveLine(int index, int count)
        {
            _sourceFileLinePositions.RemoveRange(index, count);
            base.RemoveLine(index, count);
        }

        public override int GetLineLength(int i)
        {
            return Lines[i] == null ? 0 : Lines[i].Count;
        }

        public override bool LineHasFoldingStartMarker(int iLine)
        {
            return Lines[iLine] != null && !string.IsNullOrEmpty(Lines[iLine].FoldingStartMarker);
        }

        public override bool LineHasFoldingEndMarker(int iLine)
        {
            return Lines[iLine] != null && !string.IsNullOrEmpty(Lines[iLine].FoldingEndMarker);
        }

        public override void Dispose()
        {
            if (_fs != null)
            {
                _fs.Dispose();
            }
            _timer.Dispose();
        }

        internal void UnloadLine(int iLine)
        {
            if (Lines[iLine] != null && !Lines[iLine].IsChanged)
            {
                Lines[iLine] = null;
            }
        }
    }
}