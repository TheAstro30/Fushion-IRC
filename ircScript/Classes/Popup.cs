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
using ircScript.Classes.Structures;

namespace ircScript.Classes
{
    /* Popup(s) are different from script(s) as this is just a container for single line commands to be
     * displayed in a popup menu - it gets passed to the command-line parser for processing */
    public class PopupData
    {
        public PopupType Type { get; set; }

        public string Name { get; set; }

        public string LineData { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class PopupManager
    {
        public static ToolStripMenuItem Commands = new ToolStripMenuItem("Commands");

        public static ContextMenuStrip Console = new ContextMenuStrip();
        public static ContextMenuStrip Channel = new ContextMenuStrip();
        public static ContextMenuStrip Nicklist = new ContextMenuStrip();
        public static ContextMenuStrip Private = new ContextMenuStrip();
        public static ContextMenuStrip DccChat = new ContextMenuStrip();

        public static ScriptData CommandsRawData = new ScriptData();
        public static ScriptData ConsoleRawData = new ScriptData();
        public static ScriptData ChannelRawData = new ScriptData();
        public static ScriptData NicklistRawData = new ScriptData();
        public static ScriptData PrivateRawData = new ScriptData();
        public static ScriptData DccChatRawData = new ScriptData();

        /* Events raised by this class */
        public static event Action<PopupData> OnPopupItemClicked;
        public static event Action OnPopupClosed;

        public static void LoadMultiplePopups(List<SettingsScripts.SettingsScriptPath> popups)
        {
            foreach (var s in popups)
            {
                switch (s.Type)
                {
                    case PopupType.Commands:
                        LoadPopup(Functions.MainDir(s.Path), ref CommandsRawData);
                        break;

                    case PopupType.Console:
                        LoadPopup(Functions.MainDir(s.Path), ref ConsoleRawData);
                        BuildPopups(s.Type, ConsoleRawData, Console.Items);
                        break;

                    case PopupType.Channel:
                        LoadPopup(Functions.MainDir(s.Path), ref ChannelRawData);
                        BuildPopups(s.Type, ChannelRawData, Channel.Items);
                        break;

                    case PopupType.Nicklist:
                        LoadPopup(Functions.MainDir(s.Path), ref NicklistRawData);
                        BuildPopups(s.Type, NicklistRawData, Nicklist.Items);
                        break;

                    case PopupType.Private:
                        LoadPopup(Functions.MainDir(s.Path), ref PrivateRawData);
                        BuildPopups(s.Type, PrivateRawData, Private.Items);
                        break;

                    case PopupType.DccChat:
                        LoadPopup(Functions.MainDir(s.Path), ref DccChatRawData);
                        BuildPopups(s.Type, DccChatRawData, DccChat.Items);
                        break;
                }
            }
        }

        public static void LoadPopup(string fileName, ref ScriptData data)
        {
            if (!XmlSerialize<ScriptData>.Load(fileName, ref data))
            {
                data = new ScriptData();
            }
        }

        public static void SavePopup(ScriptData data)
        {
            if (data.ContentsChanged)
            {
                XmlSerialize<ScriptData>.Save(data.ToString(), CommandsRawData);
            }
        }

        public static void BuildPopups(PopupType type, ScriptData data, ToolStripItemCollection popup)
        {
            /* This code is "borrowed" from my previous IRC client, dIRC7 - thing is, it may be ugly, but it works
             * and serves the purpose, so why change it? */
            popup.Clear();
            var lastLevel = 0;
            ToolStripMenuItem currentNode = null;
            ToolStripMenuItem lastNode = null;
            foreach (var l in data.RawScriptData)
            {
                var line = l;
                /* Get our current menu level based on the leading dots */
                var currentLevel = GetLevel(line);
                /* Strip off the leading dots if there's at least one */
                if (currentLevel > 0)
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
                    string name;
                    var lineData = string.Empty;
                    if (i != -1)
                    {
                        name = line.Substring(0, i);
                        lineData = line.Substring(i + 1);
                    }
                    else
                    {
                        name = line;
                    }
                    var p = new PopupData {Type = type, Name = name, LineData = lineData};
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
        }

        /* Click callback */
        private static void OnMenuItemClick(object sender, EventArgs e)
        {
            var t = (ToolStripMenuItem) sender;
            if (t == null)
            {
                return;
            }
            var data = (PopupData) t.Tag;
            if (data == null || string.IsNullOrEmpty(data.LineData))
            {
                return;
            }
            if (OnPopupItemClicked != null)
            {
                OnPopupItemClicked(data);
            }           
        }

        /* Private helper method */
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
