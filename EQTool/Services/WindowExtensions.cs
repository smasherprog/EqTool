using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class WindowExtensions
{
    private const int MONITOR_DEFAULTTONEAREST = 0x00000002;
    private const int WM_GETMINMAXINFO = 0x0024;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    [DllImport("user32.dll")]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITOR_INFO lpmi);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

    public static void ToggleClickThrough(this Window window, bool enable)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }
        
        var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

        if (enable)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }
        else
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);
        }
    }

    public static void ApplyMaximizeWindowBoundsFix(this Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }
        
        var source = HwndSource.FromHwnd(hwnd);
        source?.AddHook(WindowProc);
    }

    #region Interop voodoo
    
    private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_GETMINMAXINFO)
        {
            try
            {
                AdjustMaximizeToWorkingArea(hwnd, lParam);
                handled = true;
            }
            catch
            {
                // Fail silently
            }
        }

        return IntPtr.Zero;
    }

    private static void AdjustMaximizeToWorkingArea(IntPtr hwnd, IntPtr lParam)
    {
        if (lParam == IntPtr.Zero)
        {
            return;
        }

        var minMax = Marshal.PtrToStructure<MIN_MAX_INFO>(lParam);

        var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        if (monitor == IntPtr.Zero)
        {
            return;
        }

        var monInfo = new MONITOR_INFO();
        monInfo.cbSize = Marshal.SizeOf(typeof(MONITOR_INFO));

        if (!GetMonitorInfo(monitor, ref monInfo))
        {
            return;
        }

        var rcWork = monInfo.rcWork;
        var rcMonitor = monInfo.rcMonitor;

        // Calculate work area relative to monitor
        minMax.ptMaxPosition.x = Math.Abs(rcWork.Left - rcMonitor.Left);
        minMax.ptMaxPosition.y = Math.Abs(rcWork.Top - rcMonitor.Top);
        minMax.ptMaxSize.x = Math.Abs(rcWork.Right - rcWork.Left);
        minMax.ptMaxSize.y = Math.Abs(rcWork.Bottom - rcWork.Top);

        Marshal.StructureToPtr(minMax, lParam, true);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    { 
        public int x;
        public int y;
    } 

    [StructLayout(LayoutKind.Sequential)]
    private struct MIN_MAX_INFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITOR_INFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion
}
