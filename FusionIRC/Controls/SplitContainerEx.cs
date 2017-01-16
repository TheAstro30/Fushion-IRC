/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Reflection;
using System.Windows.Forms;

namespace FusionIRC.Controls
{
    public class SplitContainerEx : SplitContainer
    {
        /* A little hack class for the SplitContainer control to reduce flicker */
        public SplitContainerEx()
        {
            /* Must use reflection to set double buffering on the child panel controls */
            var mi = typeof(Control).GetMethod("SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);
            var args = new object[] { ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true };
            mi.Invoke(Panel1, args);
            mi.Invoke(Panel2, args);
        }
    }
}