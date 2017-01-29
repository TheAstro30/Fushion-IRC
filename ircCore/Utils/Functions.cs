/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ircCore.Utils
{
    public static class Functions
    {
        public static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        public sealed class EnumUtils
        {
            public static string GetDescriptionFromEnumValue(Enum value)
            {
                var attribute = value.GetType()
                                    .GetField(value.ToString())
                                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                    .SingleOrDefault() as DescriptionAttribute;
                return attribute == null ? value.ToString() : attribute.Description;
            }
        }

        public class EnumComboData
        {
            public string Text { get; set; }
            public uint Data { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
