/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System.Windows.Forms;
using FusionIRC.Controls.SwitchView.Base;
using FusionIRC.Forms.Child;
using FusionIRC.Helpers;
using ircCore.Settings.Theming;

namespace FusionIRC.Controls.SwitchView
{
    public class WindowTreeView : TreeViewEx
    {
        /* Treeview SwitchView window - simplified */
        public void AddWindow(FrmChildWindow window)
        {
            switch (window.WindowType)
            {
                case ChildWindowType.Console:
                    /* Add a root node */
                    var node = Nodes.Add(window.Handle.ToString(), window.ToString(), 0, 0);
                    node.Tag = window;
                    SelectedNode = node;
                    node.Expand();
                    window.DisplayNode = node;
                    break;

                case ChildWindowType.Channel:
                case ChildWindowType.Private:
                case ChildWindowType.DccChat:
                    var c = WindowManager.GetConsoleWindow(window.Client);
                    if (c != null)
                    {
                        var rootNode = Nodes.Find(c.Handle.ToString(), true);
                        TreeNode[] subNodes = null;
                        var iconIndex = (int) window.WindowType;
                        switch (window.WindowType)
                        {
                            case ChildWindowType.Channel:
                                if (rootNode[0].Nodes.IndexOfKey("CHANNELS") == -1)
                                {
                                    rootNode[0].Nodes.Insert(0, new TreeNodeEx("CHANNELS", "Channels", iconIndex, iconIndex));
                                }
                                subNodes = rootNode[0].Nodes.Find("CHANNELS", true);
                                break;

                            case ChildWindowType.Private:
                                if (rootNode[0].Nodes.IndexOfKey("QUERIES") == -1)
                                {
                                    rootNode[0].Nodes.Insert(1, new TreeNodeEx("QUERIES", "Queries", iconIndex, iconIndex));
                                }
                                subNodes = rootNode[0].Nodes.Find("QUERIES", true);
                                break;

                            case ChildWindowType.DccChat:
                                if (rootNode[0].Nodes.IndexOfKey("CHATS") == -1)
                                {
                                    rootNode[0].Nodes.Insert(2, new TreeNodeEx("CHATS", "Chats", iconIndex, iconIndex));
                                }
                                subNodes = rootNode[0].Nodes.Find("CHATS", true);
                                break;
                        }
                        if (subNodes != null && subNodes.Length > 0)
                        {
                            SelectedNode = ((TreeNodeEx) subNodes[0]).AddSorted(window.Handle.ToString(), window.ToString(), iconIndex, iconIndex);
                            SelectedNode.Tag = window;
                            window.DisplayNode = SelectedNode;                            
                            subNodes[0].Expand();
                            window.DisplayNodeRoot = subNodes[0];
                        }
                        else
                        {
                            /* Just insert it at the bottom */
                            SelectedNode = rootNode[0].Nodes.Add(window.Handle.ToString(), window.ToString(), iconIndex,
                                                                 iconIndex);
                        }
                        rootNode[0].Expand();
                    }
                    break;
            }
        }

        public void RemoveWindow(FrmChildWindow window)
        {
            /* Simplified by keeping a reference to the main tree node plus the "folder" (sub node it's in) */
            Nodes.Remove(window.DisplayNode);
            if (window.DisplayNodeRoot != null && window.DisplayNodeRoot.Nodes.Count == 0)
            {
                Nodes.Remove(window.DisplayNodeRoot);
            }
        }
    }
}
