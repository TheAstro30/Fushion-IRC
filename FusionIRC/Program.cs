/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Windows.Forms;
using FusionIRC.Forms;
using FusionIRC.Forms.Misc;

namespace FusionIRC
{
    static class Program
    {
        [STAThread]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmClientWindow());
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (var f = new FrmException((Exception)e.ExceptionObject))
            {
                f.ShowDialog();
            }
        }
    }
}
