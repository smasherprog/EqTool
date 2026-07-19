using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EQTool.Services
{
    //
    // ForegroundWindowHelper
    //
    // Determine whether the eqgame client is the current foreground (focused) window.
    // Used by the "AFK Attacked" alert so we only warn the player when they are tabbed
    // away from EverQuest.
    //
    public static class ForegroundWindowHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public static bool IsEqGameFocused()
        {
            try
            {
                var hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                {
                    return false;
                }

                _ = GetWindowThreadProcessId(hwnd, out var pid);
                if (pid == 0)
                {
                    return false;
                }

                using (var process = Process.GetProcessById((int)pid))
                {
                    return string.Equals(process.ProcessName, "eqgame", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                // The foreground process may have exited between calls, or access may be denied.
                return false;
            }
        }
    }
}
