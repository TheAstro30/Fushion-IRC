/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Windows.Forms;
using FusionIRC.Controls.SwitchView.Base;
using FusionIRC.Forms.Child;
using FusionIRC.Helpers;
using ircClient;
using ircCore.Settings.Theming;
using ircCore.Users;
using ircCore.Utils;

namespace FusionIRC.Controls.SwitchView
{
    public class WindowTreeView : TreeViewEx
    {
        /* Treeview SwitchView window - simplified */
        private const int InitialToolTipDelay = 500;
        private const int MaxToolTipDisplayTime = 2000;

        private readonly ToolTip _toolTip;
        private readonly Timer _timer;
        private TreeNode _toolTipNode;

        /* Constructor */
        public WindowTreeView()
        {
            _toolTip = new ToolTip {IsBalloon = true};
            _timer = new Timer();
            _timer.Tick += ToolTipTimer;

            NodeMouseHover += OnNodeMouseHover;
        }

        /* Overrides */
        protected override void OnMouseLeave(EventArgs e)
        {
            _timer.Enabled = false;
            _toolTip.Hide(this);
            _toolTipNode = null;
            base.OnMouseLeave(e);
        }

        /* Public methods */
        public void AddWindow(FrmChildWindow window)
        {
            switch (window.WindowType)
            {
                case ChildWindowType.Console:
                    /* Add a root node */
                    var node = Nodes.Add(window.Handle.ToString(), window.ToString(), 0, 0);
                    node.Tag = window;
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
                            window.DisplayNode = ((TreeNodeEx)subNodes[0]).AddSorted(window.Handle.ToString(),
                                                                                    window.ToString(), iconIndex,
                                                                                    iconIndex);
                            window.DisplayNode.Tag = window;                          
                            subNodes[0].Expand();
                            window.DisplayNodeRoot = subNodes[0];
                        }
                        else
                        {
                            if (!window.NoActivate)
                            {
                                /* Just insert it at the bottom */
                                SelectedNode = rootNode[0].Nodes.Add(window.Handle.ToString(), window.ToString(),
                                                                     iconIndex,
                                                                     iconIndex);
                            }
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

        public void AddNotify(ClientConnection client, User user)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var rootNode = Nodes.Find(c.Handle.ToString(), true);
            if (rootNode[0].Nodes.IndexOfKey("NOTIFY") == -1)
            {
                rootNode[0].Nodes.Insert(3, new TreeNodeEx("NOTIFY", "Notify", 4, 4));
            }
            var subNodes = rootNode[0].Nodes.Find("NOTIFY", true);
            if (subNodes.Length > 0)
            {
                var node = ((TreeNodeEx)subNodes[0]).AddSorted(user.Nick, user.Nick, 5, 5);
                node.Tag = user;
                subNodes[0].Expand();
                subNodes[0].Text = string.Format("Notify ({0})", subNodes[0].Nodes.Count);
            }
            rootNode[0].Expand();
        }

        public void RemoveNotify(ClientConnection client, User user)
        {
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var rootNode = Nodes.Find(c.Handle.ToString(), true);
            var subNodes = rootNode[0].Nodes.Find("NOTIFY", true);
            if (subNodes.Length > 0)
            {
                foreach (var n in from TreeNodeEx n in subNodes[0].Nodes where n.Tag == user select n)
                {
                    subNodes[0].Nodes.Remove(n);
                    break;
                }
            }
            if (subNodes[0].Nodes.Count == 0)
            {
                rootNode[0].Nodes.Remove(subNodes[0]);
            }
        }

        /* Callbacks */
        private void OnNodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            _timer.Enabled = false;
            _toolTip.Hide(this);

            _toolTipNode = e.Node;

            _timer.Interval = InitialToolTipDelay;
            _timer.Enabled = true;
        }

        private void ToolTipTimer(object sender, EventArgs e)
        {
            _timer.Enabled = false;
            var title = string.Empty;
            var msg = string.Empty;
            if (_toolTipNode.Tag is FrmChildWindow)
            {
                var t = (FrmChildWindow) _toolTipNode.Tag;
                title = t.Tag.ToString();
                switch (t.WindowType)
                {
                    case ChildWindowType.Console:
                        msg = t.Client.IsConnected
                                  ? string.Format("Connected to: {0}",
                                                  !string.IsNullOrEmpty(t.Client.Network)
                                                      ? string.Format("{0} ({1})", t.Client.Network, t.Client.Server)
                                                      : t.Client.Server.Address)
                                  : "Not connected";
                        break;

                    case ChildWindowType.Channel:
                        var topic = Functions.StripControlCodes(t.Modes.Topic);
                        if (string.IsNullOrEmpty(topic))
                        {
                            topic = "None";
                        }
                        msg = string.Format("Topic: {0}",
                                            topic.Length < 50 ? topic : string.Format("{0}...", topic.Substring(0, 50)));
                        break;

                    case ChildWindowType.Private:
                        msg = string.Format("Address: {0}", t.Client.Ial.Get(t.Tag.ToString()));
                        break;
                }
            }
            else if (_toolTipNode.Tag is User)
            {
                var u = (User) _toolTipNode.Tag;
                title = !string.IsNullOrEmpty(u.Address) ? string.Format("{0} ({1})", u.Nick, u.Address) : u.Nick;
                msg = !string.IsNullOrEmpty(u.Note) ? string.Format("Online - {0}", u.Note) : "Online";
            }
            else
            {
                return;
            }
            if (_timer.Interval == InitialToolTipDelay)
            {
                /* This is the closest I can get it to show with the arrow pointing to just above the mouse pointer */
                var mousePos = PointToClient(MousePosition);
                /* Show the ToolTip if the mouse is still over the same node. */
                if (_toolTipNode.Bounds.Contains(mousePos))
                {
                    /* Node location in treeView coordinates. */
                    var loc = mousePos;// _toolTipNode.Bounds.Location;
                    /* Node location in client coordinates - this isn't exactly correct */
                    var offset = _toolTipNode.Bounds.Height/2;
                    loc.Offset(-Location.X - offset, -Location.Y - offset);
                    /* Set tooltip title and show the tip */
                    _toolTip.ToolTipTitle = title;
                    _toolTip.Show(msg, this, loc);
                    /* Time out timer */
                    _timer.Interval = MaxToolTipDisplayTime;
                    _timer.Enabled = true;
                }
            }
            else
            {
                /* Maximium ToolTip display time exceeded. */
                _toolTip.Hide(this);
            }
        }
    }
}
