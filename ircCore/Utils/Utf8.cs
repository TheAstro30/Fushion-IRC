/* FusionIRC IRC Client
 * Written by Jason James Newland
 * Copyright (C) 2016 - 2017
 * Provided AS-IS with no warranty expressed or implied
 */
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace ircCore.Utils
{
    public sealed class Utf8
    {
        /* UTF8/IRC encoder/decoder module
           Original Author: MichKa
           Copyright © 1999 Trigeminal Software, Inc. All Rights Reserved.        
           Modified and converted to VB.NET/C# by Jason James Newland and sexist (Ryan Alexander)
           Copyright ©2009, Kangasoft Software
         */
        private enum Cp
        {
            CpAcp = 0,
            CpUtf8 = 65001,
            CpUnknown = -1
        }

        private const int Utf8DefaultReadsize = 1024;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern Cp GetACP();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int MultiByteToWideChar(Cp codePage, int dwFlags, IntPtr lpMultiByteStr, int cchMultiByte, IntPtr lpWideCharStr, int cchWideChar);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int WideCharToMultiByte(Cp codePage, int dwFlags, IntPtr lpWideCharStr, int cchWideChar, IntPtr lpMultiByteStr, int cchMultiByte, int lpDefaultChar, int lpUsedDefaultChar);

        /* ASCII character helper */
        public static char AsciiChr(int charCode)
        {
            if (charCode >= 0 && charCode <= 127) { return (char)charCode; }
            var encoding = Encoding.GetEncoding(Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage);
            var array = new char[2];
            var array2 = new byte[2];
            var decoder = encoding.GetDecoder();
            if (charCode >= 0 && charCode <= 255)
            {
                array2[0] = (byte)(charCode & 255);
                decoder.GetChars(array2, 0, 1, array, 0);
            }
            else
            {
                array2[0] = (byte)((charCode & 65280) >> 8);
                array2[1] = (byte)(charCode & 255);
                decoder.GetChars(array2, 0, 2, array, 0);
            }
            return array[0];            
        }

        /* Convert to UTF8 */
        public static string ConvertToUtf8(string text, bool processUtf8)
        {
            /* This checks if the whole string contains unicode then returns the result */
            if (!String.IsNullOrEmpty(text))
            {
                if (!processUtf8) { return text; }
                var s = new StringBuilder();
                if (text.Contains(" "))
                {
                    var temp = text.Split(' ');
                    for (var i = 0; i <= temp.Length - 1; i++)
                    {
                        if (IsUnicode(temp[i], false))
                        {
                            temp[i] = WtoA(temp[i], Cp.CpAcp, 0);
                            temp[i] = AtoW(temp[i], Cp.CpUtf8, 0);
                        }
                        if (i == 0) { s.Append(temp[i]); }
                        else { s.Append(' ' + temp[i]); }
                    }
                }
                else
                {
                    var sTmp = text;
                    if (IsUnicode(sTmp, false))
                    {
                        sTmp = WtoA(sTmp, Cp.CpAcp, 0);
                        sTmp = AtoW(sTmp, Cp.CpUtf8, 0);
                    }
                    s.Append(sTmp);
                }
                return s.ToString();
            }
            return null;
        }
        
        /* Reverse from UFT8 to ASCII */
        public static string ConvertFromUtf8(string text, bool processUtf8)
        {
            /* This checks if the whole string contains unicode then reverses it */
            var s = new StringBuilder();
            if (!processUtf8) { return text; }
            if (text.Contains(" "))
            {
                /* ChrW(160) causes a weird character to be displayed, so we replace it temporarily to
                   ChrW(4) */
                var temp = text.Replace('\x00a0', '\x0004').Split(' ');
                for (var i = 0; i <= temp.Length - 1; i++)
                {
                    if (!IsUnicode(temp[i], true))
                    {
                        temp[i] = WtoA(temp[i], Cp.CpUtf8, 0);
                        temp[i] = AtoW(temp[i], Cp.CpAcp, 0);
                    }

                    if (i == 0) { s.Append(temp[i]); }
                    else { s.Append(" " + temp[i]); }
                }
            }
            else
            {
                var sTmp = text;
                if (!IsUnicode(sTmp, true))
                {
                    sTmp = WtoA(sTmp, Cp.CpUtf8, 0);
                    sTmp = AtoW(sTmp, Cp.CpAcp, 0);
                }
                s.Append(sTmp);
            }
            /* Return result remembering to convert ChrW(4) back to ChrW(160) */
            return s.ToString().Replace('\x0004', '\x00a0');
        }
        
        public static byte[] StringToByteArray(string str)
        {
            return Encoding.Default.GetBytes(str);
        }
        
        public static bool IsUnicode(string text, bool reverse)
        {
            /* UTF8 Function (helps us decode the text data, if it's not UTF-8 encoded, the we don't need to
               process it) */
            var readPosition = 0;
            var utf8ByteSize = 0;
            var isUtf8 = 0;

            var readSize = Utf8DefaultReadsize;
            var bytArray = StringToByteArray(text);

            if (bytArray != null)
            {
                var lngArraySize = bytArray.Length;
                if (readSize > lngArraySize) { readSize = lngArraySize; }
                while (readPosition < readSize)
                {
                    if (bytArray[readPosition] <= 0x7f) { readPosition += 1; }
                    else if (bytArray[readPosition] < 0xc0) { return false; }
                    else if ((bytArray[readPosition] >= 0xc0) & (bytArray[readPosition] <= 0xfd))
                    {
                        switch (bytArray[readPosition] & 0xfc)
                        {
                            case 0xfc:
                                utf8ByteSize = 5;
                                break;
                            default:
                                if ((bytArray[readPosition] & 0xf8) == 0xf8)
                                {
                                    utf8ByteSize = 4;
                                }
                                else if ((bytArray[readPosition] & 0xf0) == 0xf0)
                                {
                                    utf8ByteSize = 3;
                                }
                                else if ((bytArray[readPosition] & 0xe0) == 0xe0)
                                {
                                    utf8ByteSize = 2;
                                }
                                else if ((bytArray[readPosition] & 0xc0) == 0xc0)
                                {
                                    utf8ByteSize = 1;
                                }
                                break;
                        }
                        if (readPosition + utf8ByteSize >= readSize) { break; }
                        for (var i = (readPosition + 1); i <= (readPosition + utf8ByteSize); i++)
                        {
                            if (!(bytArray[i] >= 0x80 & bytArray[i] <= 0xbf))
                            {
                                return false;
                            }
                        }
                        isUtf8 += 1;
                        readPosition = readPosition + utf8ByteSize + 1;
                    }
                    else { readPosition += 1; }
                }
                return ((reverse && isUtf8 > 0) || isUtf8 > 0);
            }
            return false;
        }
        
        /* Private helper methods */
        private static string AtoW(string st, Cp cpg, int flags)
        {
            /* ANSI to UNICODE conversion, via a given codepage. */
            if (cpg == Cp.CpUnknown) { cpg = GetACP(); }
            var pwz = Marshal.StringToHGlobalUni(st);
            var cwch = MultiByteToWideChar(cpg, flags, pwz, -1, IntPtr.Zero, 0);
            var stBuffer = new string((char)0, cwch + 1);
            if (!String.IsNullOrEmpty(stBuffer))
            {
                var pwzBuffer = Marshal.StringToHGlobalUni(stBuffer);
                MultiByteToWideChar(cpg, flags, pwz, -1, pwzBuffer, stBuffer.Length);
                Marshal.FreeHGlobal(pwz);
                var outStr = Marshal.PtrToStringUni(pwzBuffer);
                Marshal.FreeHGlobal(pwzBuffer);
                return outStr;
            }
            Marshal.FreeHGlobal(pwz);
            return null;
        }
        
        private static string WtoA(string st, Cp cpg, int flags)
        {
            /* UNICODE to ANSI conversion, via a given codepage */
            if (cpg == Cp.CpUnknown) { cpg = GetACP(); }
            var pwz = Marshal.StringToHGlobalUni(st);
            var cwch = WideCharToMultiByte(cpg, flags, pwz, -1, IntPtr.Zero, 0, 0, 0);
            var stBuffer = new string((char)0, cwch + 1);
            if (!String.IsNullOrEmpty(stBuffer))
            {
                var pwzBuffer = Marshal.StringToHGlobalUni(stBuffer);
                WideCharToMultiByte(cpg, flags, pwz, -1, pwzBuffer, stBuffer.Length, 0, 0);
                Marshal.FreeHGlobal(pwz);
                var outStr = Marshal.PtrToStringUni(pwzBuffer);
                Marshal.FreeHGlobal(pwzBuffer);
                return outStr;
            }
            Marshal.FreeHGlobal(pwz);
            return null;
        }
    }
}
