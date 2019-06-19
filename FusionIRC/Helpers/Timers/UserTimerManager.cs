/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Linq;
using FusionIRC.Forms.Child;
using ircClient;
using ircCore.Settings.Theming;
using ircCore.Settings.Theming.Structures;

namespace FusionIRC.Helpers.Timers
{
    public static class UserTimerManager
    {
        /* Helper class for creating script-based timers */
        public static List<UserTimer> Timers = new List<UserTimer>();

        public static void ParseTimer(ClientConnection client, FrmChildWindow child, string args)
        {
            var l = new List<string>(args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries));
            if (l.Count == 0)
            {
                return;
            }
            var ms = false;
            var silent = false;
            ParseSwitches(ref l, ref ms, ref silent);
            if (l.Count > 3)
            {
                /* Timers must be /timer <name> <continuous> <interval-num> <command> */
                CreateTimer(client, child, l, ms, silent);
            }
            else if(l.Count == 2)
            {
                /* Halt */
                if (l[1].ToUpper() == "OFF")
                {
                    DestroyTimer(client, l);
                }
            }
        }

        /* Private helper methods */
        private static void ParseSwitches(ref List<string> args, ref bool ms, ref bool silent)
        {
            if (args[0][0] != '-')
            {
                return;
            }
            foreach (var c in args[0])
            {
                switch (Char.ToUpper(c))
                {
                    case 'M':
                        /* Timer is in milliseconds */
                        ms = true;
                        break;

                    case 'Q':
                        silent = true;
                        break;
                }
            }
            args.RemoveAt(0);
        }

        private static void CreateTimer(ClientConnection client, FrmChildWindow child, List<string> args, bool ms, bool silent)
        {
            int constant;
            if (!int.TryParse(args[1], out constant))
            {
                return;
            }
            int interval;
            if (!int.TryParse(args[2], out interval))
            {
                return;
            }
            var name = args[0];
            var t = GetTimer(name);
            args.RemoveRange(0, 3);
            if (t != null)
            {
                t.Stop();
                t.OnTimer -= OnTimer;
                t.OnTimerEnded -= OnTimerEnded;
                Timers.Remove(t);
            }
            CreateTimerInternal(client, child, name, interval, string.Join(" ", args), ms, constant == 0, silent);
            DisplayText(client, string.Format("Timer \"{0}\" started", name), silent);
        }

        private static void CreateTimerInternal(ClientConnection client, FrmChildWindow child, string name, int interval, string command, bool isMilliseconds, bool isConstant, bool isSilent)
        {
            var t = new UserTimer(isMilliseconds)
                        {
                            Name = name,
                            Interval = interval,
                            IsContinuous = isConstant,
                            Command = command,
                            Client = client,
                            Child = child,
                            IsSilent = isSilent,
                            Enabled = true
                        };
            t.OnTimer += OnTimer;
            t.OnTimerEnded += OnTimerEnded;
            Timers.Add(t);
            t.Start();
        }

        private static void DestroyTimer(ClientConnection client, IList<string> args)
        {
            UserTimer t;
            if (args[0] == "*")
            {
                /* Kill all timers */
                var count = Timers.Count;
                for (var i = count - 1; i >= 0; i--)
                {
                    t = Timers[i];
                    t.Stop();
                    t.OnTimer -= OnTimer;
                    t.OnTimerEnded -= OnTimerEnded;
                    Timers.Remove(t);
                }
                if (count > 0)
                {
                    DisplayText(client, "All timers stopped", false);
                }
            }
            else
            {
                /* Get timer by name */
                t = GetTimer(args[0]);
                if (t == null)
                {
                    DisplayText(client, string.Format("No matching timer \"{0}\"", args[0]), false);
                    return;
                }
                t.Stop();
                t.OnTimer -= OnTimer;
                t.OnTimerEnded -= OnTimerEnded;
                Timers.Remove(t);
                DisplayText(client, string.Format("Timer \"{0}\" stopped", args[0]), false);
            }
        }

        private static UserTimer GetTimer(string name)
        {
            return Timers.FirstOrDefault(t => t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        private static void DisplayText(ClientConnection client, string text, bool silent)
        {
            if (silent)
            {
                return;
            }
            var c = WindowManager.GetConsoleWindow(client);
            if (c == null)
            {
                return;
            }
            var tmd = new IncomingMessageData
                          {
                              Message = ThemeMessage.InfoText,
                              TimeStamp = DateTime.Now,
                              Text = text
                          };
            var pmd = ThemeManager.ParseMessage(tmd);
            c.Output.AddLine(pmd.DefaultColor, pmd.Message);
            /* Update treenode color */
            WindowManager.SetWindowEvent(c, WindowManager.MainForm, WindowEvent.EventReceived);
        }

        /* Callbacks */
        private static void OnTimer(UserTimer timer)
        {
            /* Main callback to trigger "on TIMER" events */            
            CommandProcessor.Parse(timer.Client, timer.Child, timer.Command);
            if (timer.IsContinuous)
            {
                return;
            }
            DisplayText(timer.Client, string.Format("Timer \"{0}\" stopped", timer.Name), timer.IsSilent);
        }

        private static void OnTimerEnded(UserTimer timer)
        {
            timer.OnTimer -= OnTimer;
            timer.OnTimerEnded -= OnTimerEnded;
            Timers.Remove(timer);
        }
    }
}
