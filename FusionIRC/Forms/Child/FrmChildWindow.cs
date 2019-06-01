/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FusionIRC.Forms.Warning;
using FusionIRC.Helpers;
using FusionIRC.Helpers.Commands;
using FusionIRC.Properties;
using ircClient;
using ircCore.Controls;
using ircCore.Controls.ChildWindows.Classes;
using ircCore.Controls.ChildWindows.Classes.Channels;
using ircCore.Controls.ChildWindows.Input;
using ircCore.Controls.ChildWindows.Nicklist;
using ircCore.Controls.ChildWindows.OutputDisplay;
using ircCore.Settings;
using ircCore.Settings.Networks;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Settings.Theming;
using ircCore.Utils;
using ircScript;
using ircScript.Classes;
using ircScript.Classes.Structures;

namespace FusionIRC.Forms.Child
{
    /* This class is our "main" chat window for console, channel, query and DCC chats - one class for all */
    public sealed class FrmChildWindow : ChildWindow
    {
        /* Non alpha-numeric check - used for nick name on mouse-over of output window */
        private readonly Regex _regExNick = new Regex(@"[^A-Za-z0-9_-|#|\[\]\\\/`\^{}]", RegexOptions.Compiled);
        
        private readonly Timer _focus;
        private WindowEvent _event;
        private readonly SplitContainer _splitter;

        private readonly bool _initialize;
        private readonly string _windowChildName;

        public ReconnectOnDisconnect Reconnect;

        public Logger Logger = new Logger();

        public Channel Modes = new Channel();        

        /* Public properties */
        public ChildWindowType WindowType { get; private set; }
        public ClientConnection Client { get; private set; }
        public OutputWindow Output { get; set; }
        public InputWindow Input { get; set; }
        public Nicklist Nicklist { get; set; }

        /* Nodes used by the switch treeview - much easier to keep track of/update from here */
        public TreeNode DisplayNodeRoot { get; set; }
        public TreeNode DisplayNode { get; set; }
        
        public bool AutoClose { get; set; }

        /* This allows the child to be temporarily kept open during "hop" or "kick" for example */
        public bool KeepOpen { get; set; } /* In FormClosing, this will be reset to false automatically */

        public bool DisconnectedShown { get; set; } /* Stop "Disconnected" being shown in channel windows more than once */

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
            _initialize = true;            
            /* Constructor where we pass what type of window this is - then we know what controls to create ;) */
            Client = client;
            WindowType = type;           
            /* Next line is used for getting/setting window size/position & background image */
            _windowChildName = type == ChildWindowType.Console
                                   ? "console"
                                   : type == ChildWindowType.Channel
                                         ? "channel"
                                         : type == ChildWindowType.Private ? "private" : "chat";            
            /* Controls */
            Input = new InputWindow
                        {
                            BackColor = ThemeManager.GetColor(ThemeColor.InputWindowBackColor),
                            ForeColor = ThemeManager.GetColor(ThemeColor.InputWindowForeColor),
                            Font = ThemeManager.CurrentTheme.ThemeFonts[type],
                            MaximumHistoryCache = SettingsManager.Settings.Windows.Caching.Input,
                            ConfirmPaste = SettingsManager.Settings.Client.Confirmation.ConfirmPaste,
                            ConfirmPasteLines = SettingsManager.Settings.Client.Confirmation.PasteLines
                        };

            Output = new OutputWindow
                         {                             
                             BackColor = ThemeManager.GetColor(ThemeColor.OutputWindowBackColor),
                             LineMarkerColor = ThemeManager.GetColor(ThemeColor.OutputWindowLineMarkerColor),
                             Font = ThemeManager.CurrentTheme.ThemeFonts[type],
                             LinePaddingPixels = SettingsManager.Settings.Client.Messages.LinePadding,
                             LineSpacingStyle = SettingsManager.Settings.Client.Messages.LineSpacing,                             
                             MaximumLines = SettingsManager.Settings.Windows.Caching.Output
                         };

            /* Backgrounds */
            var bg = ThemeManager.GetBackground(type);
            if (!string.IsNullOrEmpty(bg.Path))
            {
                var file = Functions.MainDir(bg.Path);
                if (File.Exists(file))
                {
                    Output.BackgroundImage = (Bitmap)Image.FromFile(file);
                    Output.BackgroundImageLayout = bg.LayoutStyle;
                }
            }
            if (type == ChildWindowType.Channel)
            {                
                Nicklist = new Nicklist
                               {
                                   BackColor = ThemeManager.GetColor(ThemeColor.NicklistBackColor),
                                   ForeColor = ThemeManager.GetColor(ThemeColor.NicklistForeColor),
                                   Font = ThemeManager.CurrentTheme.ThemeFonts[type],
                                   UserModes = Client.Parser.UserModes,
                                   UserModeCharacters = Client.Parser.UserModeCharacters,
                                   Dock = DockStyle.Fill,
                                   Images = ThemeManager.GetNicklistImages(),
                                   ShowIcons = true, /* Change these two lines to the Theme.cs */
                                   ShowPrefix = true,
                                   Ial = Client.Ial
                               };
                Nicklist.OnNicklistDoubleClick += NicklistDoubleClickNick;
                Nicklist.OnNicklistRightClick += NicklistRightClick;
                /* Split control for nicklist */
                _splitter = new SplitContainer
                               {
                                   FixedPanel = FixedPanel.Panel2,
                                   Location = new Point(0, 0),
                                   Panel1MinSize = 250,                                   
                                   SplitterWidth = 1
                               };
                _splitter.Panel1.Controls.Add(Output);
                _splitter.Panel2MinSize = 80;
                _splitter.Panel2.Controls.Add(Nicklist);                
                _splitter.SplitterMoving += SplitterMoving;
                /* Only add splitter to controls, not the output window */
                Output.Dock = DockStyle.Fill;
                Controls.AddRange(new Control[] {Input, _splitter});
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
            Output.OnUrlDoubleClicked += OutputUrlDoubleClicked;
            Output.OnSpecialWordDoubleClicked += OutputSpecialWordDoubleClicked;
            Output.MouseDown += OutputMouseDown;
            Output.MouseUp += OutputMouseUp;
            Output.OnWordUnderMouse += OutputWordUnderMouse;
            Output.OnLineAdded += OutputOnLineAdded;
            Output.OnWindowDoubleClicked += OutputDoubleClicked;
            Input.TabKeyPress += InputTabKeyPress;
            Input.KeyDown += InputKeyDown;
            Input.MouseWheel += InputMouseWheel;
            Modes.OnSettingsChanged += ChannelSettingsChanged;
            /* Window properties */
            BackColor = Color.FromArgb(190, 190, 190);            
            ShowInTaskbar = false;
            MdiParent = owner;
            MinimumSize = new Size(480, 180);
            /* Set client size */
            ClientSize = new Size(540, 400);
            /* Focus activation timer */
            _focus = new Timer {Interval = 10};
            _focus.Tick += TimerFocus;
            /* Set window position and size - note: if a window is specified by it's "tag" (ie: #test), that takes priority */
            var w = SettingsManager.GetWindowByName(_windowChildName);
            if (w.Size.Width != 0 && w.Size.Height != 0)
            {
                Size = w.Size;
            }
            if (w.Position.X != -1 && w.Position.Y != -1)
            {
                Location = w.Position;
            }
            if (SettingsManager.Settings.Windows.ChildrenMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            Reconnect = new ReconnectOnDisconnect(Client);
            Reconnect.OnReconnectCancel += OnReconnectCancel;
            Reconnect.OnReconnectTimer += OnReconnectTimer;
            Reconnect.OnConnectionTry += OnConnectionTry;
            _initialize = false;            
        }
        
        /* Overrides */
        protected override void OnActivated(EventArgs e)
        {
            WindowManager.LastActiveChild = this;
            _focus.Enabled = true;
            base.OnActivated(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            var type = LoggingType.None;
            var file = string.Empty;
            if (WindowType != ChildWindowType.Console)
            {
                var name = Tag.ToString();
                Logger.FilePath = string.Format("{0}.log", Functions.GetLogFileName(Client.Network, name, true));
                type = SettingsManager.Settings.Client.Logging.ReloadLogsType;
                file = string.Format("{0}.buf", Functions.GetLogFileName(Client.Network, name));
            }
            /* Set window icon */
            switch (WindowType)
            {
                case ChildWindowType.Console:
                    Icon = Resources.status;
                    break;

                case ChildWindowType.Channel:
                    Icon = Resources.channel;
                    /* Check logging */
                    if (type == LoggingType.Channels || type == LoggingType.Both)
                    {
                        Output.LoadBuffer(file);
                        Logger.CreateLog();
                    }
                    break;

                case ChildWindowType.Private:
                case ChildWindowType.DccChat:
                    Icon = WindowType == ChildWindowType.Private ? Resources.query : Resources.dcc_chat;
                    /* Check logging */
                    if (type == LoggingType.Chats || type == LoggingType.Both)
                    {
                        Output.LoadBuffer(file);
                        Logger.CreateLog();
                    }
                    break;
            }
            OnResize(new EventArgs());            
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (KeepOpen)
            {
                KeepOpen = false;
                e.Cancel = true;
                return;
            }            
            Logger.CloseLog();
            /* Dump log buffer */
            var type = SettingsManager.Settings.Client.Logging.KeepLogsType;
            var file = string.Format("{0}.buf", Functions.GetLogFileName(Client.Network, Tag.ToString()));
            switch (WindowType)
            {
                case ChildWindowType.Channel:
                    if (type == LoggingType.Channels || type == LoggingType.Both)
                    {
                        Output.SaveBuffer(file);
                    }
                    break;

                case ChildWindowType.Private:
                case ChildWindowType.DccChat:
                    if (type == LoggingType.Chats || type == LoggingType.Both)
                    {
                        Output.SaveBuffer(file);
                    }
                    break;
            }
            /* Update window position in settings ... */
            switch (e.CloseReason)
            {
                case CloseReason.ApplicationExitCall:
                case CloseReason.WindowsShutDown:
                case CloseReason.MdiFormClosing:
                    return;
            }
            switch (WindowType)
            {
                case ChildWindowType.Console:
                    if (WindowManager.Windows.Count == 1)
                    {
                        /* Make sure we're not trying to close the only open console!! */
                        e.Cancel = true;
                        SystemSounds.Beep.Play();
                        return;
                    }
                    /* Check confirmation setting */
                    string msg = null;
                    switch (SettingsManager.Settings.Client.Confirmation.ConsoleClose)
                    {
                        case CloseConfirmation.Connected:
                            if (Client.IsConnected)
                            {
                                msg = "You are still connected to an IRC server. Are you sure you want to close this console window? ";
                            }
                            break;
                        
                        case CloseConfirmation.Always:
                            msg = "Are you sure you want to close this console window?";
                            break;
                    }
                    if (!string.IsNullOrEmpty(msg) && MessageBox.Show(msg, @"Confirm Close Console", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                    CommandChannel.ParseQuit(Client, string.Empty);
                    WindowManager.RemoveAllWindowsOfConsole(Client);
                    break;

                case ChildWindowType.Channel:
                    if (Client.IsConnected)
                    {
                        if (!AutoClose)
                        {
                            /* Part channel if closing it */
                            var p = SettingsManager.Settings.Client.Messages.PartMessage;
                            Client.Send(!string.IsNullOrEmpty(p)
                                            ? string.Format("PART {0} :{1}", Tag, p)
                                            : string.Format("PART {0}", Tag));
                        }
                        Client.Ial.RemoveChannel(Tag.ToString());
                    }
                    break;
            }
            /* Remove the window from the client */
            WindowManager.RemoveWindow(Client, this);
            _focus.Enabled = false;
            base.OnFormClosing(e);
        }

        protected override void OnMove(EventArgs e)
        {
            if (!_initialize)
            {
                var w = SettingsManager.GetWindowByName(_windowChildName);
                if (WindowState == FormWindowState.Normal)
                {
                    /* I think this is remarked out so I could modify WindowManger to get specific positions for
                     * chat windows named a certain name, eg: #dragonsrealm = this position in the MDI window, all
                     * other windows, assume default position ... etc. */
                    //w.Position = Location;
                }
            }
            base.OnMove(e);
        }

        protected override void OnResizeBegin(EventArgs e)
        {
            Output.UserResize = true;
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            Output.UserResize = false;
            base.OnResizeEnd(e);
        }

        protected override void OnResize(EventArgs e)
        {
            /* Adjust our controls based on window type */
            if (WindowState == FormWindowState.Minimized)
            {
                /* We don't do anything - can cause application lock-up */
                return;
            }
            var height = ClientRectangle.Height - Input.ClientRectangle.Height - 1;
            if (WindowType == ChildWindowType.Channel)
            {
                /* Set up our splitter */
                _splitter.SetBounds(0, 0, ClientRectangle.Width, height);
                Input.SetBounds(0, _splitter.ClientRectangle.Height + 1, ClientRectangle.Width, Input.ClientRectangle.Height);
                if (WindowState == FormWindowState.Normal)
                {
                    _splitter.SplitterDistance = ClientRectangle.Width - SettingsManager.Settings.Windows.NicklistWidth;
                }
            }
            else
            {
                /* Normal window, no nicklist/splitter */
                Output.SetBounds(0, 0, ClientRectangle.Width, height);
                Input.SetBounds(0, Output.ClientRectangle.Height + 1, ClientRectangle.Width, Input.ClientRectangle.Height);
            }
            /* Update settings */
            if (_initialize)
            {
                return;
            }
            switch (WindowState)
            {
                case FormWindowState.Normal:
                    var w = SettingsManager.GetWindowByName(_windowChildName);
                    w.Size = Size;
                    //w.Position = Location;
                    SettingsManager.Settings.Windows.ChildrenMaximized = false;
                    break;

                case FormWindowState.Maximized:
                    SettingsManager.Settings.Windows.ChildrenMaximized = true;
                    break;
            }
        }

        public override string ToString()
        {
            /* Return readable text for the tree nodes of the "switch view" */
            return Tag.ToString();
        }

        /* Channel modes class */
        private void ChannelSettingsChanged(Channel modes)
        {
            if (WindowType == ChildWindowType.Console)
            {
                Text = string.Format("{0}: {1} ({2}:{3}) {4}",
                                     !string.IsNullOrEmpty(Client.Network) ? Client.Network : Client.Server.Address,
                                     Client.UserInfo.Nick, Client.Server.Address, Client.Server.Port, modes);
                return;
            }
            Text = string.Format("{0} {1}", Tag, modes);
        }

        /* Control callbacks */
        private void OutputDoubleClicked()
        {
            var s = new Script();
            var scriptArgs = new ScriptArgs {ChildWindow = this, ClientConnection = Client};
            var args = new string[1];
            switch (WindowType)
            {
                case ChildWindowType.Console:
                    s.LineData.Add(SettingsManager.Settings.Mouse.Console);
                    break;

                case ChildWindowType.Channel:
                    args[0] = Tag.ToString();
                    scriptArgs.Channel = args[0];
                    s.LineData.Add(SettingsManager.Settings.Mouse.Channel);
                    break;

                case ChildWindowType.Private:
                    args[0] = Tag.ToString();
                    scriptArgs.Nick = args[0];
                    s.LineData.Add(SettingsManager.Settings.Mouse.Query);
                    break;
            }
            CommandProcessor.Parse(Client, this, s.Parse(scriptArgs, args));
        }

        private void OutputUrlDoubleClicked(string url)
        {            
            if (SettingsManager.Settings.Client.Confirmation.Url)
            {
                /* Create warning dialog and show it */
                using (var d = new FrmUrlWarn())
                {
                    d.Url = url;
                    d.ShowDialog(this);
                    if (d.DialogResult == DialogResult.Cancel)
                    {
                        return;                        
                    }
                    url = d.Url;
                }
            }
            Functions.OpenProcess(url);            
        }

        private void OutputSpecialWordDoubleClicked(string word)
        {
            /* Just incase... */
            if (string.IsNullOrEmpty(word))
            {
                return;
            }
            if (word[0] == Client.Parser.ChannelPrefixTypes.MatchChannelType(Tag.ToString()[0]))
            {
                /* Channel */
                Client.Send(string.Format("JOIN {0}", word));
            }
            else
            {                
                var nick = _regExNick.Replace(word, "");
                if (Nicklist != null && Nicklist.ContainsNick(nick))
                {
                    /* Open a query window with the nick (default action) */
                    var win = WindowManager.GetWindow(Client, nick) ??
                              WindowManager.AddWindow(Client, ChildWindowType.Private, MdiParent, nick, nick, true);
                    win.BringToFront();
                }
            }
        }

        private void OutputWordUnderMouse(string word)
        {
            /* This may or may not be a convoluted way of doing channel name/nick checks (instead of doing it on
             * the control itself), but I did it this way for 2 reasons:
             * a) We don't have to pass the Nicklist control as a property to the OutputWindow (means that the
             *    output window is a separate control to be used elsewhere without the need for another dependancy)
             * b) Not all channels start with # (some start with + or &), so we can look at the protocols string
             *    (RAW 005) for what the channel prefixes are and test for them externally here */
            if (!string.IsNullOrEmpty(word))
            {
                if (word[0] == Client.Parser.ChannelPrefixTypes.MatchChannelType(Tag.ToString()[0]))
                {
                    /* It's a channel name - including it's self */
                    Output.Cursor = Cursors.Hand;
                    Output.AllowSpecialWordDoubleClick = true;
                }
                else
                {
                    /* Check if it's a nick? */
                    if (Nicklist != null && Nicklist.ContainsNick(_regExNick.Replace(word, "")))
                    {
                        Output.Cursor = Cursors.Hand;
                        Output.AllowSpecialWordDoubleClick = true;
                        return;
                    }
                }
                return;
            }
            Output.Cursor = Cursors.Default;
            Output.AllowSpecialWordDoubleClick = false;
        }

        private void OutputMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }
            var p = new Point(Output.ClientRectangle.Left + e.X, Output.ClientRectangle.Top + e.Y);
            switch (WindowType)
            {
                case ChildWindowType.Console:
                    PopupManager.Console.Show(Output, p);
                    break;

                case ChildWindowType.Channel:
                    PopupManager.Channel.Show(Output, p);
                    break;

                case ChildWindowType.Private:
                    PopupManager.Private.Show(Output, p);
                    break;

                case ChildWindowType.DccChat:
                    PopupManager.DccChat.Show(Output, p);
                    break;
            }
        }        

        private void OutputMouseUp(object sender, MouseEventArgs e)
        {
            Input.Focus();
        }

        private void OutputOnLineAdded(string text)
        {
            var type = SettingsManager.Settings.Client.Logging.KeepLogsType;
            switch (WindowType)
            {
                case ChildWindowType.Console:
                    return;

                case ChildWindowType.Channel:
                    if (type != LoggingType.Channels && type != LoggingType.Both)
                    {
                        return;
                    }
                    break;

                case ChildWindowType.Private:
                case ChildWindowType.DccChat:
                    if (type != LoggingType.Chats && type != LoggingType.Both)
                    {
                        return;
                    }
                    break;
            }
            if (SettingsManager.Settings.Client.Logging.DateByDay)
            {
                var name = string.Format("{0}.log", Functions.GetLogFileName(Client.Network, Tag.ToString(), true));
                if (!name.Equals(Logger.FilePath))
                {
                    Logger.CloseLog();
                    Logger.FilePath = name;
                    Logger.CreateLog();
                }
            }
            Logger.WriteLog(SettingsManager.Settings.Client.Logging.StripCodes
                                ? Functions.StripControlCodes(text)
                                : text);
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Return:
                    if (string.IsNullOrEmpty(Input.Text))
                    {
                        SystemSounds.Beep.Play();
                        return;
                    }
                    var s = Input.Text;
                    Input.Text = string.Empty;                    
                    /* Send text to server */
                    char c;
                    char.TryParse(SettingsManager.Settings.Client.Messages.CommandCharacter, out c);
                    if (WindowType != ChildWindowType.Console)
                    {
                        if (s[0] == c && !e.Control)
                        {                            
                            if (s.Length > 1 && s[1] == c)
                            {
                                /* Process following line as a script */
                                ParseCommandLineAsScript(s.Substring(2));
                                return;
                            }
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
                                          Prefix =
                                              WindowType == ChildWindowType.Channel
                                                  ? Nicklist.GetNickPrefix(Client.UserInfo.Nick)
                                                  : null,
                                          Text = s
                                      };
                        var pmd = ThemeManager.ParseMessage(tmd);
                        Output.AddLine(pmd.DefaultColor, pmd.Message);
                        Client.Send(string.Format("PRIVMSG {0} :{1}", Tag, Utf8.ConvertFromUtf8(s, true)));
                        return;
                    }
                    /* Console window */
                    if (s[0] == c)
                    {
                        if (s.Length > 1 && s[1] == '/')
                        {
                            /* Process following line as a script */
                            ParseCommandLineAsScript(s.Substring(2));
                            return;
                        }
                        CommandProcessor.Parse(Client, this, s);
                        return;
                    }
                    break;

                case Keys.Back:
                case Keys.Delete:
                case Keys.Up:
                case Keys.Right:
                case Keys.Left:
                case Keys.Down:
                case Keys.Space:
                    /* Nicklist clear tab search completion */
                    if (Nicklist != null)
                    {
                        Nicklist.ClearTabNick();
                    }
                    break;
            }
        }

        private void InputTabKeyPress(InputWindow e)
        {
            /* Tab completion */
            var text = Text.Split(' ');
            if (text.Length < 1 || Nicklist == null)
            {
                return;
            }
            var rest = string.Empty;
            var s = Nicklist.TabNextNick(Input.Text, text[0], Input.SelectionStart, ref rest);            
            var i = 0;
            if (!string.IsNullOrEmpty(s))
            {
                i = s.Length;
            }
            Input.Text = !string.IsNullOrEmpty(rest) ? string.Format("{0} {1}", s, rest) : string.Format("{0}", s);
            Input.SelectionStart = i;
        }

        private void InputMouseWheel(object sender, MouseEventArgs e)
        {
            Output.MouseWheelScroll(e);
        }

        /* Nick list callbacks */
        private void NicklistDoubleClickNick()
        {
            if (Nicklist.SelectedNicks.Count == 0)
            {
                return;
            }
            var nick = Nicklist.SelectedNicks[0];
            /* Open a query window with the nick (default action) */
            var s = new Script();
            s.LineData.Add(SettingsManager.Settings.Mouse.Nicklist);
            var scriptArgs = new ScriptArgs
                                 {
                                     Channel = Tag.ToString(),
                                     ChildWindow = this,
                                     ClientConnection = Client,
                                     Nick = nick
                                 };
            CommandProcessor.Parse(Client, this, s.Parse(scriptArgs, new[] {nick}));
        }

        private void NicklistRightClick(MouseEventArgs e)
        {
            var p = new Point(Nicklist.ClientRectangle.Left + e.X, Nicklist.ClientRectangle.Top + e.Y);
            PopupManager.Nicklist.Show(Nicklist, p);
        }

        private void SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            /* Save the nicklist width */
            SettingsManager.Settings.Windows.NicklistWidth = ClientRectangle.Width - e.SplitX;
        }

        private void OnReconnectCancel()
        {
            if (WindowType == ChildWindowType.Console)
            {
                Text = string.Format("{0}: {1} ({2}:{3})",
                                     !string.IsNullOrEmpty(Client.Network) ? Client.Network : Client.Server.Address,
                                     Client.UserInfo.Nick, Client.Server.Address, Client.Server.Port);
            }
        }

        private void OnReconnectTimer(int count)
        {            
            if (WindowType == ChildWindowType.Console)
            {
                Text = string.Format("{0}: {1} ({2}:{3}) [{4} {5}]",
                                     !string.IsNullOrEmpty(Client.Network) ? Client.Network : Client.Server.Address,
                                     Client.UserInfo.Nick, Client.Server.Address, Client.Server.Port,
                                     "Reconnecting in: ", count);
            }
        }

        private void OnConnectionTry(Server s)
        {            
            Client.Connect(s.Address, s.Port, s.IsSsl);
        }

        private void ParseCommandLineAsScript(string line, string[] args = null)
        {
            var e = new ScriptArgs
                        {
                            ClientConnection = Client,
                            Channel = WindowType != ChildWindowType.Console ? Tag.ToString() : string.Empty,
                            ChildWindow = this,
                            Nick =
                                WindowType == ChildWindowType.Private || WindowType == ChildWindowType.DccChat
                                    ? Tag.ToString()
                                    : string.Empty
                        };
            var script = new Script();
            script.LineData.Add(line);
            CommandProcessor.Parse(Client, this, script.Parse(e, args));
        }

        private void TimerFocus(object sender, EventArgs e)
        {
            _focus.Enabled = false;
            if (NoActivate)
            {
                NoActivate = false;
                return;
            }         
            CurrentWindowEvent = WindowEvent.None;
            var ctl = ((FrmClientWindow) MdiParent).SwitchView;
            if (DisplayNode != null && DisplayNode != ctl.SelectedNode)
            {
                /* Only set SelectedNode if it's not already currently set to this DisplayNode */
                ctl.SelectedNode = DisplayNode;
            }
            Input.Focus();
        }
    }
}
