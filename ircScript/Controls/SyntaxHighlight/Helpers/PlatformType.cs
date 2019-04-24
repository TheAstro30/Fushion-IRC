//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  License: GNU Lesser General Public License (LGPLv3)
//
//  Email: pavel_torgashov@ukr.net
//
//  Copyright (C) Pavel Torgashov, 2011-2016. 
using System;
using System.Runtime.InteropServices;

namespace ircScript.Controls.SyntaxHighlight.Helpers
{
    public enum Platform
    {
        X86 = 0,
        X64 = 1,
        Unknown = 2
    }

    public static class PlatformType
    {
        public const ushort ProcessorArchitectureIntel = 0;
        public const ushort ProcessorArchitectureIa64 = 6;
        public const ushort ProcessorArchitectureAmd64 = 9;

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemInfo
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }

        [DllImport("kernel32.dll")]
        static extern void GetNativeSystemInfo(ref SystemInfo lpSystemInfo);

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(ref SystemInfo lpSystemInfo);

        public static Platform GetOperationSystemPlatform()
        {
            var sysInfo = new SystemInfo();
            /* WinXP and older - use GetNativeSystemInfo */
            if (Environment.OSVersion.Version.Major > 5 ||
                (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1))
            {
                GetNativeSystemInfo(ref sysInfo);
            }
            else
            {
                GetSystemInfo(ref sysInfo);
            }
            switch (sysInfo.wProcessorArchitecture)
            {
                case ProcessorArchitectureIa64:
                case ProcessorArchitectureAmd64:
                    return Platform.X64;

                case ProcessorArchitectureIntel:
                    return Platform.X86;

                default:
                    return Platform.Unknown;
            }
        }
    }
}
