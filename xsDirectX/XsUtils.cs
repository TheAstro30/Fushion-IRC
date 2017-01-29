using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace xsDirectX
{
    [ComVisible(true)]
    [ComImport]
        [Guid("B196B28B-BAB4-101A-B69C-00AA00341D07")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISpecifyPropertyPages
    {
        [PreserveSig]
        int GetPages(out XsCauuid pPages);
    }

    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(false)]
    public struct XsCauuid
    {
        public int cElems;
        public IntPtr pElems;
    }

    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(false)]
    public class XsOptInt64
    {
        public XsOptInt64(long value)
        {
            Value = value;
        }

        public long Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(false)]
    public class XsOptIntPtr
    {
        public IntPtr Pointer;
    }

    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(false)]
    public struct XsPointStruct
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    [ComVisible(false)]
    public struct XsRectStruct
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    [ComVisible(false)]
    public struct XsBitmapinfoheader
    {
        public int Size;
        public int Width;
        public int Height;
        public short Planes;
        public short BitCount;
        public int Compression;
        public int ImageSize;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public int ClrUsed;
        public int ClrImportant;
    }

    public class XsUtils
    {
        [DllImport("olepro32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int OleCreatePropertyFrame(
            IntPtr hwndOwner,
            int x,
            int y,
            string lpszCaption,
            int cObjects,
            [In] [MarshalAs(UnmanagedType.Interface)] ref object ppUnk,
            int cPages,
            IntPtr pPageClsId,
            int lcid,
            int dwReserved,
            IntPtr pvReserved);

        public static bool IsCorrectDirectXVersion()
        {
            return File.Exists(Path.Combine(Environment.SystemDirectory, @"dpnhpast.dll"));
        }

        public static bool ShowCapPinDialog(ICaptureGraphBuilder2 bld, IBaseFilter flt, IntPtr hwnd)
        {
            object comObj = null;
            var cauuid = new XsCauuid();
            try
            {
                var cat = PinCategory.Capture;
                var type = MediaType.Interleaved;
                var iid = typeof(IAMStreamConfig).GUID;
                var hr = bld.FindInterface(ref cat, ref type, flt, ref iid, out comObj);
                if (hr != 0)
                {
                    type = MediaType.Video;
                    hr = bld.FindInterface(ref cat, ref type, flt, ref iid, out comObj);
                    if (hr != 0) { return false; }
                }
                var spec = comObj as ISpecifyPropertyPages;
                if (spec == null) { return false; }

                spec.GetPages(out cauuid);
                OleCreatePropertyFrame(hwnd, 30, 30, null, 1, ref comObj, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero);
                return true;
            }
            catch (Exception ee)
            {
                Trace.WriteLine("!xsDirectX: ShowCapPinDialog " + ee.Message);
                return false;
            }
            finally
            {
                if (cauuid.pElems != IntPtr.Zero) { Marshal.FreeCoTaskMem(cauuid.pElems); }
                if (comObj != null) { Marshal.ReleaseComObject(comObj); }
            }
        }

        public static bool ShowTunerPinDialog(ICaptureGraphBuilder2 bld, IBaseFilter flt, IntPtr hwnd)
        {
            object comObj = null;
            var cauuid = new XsCauuid();

            try
            {
                var cat = PinCategory.Capture;
                var type = MediaType.Interleaved;
                var iid = typeof(IAMTVTuner).GUID;
                var hr = bld.FindInterface(ref cat, ref type, flt, ref iid, out comObj);
                if (hr != 0)
                {
                    type = MediaType.Video;
                    hr = bld.FindInterface(ref cat, ref type, flt, ref iid, out comObj);
                    if (hr != 0) { return false; }
                }
                var spec = comObj as ISpecifyPropertyPages;
                if (spec == null) { return false; }

                spec.GetPages(out cauuid);
                OleCreatePropertyFrame(hwnd, 30, 30, null, 1, ref comObj, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero);
                return true;
            }
            catch (Exception ee)
            {
                Trace.WriteLine("!xsDirectX: ShowCapPinDialog " + ee.Message);
                return false;
            }
            finally
            {
                if (cauuid.pElems != IntPtr.Zero) { Marshal.FreeCoTaskMem(cauuid.pElems); }
                if (comObj != null) { Marshal.ReleaseComObject(comObj); }
            }
        }

        public static IPin[] GetPins(IBaseFilter filter, int num)
        {
            IPin[] pins;
            IEnumPins pinEnum = null;
            try
            {
                var pinArray = new IntPtr[num];
                filter.EnumPins(out pinEnum);
                int fetched;
                pinEnum.Next(num, pinArray, out fetched);
                pins = new IPin[fetched];
                for (int i = 0; i < fetched; i++)
                {
                    pins[i] = (IPin)Marshal.GetObjectForIUnknown(pinArray[i]);
                }
            }
            finally
            {
                if (pinEnum != null)
                {
                    Marshal.ReleaseComObject(pinEnum);
                }
            }
            return pins;
        }

        public static IBaseFilter[] GetFilters(IGraphBuilder builder, int num)
        {
            IBaseFilter[] getFilters;
            IEnumFilters baseFilters = null;
            try
            {
                var pinArray = new IntPtr[num];
                builder.EnumFilters(out baseFilters);
                int fetched;
                baseFilters.Next(num, pinArray, out fetched);
                getFilters = new IBaseFilter[fetched];
                for (int i = 0; i < fetched; i++)
                {
                    getFilters[i] = (IBaseFilter)Marshal.GetObjectForIUnknown(pinArray[i]);
                }
            }
            finally
            {
                if (baseFilters != null)
                {
                    Marshal.ReleaseComObject(baseFilters);
                }
            }
            return getFilters;
        }
    }

    [ComVisible(false)]
    public class DsRot
    {
        private const int RotflagsRegistrationkeepsalive = 1;

        [DllImport("ole32.dll", ExactSpelling = true)]
        private static extern int GetRunningObjectTable(int r, out IRunningObjectTable pprot);

        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int CreateItemMoniker(string delim, string item, out IMoniker ppmk);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern int GetCurrentProcessId();

        public static bool AddGraphToRot(object graph, out int cookie)
        {
            cookie = 0;
            IRunningObjectTable rot = null;
            IMoniker mk = null;
            try
            {
                var hr = GetRunningObjectTable(0, out rot);
                if (hr < 0) { Marshal.ThrowExceptionForHR(hr); }

                var id = GetCurrentProcessId();
                var iuPtr = Marshal.GetIUnknownForObject(graph);
                var iuInt = (int)iuPtr;
                Marshal.Release(iuPtr);
                var item = string.Format("FilterGraph {0} pid {1}", iuInt.ToString("x8"), id.ToString("x8"));
                hr = CreateItemMoniker("!", item, out mk);
                if (hr < 0) { Marshal.ThrowExceptionForHR(hr); }

                rot.Register(RotflagsRegistrationkeepsalive, graph, mk);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (mk != null) { Marshal.ReleaseComObject(mk); }
                if (rot != null) { Marshal.ReleaseComObject(rot); }
 
            }
        }

        public static bool RemoveGraphFromRot(ref int cookie)
        {
            IRunningObjectTable rot = null;
            try
            {
                var hr = GetRunningObjectTable(0, out rot);
                if (hr < 0) { Marshal.ThrowExceptionForHR(hr); }

                rot.Revoke(cookie);
                cookie = 0;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (rot != null) { Marshal.ReleaseComObject(rot); }
            }
        }
    }
}
