/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2019
 * Provided AS-IS with no warranty expressed or implied
 */
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ircCore.Utils.Serialization
{
    /* XML Serialization class     
     * © Copyright 2012 - 2013 Jason James Newland
     * KangaSoft Software - All Rights Reserved */
    public class XmlNamespace
    {
        public string Prefix { get; set; }
        public string Namespace { get; set; }
    }

    public class XmlSerialize<TType>
    {
        /* Load method */
        public static bool Load(string fileName, ref TType classObject)
        {
            var fi = new FileInfo(fileName);
            if (!fi.Exists || fi.Length == 0) { return false; }
            try
            {
                var xml = new XmlSerializer(typeof (TType));
                bool success;
                TType cRet;
                //using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                //{   
                using (TextReader fs = new StreamReader(fileName, Encoding.UTF8))
                {
                    try
                    {
                        //cRet = (TType)xml.Deserialize(fs);
                        /* This fixes an issue with deserialization of non-ascii characters */
                        using (var xtr = new XmlTextReader(fs))
                        {
                            var s = new XmlSerializer(typeof (TType));
                            cRet = (TType) s.Deserialize(xtr);
                        }
                        success = true;
                    }
                    catch
                    {
                        cRet = default(TType);
                        success = false;
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                classObject = cRet;
                return success;
            }
            catch
            {
                return false;
            }
        }

        /* Save methods */
        public static bool Save(string fileName, TType classObject)
        {
            return Save(fileName, classObject, null);
        }

        public static bool Save(string fileName, TType classObject, XmlNamespace nameSpace)
        {
            try
            {
                var xml = new XmlSerializer(typeof (TType));
                XmlSerializerNamespaces ns = null;
                if (nameSpace != null)
                {
                    ns = new XmlSerializerNamespaces();
                    ns.Add(nameSpace.Prefix, nameSpace.Namespace);
                }
                bool success;
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    try
                    {
                        using (TextWriter tw = new StreamWriter(fs, Encoding.UTF8))
                        {
                            if (ns != null)
                            {
                                xml.Serialize(tw, classObject, ns);
                            }
                            else
                            {
                                xml.Serialize(tw, classObject);
                            }
                            success = true;
                        }
                    }
                    catch
                    {
                        success = false;
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                return success;
            }
            catch
            {
                return false;
            }
        }
    }
}