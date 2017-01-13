/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ircCore.Controls.SwitchTreeView.Base;

namespace ircCore.Controls.SwitchTreeView
{
    public class IrcSwitchTreeView : TreeViewEx
    {
        /* IRC Treeview Control - Main Control Class
           By: Jason James Newland
           ©2010-2012 - KangaSoft Software
           All Rights Reserved
         */
        private Dictionary<TreeNode, int> FlashNodes { get; set; }
        private readonly Timer _tmrFlash;
        private bool _flashInvert;

        public IrcSwitchTreeView()
        {
            FlashNodes = new Dictionary<TreeNode, int>();
            _tmrFlash = new Timer
                            {
                                Interval = 500,
                                Enabled = true
                            };
            _tmrFlash.Tick += TmrFlashTick;
        }

        public void AddServerTree(string text, IntPtr handle)
        {
            var i = Nodes.Add(handle.ToString(), text, 0, 0);
            SelectedNode = i;
            i.Expand();
        }

        public void RemoveServerTree(IntPtr handle)
        {
            Nodes.RemoveByKey(handle.ToString());
        }

        public void RenameServerTree(string newText, IntPtr handle)
        {
            var i = Nodes.Find(handle.ToString(), true);
            if (i.Length > 0)
            {
                i[0].Text = newText;
            }
        }

        public void AddWindow(string text, IntPtr handle, IntPtr rootHandle, int iconIndex)
        {
            var i = Nodes.Find(rootHandle.ToString(), true);
            TreeNode[] j = null;
            switch (iconIndex)
            {
                case 1:
                    /* Channels */
                    if (i[0].Nodes.IndexOfKey("CHANNELS") == -1)
                    {
                        i[0].Nodes.Insert(0, new TreeNodeEx("CHANNELS", "Channels", 1, 1));
                    }
                    j = i[0].Nodes.Find("CHANNELS", true);
                    break;
                case 2:
                    /* Queries */
                    if (i[0].Nodes.IndexOfKey("QUERIES") == -1)
                    {
                        i[0].Nodes.Insert(1, new TreeNodeEx("QUERIES", "Queries", 2, 2));
                    }
                    j = i[0].Nodes.Find("QUERIES", true);
                    break;
                case 3:
                    /* Chats */
                    if (i[0].Nodes.IndexOfKey("CHATS") == -1)
                    {
                        i[0].Nodes.Insert(2, new TreeNodeEx("CHATS", "Chats", 3, 3));
                    }
                    j = i[0].Nodes.Find("CHATS", true);
                    break;
                case 4:
                    /* Sends */
                    if (i[0].Nodes.IndexOfKey("SENDS") == -1)
                    {
                        i[0].Nodes.Insert(3, new TreeNodeEx("SENDS", "Sends", 4, 4));
                    }
                    j = i[0].Nodes.Find("SENDS", true);
                    break;
                case 5:
                    /* Gets */
                    if (i[0].Nodes.IndexOfKey("GETS") == -1)
                    {
                        i[0].Nodes.Insert(4, new TreeNodeEx("GETS", "Gets", 5, 5));
                    }
                    j = i[0].Nodes.Find("GETS", true);
                    break;
            }
            if (j != null && j.Length > 0)
            {
                SelectedNode = ((TreeNodeEx) j[0]).AddSorted(handle.ToString(), text, iconIndex, iconIndex);
                UpdateChildWinCount(rootHandle, iconIndex);
                j[0].Expand();
            }
            else
            {
                /* Just insert it at the bottom */
                SelectedNode = i[0].Nodes.Add(handle.ToString(), text, iconIndex, iconIndex);
            }
            i[0].Expand();
        }

        public void RemoveWindow(IntPtr handle, IntPtr rootHandle, int iconIndex)
        {
            var i = Nodes.Find(rootHandle.ToString(), true);
            TreeNode[] j = null;

            switch (iconIndex)
            {
                case 1:
                    /* Channels */
                    j = i[0].Nodes.Find("CHANNELS", true);
                    break;
                case 2:
                    /* Queries */
                    j = i[0].Nodes.Find("QUERIES", true);
                    break;
                case 3:
                    /* Chats */
                    j = i[0].Nodes.Find("CHATS", true);
                    break;
                case 4:
                    /* Sends */
                    j = i[0].Nodes.Find("SENDS", true);
                    break;
                case 5:
                    /* Gets */
                    j = i[0].Nodes.Find("GETS", true);
                    break;
            }
            if (j != null && j.Length > 0)
            {
                var t = j[0].Nodes.Find(handle.ToString(), true);
                j[0].Nodes.Remove(t[0]);
                /* Remove "folder" for current window */
                if (j[0].Nodes.Count == 0)
                {
                    j[0].Nodes.Remove(j[0]);
                }
                UpdateChildWinCount(rootHandle, iconIndex);
                j[0].Expand();
            }
            else
            {
                /* Find it in current tree */
                j = i[0].Nodes.Find(handle.ToString(), true);
                if (j.Length > 0)
                {
                    i[0].Nodes.Remove(j[0]);
                }
            }
            i[0].Expand();
        }

        public void RenameWindow(string newText, IntPtr handle, IntPtr rootHandle, int iconIndex)
        {
            /* Used mainly to rename query windows, etc */
            var i = Nodes.Find(rootHandle.ToString(), true);
            TreeNode[] j = null;
            switch (iconIndex)
            {
                case 1:
                    /* Channels */
                    j = i[0].Nodes.Find("CHANNELS", true);
                    break;
                case 2:
                    /* Queries */
                    j = i[0].Nodes.Find("QUERIES", true);
                    break;
                case 3:
                    /* Chats */
                    j = i[0].Nodes.Find("CHATS", true);
                    break;
                case 4:
                    /* Sends */
                    j = i[0].Nodes.Find("SENDS", true);
                    break;
                case 5:
                    /* Gets */
                    j = i[0].Nodes.Find("GETS", true);
                    break;
            }
            if (j == null)
            {
                return;
            }
            var k = j[0].Nodes.Find(handle.ToString(), true);
            k[0].Text = newText;
        }

        public void SelectWindow(IntPtr handle, IntPtr rootHandle, int iconIndex)
        {
            /* Selects the window's node in the treeview */
            var i = Nodes.Find(rootHandle.ToString(), true);
            TreeNode[] j = null;
            if (i.Length <= 0)
            {
                return;
            }
            if (iconIndex == 0)
            {
                /* Check it isn't already selected */
                if (ReferenceEquals(SelectedNode, i[0]))
                {
                    return;
                }
                SelectedNode = i[0];
                return;
            }
            switch (iconIndex)
            {
                case 1:
                    /* Channels */
                    j = i[0].Nodes.Find("CHANNELS", true);
                    break;
                case 2:
                    /* Queries */
                    j = i[0].Nodes.Find("QUERIES", true);
                    break;
                case 3:
                    /* DCC chats */
                    j = i[0].Nodes.Find("CHATS", true);
                    break;
                case 4:
                    /* DCC sends */
                    j = i[0].Nodes.Find("SENDS", true);
                    break;
                case 5:
                    /* DCC gets */
                    j = i[0].Nodes.Find("GETS", true);
                    break;
            }
            TreeNode[] k;
            if (j != null && j.Length > 0)
            {
                k = j[0].Nodes.Find(handle.ToString(), true);
            }
            else
            {
                /* We search for it in the root node */
                k = i[0].Nodes.Find(handle.ToString(), true);
            }
            if (k.Length <= 0)
            {
                return;
            }
            if (FlashNodes.ContainsKey(k[0]))
            {
                FlashNodes.Remove(k[0]);
                k[0].ImageIndex = iconIndex;
            }
            /* Check it isn't already selected */
            if (ReferenceEquals(SelectedNode, k[0]))
            {
                return;
            }
            SelectedNode = k[0];
        }

        //have to change this below for colors - not sure if I will even be using events list any more
        //public void SetWindowNodeColor(IntPtr handle, IntPtr rootHandle, int iconIndex, int eventColor)
        //{
        //    /* Changes window's node color in the treeview */
        //    var i = Nodes.Find(rootHandle.ToString(), true);
        //    if (i.Length <= 0) { return; }
        //    TreeNode[] j = null;
        //    var tmpCol = Color.Empty;
        //    switch (eventColor)
        //    {
        //        case 0:
        //            /* Default */
        //            tmpCol = IrcColors.IrcColor[IrcColors.IrcEvent[22]];
        //            break;
        //        case 1:
        //            /* Message */
        //            tmpCol = IrcColors.IrcColor[IrcColors.IrcEvent[24]];
        //            break;
        //        case 2:
        //            /* Event */
        //            tmpCol = IrcColors.IrcColor[IrcColors.IrcEvent[23]];
        //            break;
        //    }
        //    switch (iconIndex)
        //    {
        //        case 0:
        //            i[0].ForeColor = tmpCol;
        //            return;
        //        case 1:
        //            /* Channels */
        //            j = i[0].Nodes.Find("CHANNELS", true);
        //            break;
        //        case 2:
        //            /* Queries */
        //            j = i[0].Nodes.Find("QUERIES", true);
        //            break;
        //        case 3:
        //            /* Chats */
        //            j = i[0].Nodes.Find("CHATS", true);
        //            break;
        //        case 4:
        //            /* Sends */
        //            j = i[0].Nodes.Find("SENDS", true);
        //            break;
        //        case 5:
        //            /* Gets */
        //            j = i[0].Nodes.Find("GETS", true);
        //            break;
        //    }
        //    TreeNode[] k;
        //    if (j != null && j.Length > 0)
        //    {
        //        k = j[0].Nodes.Find(handle.ToString(), true);
        //    }
        //    else
        //    {
        //        /* Search at the bottom of the root node */
        //        k = i[0].Nodes.Find(handle.ToString(), true);
        //    }
        //    if (k.Length == 0) { return; }
        //    if (eventColor > 0)
        //    {
        //        if (j != null && (j[0].Name == "QUERIES" || j[0].Name == "CHATS"))
        //        {
        //            if (eventColor == 1 && !FlashNodes.ContainsKey(k[0]))
        //            {
        //                FlashNodes.Add(k[0], iconIndex);
        //            }
        //        }
        //        if (k[0].ForeColor != IrcColors.IrcColor[IrcColors.IrcEvent[24]])
        //        {
        //            /* Message color takes precidence, unless its 0 (reset) */
        //            k[0].ForeColor = tmpCol;
        //            return;
        //        }
        //        return;
        //    }
        //    k[0].ForeColor = tmpCol;
        //    if (!FlashNodes.ContainsKey(k[0])) { return; }
        //    FlashNodes.Remove(k[0]);
        //    k[0].ImageIndex = iconIndex;
        //}

        private void UpdateChildWinCount(IntPtr rootHandle, int iconIndex)
        {
            TreeNode[] j = null;
            string sCaption = null;

            var i = Nodes.Find(rootHandle.ToString(), true);
            switch (iconIndex)
            {
                case 1:
                    /* Channels */
                    if (i[0].Nodes.IndexOfKey("CHANNELS") == -1)
                    {
                        return;
                    }
                    j = i[0].Nodes.Find("CHANNELS", true);
                    sCaption = "Channels";
                    break;
                case 2:
                    /* Queries */
                    if (i[0].Nodes.IndexOfKey("QUERIES") == -1)
                    {
                        return;
                    }
                    j = i[0].Nodes.Find("QUERIES", true);
                    sCaption = "Queries";
                    break;
                case 3:
                    /* Chats */
                    if (i[0].Nodes.IndexOfKey("CHATS") == -1)
                    {
                        return;
                    }
                    j = i[0].Nodes.Find("CHATS", true);
                    sCaption = "Chats";
                    break;
                case 4:
                    /* Sends */
                    if (i[0].Nodes.IndexOfKey("SENDS") == -1)
                    {
                        return;
                    }
                    j = i[0].Nodes.Find("SENDS", true);
                    sCaption = "Sends";
                    break;
                case 5:
                    /* Gets */
                    if (i[0].Nodes.IndexOfKey("GETS") == -1)
                    {
                        return;
                    }
                    j = i[0].Nodes.Find("GETS", true);
                    sCaption = "Gets";
                    break;
            }
            if (j != null)
            {
                j[0].Text = string.Format("{0} ({1})", sCaption, j[0].Nodes.Count);
            }
        }

        private void TmrFlashTick(object sender, EventArgs e)
        {
            if (FlashNodes.Count == 0)
            {
                return;
            }
            _flashInvert = !_flashInvert;
            foreach (var node in FlashNodes)
            {
                node.Key.ImageIndex = _flashInvert ? node.Value : 7;
            }
        }
    }
}
