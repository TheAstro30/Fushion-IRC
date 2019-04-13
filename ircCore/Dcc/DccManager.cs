/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using ircCore.Utils;
using ircCore.Utils.Serialization;

namespace ircCore.Dcc
{
    public enum DccFileType
    {
        Download = 0,
        Upload = 1
    }

    public enum DccFileStatus
    {
        Waiting = 0,
        Downloading = 1,
        Uploading = 2,
        Completed = 3,
        Cancelled = 4,
        Failed = 5
    }

    [Serializable, XmlRoot("dccTransfers")]
    public class DccTransferData
    {
        /* This class stores recent file history - obviously we need to have a maximum number of stored files */
        [XmlElement("fileData")]
        public List<DccFile> FileData = new List<DccFile>();        
    }

    public class DccTest
    {
        public string FileName { get; set; }
    }

    [Serializable]
    public class DccFile
    {
        [XmlAttribute("type")]
        public DccFileType FileType { get; set; }

        [XmlAttribute("fileName")]
        public string FileName { get; set; }

        [XmlAttribute("userName")]
        public string UserName { get; set; }

        [XmlAttribute("address")]
        public string Address { get; set; }

        [XmlAttribute("fileSize")]
        public int FileSize { get; set; }

        [XmlAttribute("fileProgress")]
        public int Progress { get; set; }

        [XmlIgnore]
        public int Speed { get; set; }

        [XmlAttribute("fileStatus")]
        public DccFileStatus Status { get; set; }

        [XmlIgnore]
        public string UserNameToString
        {
            get { return string.Format("{0} ({1})", UserName, Address); }
        }

        [XmlIgnore]
        public string SpeedToString
        {
            get { return string.Format("{0}kb/s", Speed); }
        }
    }

    public static class DccManager
    {
        public static DccTransferData DccTransfers = new DccTransferData();

        public static void Load()
        {
            /* Load settings from disk */
            if (!XmlSerialize<DccTransferData>.Load(Functions.MainDir(@"\data\transfers.xml", false), ref DccTransfers))
            {
                DccTransfers = new DccTransferData();
            }
        }

        public static void Save()
        {
            /* Save settings to disk */
            XmlSerialize<DccTransferData>.Save(Functions.MainDir(@"\data\transfers.xml", false), DccTransfers);
        }

        public static void Add(DccFile file)
        {
            /* Check the total number of transfers, remove first entry if greater than maximum */
            if (DccTransfers.FileData.Count > 100)
            {
                DccTransfers.FileData.RemoveAt(0);
            }
            DccTransfers.FileData.Add(file);
            /* Sort list by uploads/downloads (type - downloads first, then by filename) */
            DccTransfers.FileData.Sort((x, y) =>
                                           {
                                               var result = x.FileType.CompareTo(y.FileType);
                                               return result != 0 ? result : x.FileName.CompareTo(y.FileName);
                                           });
        }

        public static void Clear()
        {
            DccTransfers.FileData.Clear();
        }
    }
}
