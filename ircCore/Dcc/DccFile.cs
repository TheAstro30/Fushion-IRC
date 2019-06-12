/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ircCore.Dcc
{
    public enum DccType
    {
        DccChat = 0,
        DccFileTransfer = 1
    }

    public enum DccChatType
    {
        Send = 0,
        Receive = 1
    }

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
}
