/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ircCore.Settings.SettingsBase.Structures;
using ircCore.Utils;
using ircCore.Utils.Serialization;
using ircScript.Classes;
using ircScript.Classes.Structures;

namespace ircScript
{
    public static class PopupManager
    {
        public static ToolStripMenuItem Commands = new ToolStripMenuItem("Commands");

        public static ContextMenuStrip Console = new ContextMenuStrip();
        public static ContextMenuStrip Channel = new ContextMenuStrip();
        public static ContextMenuStrip Nicklist = new ContextMenuStrip();
        public static ContextMenuStrip Private = new ContextMenuStrip();
        public static ContextMenuStrip DccChat = new ContextMenuStrip();

        public static List<ScriptData> Popups = new List<ScriptData>();

        /* Events raised by this class */
        public static event Action<Script> OnPopupItemClicked;

        public static void LoadMultiplePopups(List<SettingsScripts.SettingsScriptPath> popups)
        {
            foreach (var s in popups)
            {
                switch (s.Type)
                {                        
                    case PopupType.Commands:
                        LoadPopup(s, "menubar", Commands.DropDownItems);                        
                        break;

                    case PopupType.Console:
                        LoadPopup(s, "console", Console.Items);
                        break;

                    case PopupType.Channel:
                        LoadPopup(s, "channel", Channel.Items);
                        break;

                    case PopupType.Nicklist:
                        LoadPopup(s, "nicklist", Nicklist.Items);
                        break;

                    case PopupType.Private:
                        LoadPopup(s, "private", Private.Items);
                        break;

                    case PopupType.DccChat:
                        LoadPopup(s, "dcc-chat", DccChat.Items);
                        break;
                }
            }
        }

        public static void SavePopup(ScriptData data, string fileName)
        {
            XmlSerialize<ScriptData>.Save(fileName, data);
        }

        public static void ReBuildAllPopups()
        {
            BuildPopups(PopupType.Commands, Popups[0], Commands.DropDownItems);
            BuildPopups(PopupType.Console, Popups[1], Console.Items);
            BuildPopups(PopupType.Channel, Popups[2], Channel.Items);
            BuildPopups(PopupType.Nicklist, Popups[3], Nicklist.Items);
            BuildPopups(PopupType.Private, Popups[4], Private.Items);
            BuildPopups(PopupType.DccChat, Popups[5], DccChat.Items);
        }

        public static bool BuildPopups(PopupType type, ScriptData data, ToolStripItemCollection popup)
        {
            /* This code is "borrowed" from my previous IRC client, dIRC7 - thing is, it may be ugly, but it works
             * and serves the purpose, so why change it? */            
            popup.Clear();
            var lastLevel = 0;
            ToolStripMenuItem currentNode = null;
            ToolStripMenuItem lastNode = null;
            var codeBlock = 0;
            var p = new Script();
            foreach (var l in data.RawScriptData)
            {
                var line = l.Trim();
                /* Get our current menu level based on the leading dots */
                var currentLevel = codeBlock == 0 ? GetLevel(line) : lastLevel;
                /* Strip off the leading dots if there's at least one */
                if (currentLevel > 0 && codeBlock == 0)
                {
                    line = line.Substring(currentLevel);
                }
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                /* If the item equals a hyphen "-", then add the item as a seperator */
                ToolStripItem newNode;
                if (line == "-")
                {
                    newNode = new ToolStripSeparator();
                }
                else
                {
                    /* Create a new popup entry */
                    var i = line.IndexOf(':');
                    if (i != -1 && codeBlock == 0)
                    {
                        var name = line.Substring(0, i);
                        var lineData = line.Substring(i + 1);
                        p = new Script {Name = name, PopupType = type};
                        if (lineData.Length > 0)
                        {
                            if (lineData == "{")
                            {
                                /* Start of code block */
                                codeBlock++;
                                continue;
                            }
                            /* Add line to script data */
                            if (lineData.StartsWith("{") && lineData.EndsWith("}"))
                            {
                                lineData = lineData.Substring(1, lineData.Length - 2).Trim();
                            }
                            p.LineData.Add(lineData);
                        }
                    }
                    else
                    {
                        if (codeBlock > 0)
                        {
                            if (line.EndsWith("{"))
                            {
                                codeBlock++;
                                p.LineData.Add(line);
                                continue;
                            }
                            if (line.EndsWith("}"))
                            {
                                /* End of code block */
                                codeBlock--;
                                if (codeBlock > 0)
                                {
                                    p.LineData.Add(line);
                                    continue;
                                }
                            }
                            else
                            {
                                p.LineData.Add(line);
                                continue;
                            }
                        }
                        else
                        {
                            /* It's a menu item with subitems */
                            p = new Script {Name = line};
                        }
                    }
                    newNode = new ToolStripMenuItem(p.ToString(), null, OnMenuItemClick) {Tag = p};
                }
                if (currentLevel == 0)
                {
                    popup.Add(newNode);
                    lastLevel = 0;
                }
                else if (currentLevel == lastLevel)
                {
                    if (currentNode != null)
                    {
                        currentNode.DropDownItems.Add(newNode);
                    }
                }
                else if (currentLevel > lastLevel)
                {
                    if (lastNode != null)
                    {
                        lastNode.DropDownItems.Add(newNode);
                        currentNode = lastNode;
                    }
                    lastLevel = currentLevel;
                }
                else if (currentLevel < lastLevel)
                {
                    for (var j = 1; j <= (lastLevel - currentLevel); j++)
                    {
                        if (currentNode != null)
                        {
                            currentNode = (ToolStripMenuItem)currentNode.OwnerItem;
                        }
                    }
                    if (currentNode != null)
                    {
                        currentNode.DropDownItems.Add(newNode);
                    }
                    lastLevel = currentLevel;
                }
                var toolStripMenuItem = newNode as ToolStripMenuItem;
                if (toolStripMenuItem != null)
                {
                    lastNode = toolStripMenuItem;
                }
            }
            return popup.Count > 0;
        }

        /* Click callback */
        private static void OnMenuItemClick(object sender, EventArgs e)
        {
            var t = (ToolStripMenuItem) sender;
            if (t == null)
            {
                return;
            }
            var data = (Script) t.Tag;
            if (data == null || data.LineData.Count == 0)
            {
                return;
            }
            if (OnPopupItemClicked != null)
            {
                OnPopupItemClicked(data);
            }           
        }

        /* Private helper methods */
        private static void LoadPopup(SettingsScripts.SettingsScriptPath s, string name, ToolStripItemCollection menu)
        {
            var d = new ScriptData();
            if (LoadPopupInternal(Functions.MainDir(s.Path), ref d) == null)
            {
                d = new ScriptData {Name = name, ContentsChanged = true};
                SavePopup(d, Functions.MainDir(s.Path));
            }
            Popups.Add(d);
            if (s.Type != PopupType.Commands)
            {
                BuildPopups(s.Type, d, menu);
            }
        }

        private static ScriptData LoadPopupInternal(string fileName, ref ScriptData data)
        {
            /* Designed to return null */
            return !XmlSerialize<ScriptData>.Load(fileName, ref data) ? null : data;
        }

        private static int GetLevel(string text)
        {
            /* This function counts the number of dots "." at the beginning of the menu item
               to determine what menu level it is */
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }
            var inc = 0;
            for (var i = 0; i <= text.Length - 1; i++)
            {
                if (text[i] == '.')
                {
                    inc += 1;
                }
                else
                {
                    return inc;
                }
            }
            return 0;
        }
    }
}
