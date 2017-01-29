using System;
using System.Runtime.InteropServices;

namespace xsDirectX
{
    [ComVisible(true)]
    [ComImport]
        [Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaControl
    {
        [PreserveSig]
        int Run();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int GetState(int msTimeout, out int pfs);

        [PreserveSig]
        int RenderFile(string strFilename);

        [PreserveSig]
        int AddSourceFilter([In] string strFilename, [Out] [MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

        [PreserveSig]
        int GetFilterCollection([Out] [MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

        [PreserveSig]
        int GetRegFilterCollection([Out] [MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

        [PreserveSig]
        int StopWhenReady();
    }

    [ComVisible(true)]
    [ComImport]
        [Guid("56a868b6-0ad4-11ce-b03a-0020af0ba770")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaEvent
    {
        [PreserveSig]
        int GetEventHandle(out IntPtr hEvent);

        [PreserveSig]
        int GetEvent(out DxEvCode lEventCode, out int lParam1, out int lParam2, int msTimeout);

        [PreserveSig]
        int WaitForCompletion(int msTimeout, out int pEvCode);

        [PreserveSig]
        int CancelDefaultHandling(int lEvCode);

        [PreserveSig]
        int RestoreDefaultHandling(int lEvCode);

        [PreserveSig]
        int FreeEventParams(DxEvCode lEvCode, int lParam1, int lParam2);
    }

    [ComVisible(true)]
    [ComImport]
        [Guid("56a868c0-0ad4-11ce-b03a-0020af0ba770")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaEventEx
    {
        #region IMediaEvent Methods
        [PreserveSig]
        int GetEventHandle(out IntPtr hEvent);

        [PreserveSig]
        int GetEvent(out DxEvCode lEventCode, out int lParam1, out int lParam2, int msTimeout);

        [PreserveSig]
        int WaitForCompletion(int msTimeout, [Out] out int pEvCode);

        [PreserveSig]
        int CancelDefaultHandling(int lEvCode);

        [PreserveSig]
        int RestoreDefaultHandling(int lEvCode);

        [PreserveSig]
        int FreeEventParams(DxEvCode lEvCode, int lParam1, int lParam2);
        #endregion

        [PreserveSig]
        int SetNotifyWindow(IntPtr hwnd, int lMsg, IntPtr lInstanceData);

        [PreserveSig]
        int SetNotifyFlags(int lNoNotifyFlags);

        [PreserveSig]
        int GetNotifyFlags(out int lplNoNotifyFlags);
    }

    [ComVisible(true)]
    [ComImport]
        [Guid("329bb360-f6ea-11d1-9038-00a0c9697298")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IBasicVideo2
    {
        [PreserveSig]
        int AvgTimePerFrame(out double pAvgTimePerFrame);

        [PreserveSig]
        int BitRate(out int pBitRate);

        [PreserveSig]
        int BitErrorRate(out int pBitRate);

        [PreserveSig]
        int VideoWidth(out int pVideoWidth);

        [PreserveSig]
        int VideoHeight(out int pVideoHeight);

        [PreserveSig]
        int SetSourceLeft(int sourceLeft);

        [PreserveSig]
        int GetSourceLeft(out int pSourceLeft);

        [PreserveSig]
        int SetSourceWidth(int sourceWidth);

        [PreserveSig]
        int GetSourceWidth(out int pSourceWidth);

        [PreserveSig]
        int SetSourceTop(int sourceTop);

        [PreserveSig]
        int GetSourceTop(out int pSourceTop);

        [PreserveSig]
        int SetSourceHeight(int sourceHeight);

        [PreserveSig]
        int GetSourceHeight(out int pSourceHeight);

        [PreserveSig]
        int SetDestinationLeft(int destinationLeft);

        [PreserveSig]
        int GetDestinationLeft(out int pDestinationLeft);

        [PreserveSig]
        int SetDestinationWidth(int destinationWidth);

        [PreserveSig]
        int GetDestinationWidth(out int pDestinationWidth);

        [PreserveSig]
        int SetDestinationTop(int destinationTop);

        [PreserveSig]
        int GetDestinationTop(out int pDestinationTop);

        [PreserveSig]
        int SetDestinationHeight(int destinationHeight);

        [PreserveSig]
        int GetDestinationHeight(out int pDestinationHeight);

        [PreserveSig]
        int SetSourcePosition(int left, int top, int width, int height);

        [PreserveSig]
        int GetSourcePosition(out int left, out int top, out int width, out int height);

        [PreserveSig]
        int SetDefaultSourcePosition();

        [PreserveSig]
        int SetDestinationPosition(int left, int top, int width, int height);

        [PreserveSig]
        int GetDestinationPosition(out int left, out int top, out int width, out int height);

        [PreserveSig]
        int SetDefaultDestinationPosition();

        [PreserveSig]
        int GetVideoSize(out int pWidth, out int pHeight);

        [PreserveSig]
        int GetVideoPaletteEntries(int startIndex, int entries, out int pRetrieved, IntPtr pPalette);

        [PreserveSig]
        int GetCurrentImage(ref int pBufferSize, IntPtr pDibImage);

        [PreserveSig]
        int IsUsingDefaultSource();

        [PreserveSig]
        int IsUsingDefaultDestination();

        [PreserveSig]
        int GetPreferredAspectRatio(out int plAspectX, out int plAspectY);
    }

    [ComVisible(true)]
    [ComImport]
        [Guid("56a868b4-0ad4-11ce-b03a-0020af0ba770")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IVideoWindow
    {
        [PreserveSig]
        int SetCaption(string caption);

        [PreserveSig]
        int GetCaption([Out] out string caption);

        [PreserveSig]
        int SetWindowStyle(int windowStyle);

        [PreserveSig]
        int GetWindowStyle(out int windowStyle);

        [PreserveSig]
        int SetWindowStyleEx(int windowStyleEx);

        [PreserveSig]
        int GetWindowStyleEx(out int windowStyleEx);

        [PreserveSig]
        int SetAutoShow(int autoShow);

        [PreserveSig]
        int GetAutoShow(out int autoShow);

        [PreserveSig]
        int SetWindowState(int windowState);

        [PreserveSig]
        int GetWindowState(out int windowState);

        [PreserveSig]
        int SetBackgroundPalette(int backgroundPalette);

        [PreserveSig]
        int GetBackgroundPalette(out int backgroundPalette);

        [PreserveSig]
        int SetVisible(int visible);

        [PreserveSig]
        int GetVisible(out int visible);

        [PreserveSig]
        int SetLeft(int left);

        [PreserveSig]
        int GetLeft(out int left);

        [PreserveSig]
        int SetWidth(int width);

        [PreserveSig]
        int GetWidth(out int width);

        [PreserveSig]
        int SetTop(int top);

        [PreserveSig]
        int GetTop(out int top);

        [PreserveSig]
        int SetHeight(int height);

        [PreserveSig]
        int GetHeight(out int height);

        [PreserveSig]
        int SetOwner(IntPtr owner);

        [PreserveSig]
        int GetOwner(out IntPtr owner);

        [PreserveSig]
        int SetMessageDrain(IntPtr drain);

        [PreserveSig]
        int GetMessageDrain(out IntPtr drain);

        [PreserveSig]
        int GetBorderColor(out int color);

        [PreserveSig]
        int SetBorderColor(int color);

        [PreserveSig]
        int GetFullScreenMode(out int fullScreenMode);

        [PreserveSig]
        int SetFullScreenMode(int fullScreenMode);

        [PreserveSig]
        int SetWindowForeground(int focus);

        [PreserveSig]
        int NotifyOwnerMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [PreserveSig]
        int SetWindowPosition(int left, int top, int width, int height);

        [PreserveSig]
        int GetWindowPosition(out int left, out int top, out int width, out int height);

        [PreserveSig]
        int GetMinIdealImageSize(out int width, out int height);

        [PreserveSig]
        int GetMaxIdealImageSize(out int width, out int height);

        [PreserveSig]
        int GetRestorePosition(out int left, out int top, out int width, out int height);

        [PreserveSig]
        int HideCursor(int hideCursor);

        [PreserveSig]
        int IsCursorHidden(out int hideCursor);

    }

    [ComVisible(true)]
    [ComImport]
        [Guid("56a868b2-0ad4-11ce-b03a-0020af0ba770")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaPosition
    {
        [PreserveSig]
        int GetDuration(out double pLength);

        [PreserveSig]
        int SetCurrentPosition(double llTime);

        [PreserveSig]
        int GetCurrentPosition(out double pllTime);

        [PreserveSig]
        int GetStopTime(out double pllTime);

        [PreserveSig]
        int SetStopTime(double llTime);

        [PreserveSig]
        int GetPrerollTime(out double pllTime);

        [PreserveSig]
        int SetPrerollTime(double llTime);

        [PreserveSig]
        int SetRate(double dRate);

        [PreserveSig]
        int GetRate(out double pdRate);

        [PreserveSig]
        int CanSeekForward(out int pCanSeekForward);

        [PreserveSig]
        int CanSeekBackward(out int pCanSeekBackward);
    }

    [ComVisible(true)]
    [ComImport]
        [Guid("56a868b3-0ad4-11ce-b03a-0020af0ba770")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IBasicAudio
    {
        [PreserveSig]
        int SetVolume(int lVolume);

        [PreserveSig]
        int GetVolume(out int plVolume);

        [PreserveSig]
        int SetBalance(int lBalance);

        [PreserveSig]
        int GetBalance(out int plBalance);
    }

    [ComVisible(true)]
    [ComImport]
        [Guid("56a868b9-0ad4-11ce-b03a-0020af0ba770")]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IAMCollection
    {
        [PreserveSig]
        int GetCount(out int plCount);

        [PreserveSig]
        int Item(int lItem, [Out] [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);

        [PreserveSig]
        int GetNewEnum([Out] [MarshalAs(UnmanagedType.IUnknown)] out object ppUnk);
    }

    public enum DxEvCode
    {
        None,
        Complete = 0x01, // EC_COMPLETE
        UserAbort = 0x02, // EC_USERABORT
        ErrorAbort = 0x03, // EC_ERRORABORT
        Time = 0x04, // EC_TIME
        Repaint = 0x05, // EC_REPAINT
        StErrStopped = 0x06, // EC_STREAM_ERROR_STOPPED
        StErrStPlaying = 0x07, // EC_STREAM_ERROR_STILLPLAYING
        ErrorStPlaying = 0x08, // EC_ERROR_STILLPLAYING
        PaletteChanged = 0x09, // EC_PALETTE_CHANGED
        VideoSizeChanged = 0x0a, // EC_VIDEO_SIZE_CHANGED
        QualityChange = 0x0b, // EC_QUALITY_CHANGE
        ShuttingDown = 0x0c, // EC_SHUTTING_DOWN
        ClockChanged = 0x0d, // EC_CLOCK_CHANGED
        Paused = 0x0e, // EC_PAUSED
        OpeningFile = 0x10, // EC_OPENING_FILE
        BufferingData = 0x11, // EC_BUFFERING_DATA
        FullScreenLost = 0x12, // EC_FULLSCREEN_LOST
        Activate = 0x13, // EC_ACTIVATE
        NeedRestart = 0x14, // EC_NEED_RESTART
        WindowDestroyed = 0x15, // EC_WINDOW_DESTROYED
        DisplayChanged = 0x16, // EC_DISPLAY_CHANGED
        Starvation = 0x17, // EC_STARVATION
        OleEvent = 0x18, // EC_OLE_EVENT
        NotifyWindow = 0x19, // EC_NOTIFY_WINDOW
        // EC_ ....

        // DVDevCod.h
        DvdDomChange = 0x101, // EC_DVD_DOMAIN_CHANGE
        DvdTitleChange = 0x102, // EC_DVD_TITLE_CHANGE
        DvdChaptStart = 0x103, // EC_DVD_CHAPTER_START
        DvdAudioStChange = 0x104, // EC_DVD_AUDIO_STREAM_CHANGE

        DvdSubPicStChange = 0x105, // EC_DVD_SUBPICTURE_STREAM_CHANGE
        DvdAngleChange = 0x106, // EC_DVD_ANGLE_CHANGE
        DvdButtonChange = 0x107, // EC_DVD_BUTTON_CHANGE
        DvdValidUopsChange = 0x108, // EC_DVD_VALID_UOPS_CHANGE
        DvdStillOn = 0x109, // EC_DVD_STILL_ON
        DvdStillOff = 0x10a, // EC_DVD_STILL_OFF
        DvdCurrentTime = 0x10b, // EC_DVD_CURRENT_TIME
        DvdError = 0x10c, // EC_DVD_ERROR
        DvdWarning = 0x10d, // EC_DVD_WARNING
        DvdChaptAutoStop = 0x10e, // EC_DVD_CHAPTER_AUTOSTOP
        DvdNoFpPgc = 0x10f, // EC_DVD_NO_FP_PGC
        DvdPlaybRateChange = 0x110, // EC_DVD_PLAYBACK_RATE_CHANGE
        DvdParentalLChange = 0x111, // EC_DVD_PARENTAL_LEVEL_CHANGE
        DvdPlaybStopped = 0x112, // EC_DVD_PLAYBACK_STOPPED
        DvdAnglesAvail = 0x113, // EC_DVD_ANGLES_AVAILABLE
        DvdPeriodAStop = 0x114, // EC_DVD_PLAYPERIOD_AUTOSTOP
        DvdButtonAActivated = 0x115, // EC_DVD_BUTTON_AUTO_ACTIVATED
        DvdCmdStart = 0x116, // EC_DVD_CMD_START
        DvdCmdEnd = 0x117, // EC_DVD_CMD_END
        DvdDiscEjected = 0x118, // EC_DVD_DISC_EJECTED
        DvdDiscInserted = 0x119, // EC_DVD_DISC_INSERTED
        DvdCurrentHmsfTime = 0x11a, // EC_DVD_CURRENT_HMSF_TIME
        DvdKaraokeMode = 0x11b // EC_DVD_KARAOKE_MODE
    }
}