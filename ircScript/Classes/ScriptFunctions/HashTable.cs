/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;

namespace ircScript.Classes.ScriptFunctions
{
    /* Hash table data class
       By: Jason James Newland
       ©2012 - KangaSoft Software
       All Rights Reserved
     */
    [Serializable]
    public class HashData
    {
        public string Key { get; set; }
        public string Data { get; set; }
    }

    [Serializable]
    public class HashTable
    {
        public Dictionary<string, HashData> HashData { get; set; }

        public HashTable()
        {
            HashData = new Dictionary<string, HashData>();
        }

        public int Count()
        {
            return HashData.Count;
        }

        public void Add(string key, string data)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(data))
            {
                return;
            }
            HashData hash;
            if (HashData.ContainsKey(key.ToLower()))
            {
                hash = new HashData { Key = key, Data = data };
                HashData[key.ToLower()] = hash;
                return;
            }
            hash = new HashData { Key = key, Data = data };
            HashData.Add(key.ToLower(), hash);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (HashData.ContainsKey(key.ToLower()))
            {
                HashData.Remove(key.ToLower());
            }
        }

        public bool ContainsKey(string key)
        {
            return HashData.ContainsKey(key);
        }

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            return HashData.ContainsKey(key.ToLower()) ? HashData[key.ToLower()].Data : null;
        }
    }
}
