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
namespace ircScript.Controls.SyntaxHighlight.Helpers
{
    public class PrintDialogSettings
    {
        public bool ShowPageSetupDialog { get; set; }
        public bool ShowPrintDialog { get; set; }
        public bool ShowPrintPreviewDialog { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string Header { get; set; }
        public bool IncludeLineNumbers { get; set; }

        public PrintDialogSettings()
        {
            ShowPrintPreviewDialog = true;
            Title = "";
            Footer = "";
            Header = "";
        }
    }
}
