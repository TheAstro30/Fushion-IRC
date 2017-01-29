using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace xsDirectX
{
    [ComVisible(false)]
    public class DsDev
    {
        public static bool GetDevicesOfCat(Guid cat, out ArrayList devs)
        {
            devs = null;
            object comObj = null;
            IEnumMoniker enumMon = null;
            var mon = new IMoniker[1];
            try
            {
                var srvType = Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum);
                if (srvType == null)
                {
                    throw new NotImplementedException("System Device Enumerator");
                }

                comObj = Activator.CreateInstance(srvType);
                var enumDev = (ICreateDevEnum)comObj;
                var hr = enumDev.CreateClassEnumerator(ref cat, out enumMon, 0);
                if (hr != 0)
                {
                    throw new NotSupportedException("No devices of the category");
                }

                var count = 0;
                do
                {
                    var f = IntPtr.Zero;
                    hr = enumMon.Next(1, mon, f);
                    if (hr != 0 || mon[0] == null)
                    {
                        break;
                    }
                    var dev = new XsDevice { Name = GetFriendlyName(mon[0]) };
                    if (devs == null)
                    {
                        devs = new ArrayList();
                    }
                    dev.Mon = mon[0]; mon[0] = null;
                    devs.Add(dev);
                    count++;
                }
                while (true);

                return count > 0;
            }
            catch (Exception)
            {
                if (devs != null)
                {
                    foreach (XsDevice d in devs)
                    {
                        d.Dispose();
                    }
                    devs = null;
                }
                return false;
            }
            finally
            {
                if (mon[0] != null)
                {
                    Marshal.ReleaseComObject(mon[0]);
                    mon[0] = null;
                }
                if (enumMon != null)
                {
                    Marshal.ReleaseComObject(enumMon);                 
                }
                if (comObj != null)
                {
                    Marshal.ReleaseComObject(comObj);                    
                }
            }
        }

        private static string GetFriendlyName(IMoniker mon)
        {
            object bagObj = null;
            try
            {
                var bagId = typeof(IPropertyBag).GUID;
                mon.BindToStorage(null, null, ref bagId, out bagObj);
                var bag = (IPropertyBag)bagObj;
                object val = "";
                var hr = bag.Read("FriendlyName", ref val, IntPtr.Zero);
                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
                var ret = val as string;
                if (string.IsNullOrEmpty(ret))
                {
                    throw new NotImplementedException("Device FriendlyName");
                }
                return ret;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (bagObj != null)
                {
                    Marshal.ReleaseComObject(bagObj);
                }
            }
        }
    }

    [ComVisible(false)]
    public class XsDevice : IDisposable
    {
        public string Name;
        public IMoniker Mon;

        public void Dispose()
        {
            if (Mon == null) { return; }
            Marshal.ReleaseComObject(Mon);
            Mon = null;
        }
    }

    [ComVisible(true), ComImport,
    Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICreateDevEnum
    {
        [PreserveSig]
        int CreateClassEnumerator([In] ref Guid pType, [Out] out IEnumMoniker ppEnumMoniker, [In] int dwFlags);
    }

    [ComVisible(true), ComImport,
    Guid("55272A00-42CB-11CE-8135-00AA004BB851"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyBag
    {
        [PreserveSig]
        int Read(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [In] [Out] [MarshalAs(UnmanagedType.Struct)] ref object pVar,
            IntPtr pErrorLog);

        [PreserveSig]
        int Write(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [In] [MarshalAs(UnmanagedType.Struct)] ref object pVar);
    }
}