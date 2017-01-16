/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using FusionIRC.Controls;
using FusionIRC.Helpers;
using FusionIRC.Properties;
using ircClient;
using ircCore.Controls;
using ircCore.Controls.ChildWindows.Input;
using ircCore.Controls.ChildWindows.Nicklist;
using ircCore.Controls.ChildWindows.OutputDisplay;
using ircCore.Controls.ChildWindows.OutputDisplay.Helpers;
using ircCore.Settings;
using ircCore.Settings.Theming;
using ircCore.Utils;

namespace FusionIRC.Forms
{
    /* This class is our "main" chat window for console, channel, query and DCC chats - one class for all */
    public enum WindowEvent
    {
        None = 0,
        EventReceived = 1,
        MessageReceived = 2
    }

    public sealed class FrmChildWindow : ChildWindow
    {        
        private readonly Timer _focus;
        private WindowEvent _event;

        /* Public properties */
        public ChildWindowType WindowType { get; private set; }
        public ClientConnection Client { get; private set; }
        public OutputWindow Output { get; set; }
        public InputWindow Input { get; set; }
        public Nicklist Nicklist { get; set; }

        public SplitContainer Splitter { get; set; }

        /* Nodes used by the switch treeview - much easier to keep track of/update from here */
        public TreeNode DisplayNodeRoot { get; set; }
        public TreeNode DisplayNode { get; set; }
        
        public bool AutoClose { get; set; }

        public WindowEvent CurrentWindowEvent
        {
            get
            {
                return _event;
            }
            set
            {
                /* We can adjust our node color based on the event :) */                
                switch (value)
                {
                    case WindowEvent.None:
                        DisplayNode.ForeColor = Color.Black;
                        break;

                    case WindowEvent.EventReceived:
                        if (_event == WindowEvent.MessageReceived)
                        {
                            /* Messages take ownership */
                            return;
                        }
                        DisplayNode.ForeColor = Color.ForestGreen;
                        break;

                    case WindowEvent.MessageReceived:
                        DisplayNode.ForeColor = Color.Red;
                        break;
                }
                _event = value;
            }
        }
        
        /* Constructor */
        public FrmChildWindow(ClientConnection client, ChildWindowType type, Form owner)
        {
            /* Constructor where we pass what type of window this is - then we know what controls to create ;) */
            Client = client;
            WindowType = type;            
            /* Controls */
            Input = new InputWindow
                        {
                            BackColor = ThemeManager.GetColor(ThemeColor.WindowBackColor),//change to its own
                            ForeColor = ThemeManager.GetColor(ThemeColor.WindowForeColor),//change to its own
                            Font = ThemeManager.CurrentTheme.ThemeFonts[type]
                        };

            Output = new OutputWindow
                         {                             
                             BackColor = ThemeManager.GetColor(ThemeColor.WindowBackColor),
                             ForeColor = ThemeManager.GetColor(ThemeColor.WindowForeColor),
                             Font = ThemeManager.CurrentTheme.ThemeFonts[type],
                             LineSpacingStyle = LineSpacingStyle.Paragraph
                         };

            if (type == ChildWindowType.Channel)
            {                
                Nicklist = new Nicklist
                               {
                                   BackColor = ThemeManager.GetColor(ThemeColor.WindowBackColor),//change to its own
                                   ForeColor = ThemeManager.GetColor(ThemeColor.WindowForeColor),//change to its own
                                   Font = ThemeManager.CurrentTheme.ThemeFonts[type],
                                   UserModes = Client.Parser.UserModes,
                                   UserModeCharacters = Client.Parser.UserModeCharacters,
                                   Dock = DockStyle.Fill
                               };
                Splitter = new SplitContainer
                               {
                                   FixedPanel = FixedPanel.Panel2,
                                   Location = new Point(0, 0),
                                   Panel1MinSize = 250,                                   
                                   SplitterWidth = 1
                               };
                Splitter.Panel1.Controls.Add(Output);
                Splitter.Panel2MinSize = 80;
                Splitter.Panel2.Controls.Add(Nicklist);                
                Splitter.SplitterMoving += SplitterMoving;

                Output.Dock = DockStyle.Fill;
                Controls.AddRange(new Control[] {Input, Splitter});
            }
            else
            {
                Controls.AddRange(new Control[] { Input, Output });
            }
            /* Set up window background */
            var bd = ThemeManager.GetBackground(type);
            if (bd != null && File.Exists(bd.Path))
            {
                Output.BackgroundImage = (Bitmap)Image.FromFile(bd.Path);
                Output.BackgroundImageLayout = bd.LayoutStyle;
            }
            /* Callbacks */
            Input.TabKeyPress += InputTabKeyPress;
            Input.KeyDown += InputKeyDown;
            /* Window properties */
            BackColor = Color.FromArgb(190, 190, 190);            
            ShowInTaskbar = false;
            MdiParent = owner;
            MinimumSize = new Size(480, 280);
            /* Set window icon */
            switch (type)
            {
                case ChildWindowType.Console:
                    Icon = Resources.status;
                    break;

                case ChildWindowType.Channel:
                    Icon = Resources.channel;
                    break;

                case ChildWindowType.Private:
                    Icon = Resources.query;
                    break;

                case ChildWindowType.DccChat:
                    Icon = Resources.dcc_chat;
                    break;
            }
            /* Set client size */
            ClientSize = new Size(540, 400);
            /* Focus activation timer */
            _focus = new Timer {Interval = 10};
            _focus.Tick += TimerFocus;
        }
        
        /* Overrides */
        protected override void OnActivated(EventArgs e)
        {            
            _focus.Enabled = true;
            base.OnActivated(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            OnResize(new EventArgs());
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            /* Update window position in settings ... */
            switch (e.CloseReason)
            {
                case CloseReason.ApplicationExitCall:
                case CloseReason.WindowsShutDown:
                case CloseReason.MdiFormClosing:
                    return;
            }            
            if (WindowType == ChildWindowType.Console)                
            {
                if (WindowManager.Windows.Count == 1)
                {
                    /* Make sure we're not trying to close the only open console!! */
                    e.Cancel = true;
                    SystemSounds.Beep.Play();
                    return;
                }
                /* Before removing the window, it would be a good idea to 1) send the quit message & 2) close all windows associated with this console */
                Client.Send(string.Format("QUIT :Leaving."));
                WindowManager.RemoveAllWindowsOfConsole(Client);
            }            
            if (WindowType == ChildWindowType.Channel && Client.IsConnected && !AutoClose)
            {
                /* Part channel if closing it */
                Client.Send(string.Format("PART {0}", Tag));
            }
            WindowManager.RemoveWindow(Client, this);
            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            /* Adjust our controls based on window type */            
            var height = ClientRectangle.Height - Input.ClientRectangle.Height - 1;
            if (WindowType == ChildWindowType.Channel)
            {
                /* Set up our splitter */
                Splitter.SetBounds(0, 0, ClientRectangle.Width, height);
                Input.SetBounds(0, Splitter.ClientRectangle.Height + 1, ClientRectangle.Width, Input.ClientRectangle.Height);
                if (WindowState == FormWindowState.Normal)
                {
                    Splitter.SplitterDistance = ClientRectangle.Width - SettingsManager.Settings.SettingsWindows.NicklistWidth;
                }
            }
            else
            {
                /* Normal window, no nicklist/splitter */
                Output.SetBounds(0, 0, ClientRectangle.Width, height);
                Input.SetBounds(0, Output.ClientRectangle.Height + 1, ClientRectangle.Width, Input.ClientRectangle.Height);
            }            
            base.OnResize(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Output.Dispose();
                Input.Dispose();
                if (Nicklist != null)
                {
                    Nicklist.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        public override string ToString()
        {
            /* Return readable text for the tree nodes of the "switch view" */
            return Tag.ToString();
        }

        /* Control callbacks */
        private void InputTabKeyPress(InputWindow e)
        {
            //lstNicklist.TabNextNick(txtOut, IrcTokens.Gettok(Text, 1, 1, (char)32));
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Return:
                    var s = Utf8.ConvertToUtf8(Input.Text, true);
                    Input.Text = null;                    
                    /* Send text to server */
                    if (WindowType != ChildWindowType.Console)
                    {
                        if (s[0] == '/')
                        {
                            CommandProcessor.Parse(Client, this, s);
                            return;
                        }
                        if (!Client.IsConnected)
                        {
                            return;
                        }
                        var tmd = new IncomingMessageData
                                      {
                                          Message =
                                              WindowType == ChildWindowType.Channel
                                                  ? ThemeMessage.ChannelSelfText
                                                  : WindowType == ChildWindowType.Private
                                                        ? ThemeMessage.PrivateSelfText
                                                        : ThemeMessage.ChannelSelfText,
                                          TimeStamp = DateTime.Now,
                                          Nick = Client.UserInfo.Nick,
                                          Text = s
                                      };
                        var pmd = ThemeManager.ParseMessage(tmd);
                        Output.AddLine(pmd.DefaultColor, true, pmd.Message);
                        Client.Send(string.Format("PRIVMSG {0} :{1}", Tag, Utf8.ConvertFromUtf8(s, true)));
                        return;
                    }
                    /* Console window */
                    if (s[0] == '/')
                    {
                        CommandProcessor.Parse(Client, this, s);
                        return;
                    }
                    if (!Client.IsConnected)
                    {
                        return;
                    }
                    /* Console window - send data as raw */
                    Client.Send(Utf8.ConvertFromUtf8(s, true));
                    break;

                case Keys.Back:
                case Keys.Delete:
                case Keys.Up:
                case Keys.Right:
                case Keys.Left:
                case Keys.Down:
                case Keys.Space:
                    /* Nicklist clear tab search completion */
                    break;
            }
        }

        private void SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            /* Save the nicklist width */
            SettingsManager.Settings.SettingsWindows.NicklistWidth = ClientRectangle.Width - e.SplitX;
        }

        private void InputMouseWheel(object sender, MouseEventArgs e)
        {
            //txtIn.MouseWheelScroll(sender, e); not implemented on control yet
        }

        private void TimerFocus(object sender, EventArgs e)
        {
            _focus.Enabled = false;
            CurrentWindowEvent = WindowEvent.None;
            ((FrmClientWindow)MdiParent).switchTree.SelectedNode = DisplayNode;
            Input.Focus();
        }
    }
}
