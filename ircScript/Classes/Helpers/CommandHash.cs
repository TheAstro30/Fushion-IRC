/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ircCore.Utils.Serialization;
using ircScript.Classes.ScriptFunctions;

namespace ircScript.Classes.Helpers
{
    public static class CommandHash
    {
        public static Dictionary<string, HashTable> HashTables = new Dictionary<string, HashTable>();

        public static void HashMake(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var tableKey = sp[0].ToLower();
            if (HashTables.ContainsKey(tableKey))
            {
                HashTables[tableKey] = new HashTable();
                return;
            }
            var hash = new HashTable();
            HashTables.Add(tableKey, hash);
        }

        public static void HashFree(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var tableKey = sp[0].ToLower();
            if (HashTables.ContainsKey(tableKey))
            {
                HashTables.Remove(tableKey);
            }         
        }
        
        public static void HashLoad(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (sp.Count < 2)
            {
                return;
            }
            var key = sp[0].ToLower();
            sp.RemoveAt(0);
            var file = string.Join(" ", sp);
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                return;
            }
            var hash = new HashTable();
            if (!BinarySerialize<HashTable>.Load(file, ref hash))
            {
                return;
            }
            if (HashTables.ContainsKey(key))
            {
                HashTables[key] = hash;
            }
            else
            {
                HashTables.Add(key, hash);
            }
        }

        public static void HashSave(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (sp.Count < 2)
            {
                return;
            }
            var key = sp[0].ToLower();
            sp.RemoveAt(0);
            var file = string.Join(" ", sp);
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(file))
            {
                return;
            }
            if (HashTables.ContainsKey(key))
            {
                BinarySerialize<HashTable>.Save(file, HashTables[key]);
            }
        }

        public static void HashAdd(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (sp.Count < 3)
            {
                return;
            }
            var tableKey = sp[0].ToLower();
            var key = sp[1].ToLower();
            sp.RemoveRange(0, 2);
            var data = string.Join(" ", sp);
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(data) || !HashTables.ContainsKey(tableKey))
            {
                return;
            }
            HashTables[tableKey].Add(key, data);
        }

        public static void HashDelete(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return;
            }
            var sp = args.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (sp.Length < 2)
            {
                return;
            }
            var tableKey = sp[0].ToLower();
            var key = sp[1].ToLower();
            if (!HashTables.ContainsKey(tableKey))
            {
                return;
            }
            if (string.IsNullOrEmpty(key))
            {
                HashTables[tableKey] = new HashTable();
            }
            else
            {
                HashTables[tableKey].Remove(key);    
            }            
        }

        public static string HashGet(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return string.Empty;
            }
            var tableKey = args[0].ToLower();
            var key = args[1].ToLower();
            if (!HashTables.ContainsKey(tableKey) || (!string.IsNullOrEmpty(key) && !HashTables[tableKey].ContainsKey(key)))
            {
                return string.Empty;
            }
            return string.IsNullOrEmpty(key)
                       ? HashTables[tableKey].Count().ToString(CultureInfo.InvariantCulture)
                       : HashTables[tableKey].Get(key);
        }
    }
}
