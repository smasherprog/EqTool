using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace EQTool.UI
{
    // Window snapping. While a window is dragged its edges snap to the other tool windows and to the
    // screen edges, and every window snapped to it (directly or through another snapped window) moves
    // along with it. Holding Shift when the drag starts moves just that one window, which is how a
    // window is pulled out of a group. Groups aren't saved anywhere: they are worked out from which
    // windows are touching each time a drag starts.
    public partial class BaseSaveStateWindow
    {
        private const int WM_MOVING = 0x0216;
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_EXITSIZEMOVE = 0x0232;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        // How close (in DIPs) an edge has to get to another edge before it snaps.
        private const double SnapDistance = 10;

        // How far apart (in pixels) two edges can be and still count as snapped together.
        // Allows for rounding when window positions are saved and restored.
        private const int AttachedTolerance = 2;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect(ref WindowExtensions.RECT lprc, uint dwFlags);

        private class SnapWindow
        {
            public IntPtr Handle;
            public WindowExtensions.RECT Rect;
        }

        private class SnapDrag
        {
            public WindowExtensions.RECT StartRect;
            public List<SnapWindow> Group;
            public List<SnapWindow> Targets;
        }

        private SnapDrag snapDrag;

        // Windows that shouldn't snap or be snapped to, like the see-through overlay, override this.
        protected virtual bool CanSnap => true;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource.FromHwnd(new WindowInteropHelper(this).Handle)?.AddHook(SnapWndProc);
        }

        private IntPtr SnapWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_ENTERSIZEMOVE:
                    snapDrag = BeginSnapDrag(hwnd);
                    break;
                case WM_MOVING:
                    if (snapDrag != null)
                    {
                        var rect = Marshal.PtrToStructure<WindowExtensions.RECT>(lParam);
                        SnapAndMoveGroup(snapDrag, ref rect);
                        Marshal.StructureToPtr(rect, lParam, false);
                        handled = true;
                        return new IntPtr(1);
                    }
                    break;
                case WM_EXITSIZEMOVE:
                    snapDrag = null;
                    break;
            }
            return IntPtr.Zero;
        }

        private SnapDrag BeginSnapDrag(IntPtr hwnd)
        {
            if (!settings.SnapWindows || !CanSnap || WindowState != System.Windows.WindowState.Normal || !GetWindowRect(hwnd, out var startRect))
            {
                return null;
            }

            var others = new List<SnapWindow>();
            foreach (var window in App.WindowList)
            {
                if (window == this || !window.CanSnap || !window.IsVisible || window.WindowState != System.Windows.WindowState.Normal)
                {
                    continue;
                }

                var other = new SnapWindow { Handle = new WindowInteropHelper(window).Handle };
                if (other.Handle != IntPtr.Zero && GetWindowRect(other.Handle, out other.Rect))
                {
                    others.Add(other);
                }
            }

            // Collect every window touching this one, then every window touching those, and so on.
            var group = new List<SnapWindow>();
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                var toCheck = new Queue<WindowExtensions.RECT>();
                toCheck.Enqueue(startRect);
                while (toCheck.Count > 0)
                {
                    var rect = toCheck.Dequeue();
                    foreach (var attached in others.Where(a => !group.Contains(a) && AreAttached(rect, a.Rect)).ToList())
                    {
                        group.Add(attached);
                        toCheck.Enqueue(attached.Rect);
                    }
                }
            }

            return new SnapDrag
            {
                StartRect = startRect,
                Group = group,
                Targets = others.Except(group).ToList()
            };
        }

        private void SnapAndMoveGroup(SnapDrag drag, ref WindowExtensions.RECT proposed)
        {
            var dx = proposed.Left - drag.StartRect.Left;
            var dy = proposed.Top - drag.StartRect.Top;
            var distance = (int)Math.Round(SnapDistance * VisualTreeHelper.GetDpi(this).DpiScaleX);

            // Any window in the group can snap; the rest of the group keeps its place around it.
            int? snapX = null;
            int? snapY = null;
            foreach (var startRect in new[] { drag.StartRect }.Concat(drag.Group.Select(a => a.Rect)))
            {
                var moved = Offset(startRect, dx, dy);
                foreach (var target in drag.Targets)
                {
                    FindWindowSnap(moved, target.Rect, distance, ref snapX, ref snapY);
                }
                FindScreenSnap(moved, distance, ref snapX, ref snapY);
            }

            dx += snapX ?? 0;
            dy += snapY ?? 0;
            proposed = Offset(proposed, snapX ?? 0, snapY ?? 0);
            foreach (var member in drag.Group)
            {
                _ = SetWindowPos(member.Handle, IntPtr.Zero, member.Rect.Left + dx, member.Rect.Top + dy, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
            }
        }

        // True when a and b sit edge to edge, side by side or stacked, with some of that edge shared.
        private static bool AreAttached(WindowExtensions.RECT a, WindowExtensions.RECT b)
        {
            var sideBySide = (Math.Abs(a.Right - b.Left) <= AttachedTolerance || Math.Abs(a.Left - b.Right) <= AttachedTolerance)
                && a.Top < b.Bottom && a.Bottom > b.Top;
            var stacked = (Math.Abs(a.Bottom - b.Top) <= AttachedTolerance || Math.Abs(a.Top - b.Bottom) <= AttachedTolerance)
                && a.Left < b.Right && a.Right > b.Left;
            return sideBySide || stacked;
        }

        private static void FindWindowSnap(WindowExtensions.RECT moving, WindowExtensions.RECT target, int distance, ref int? snapX, ref int? snapY)
        {
            if (moving.Top < target.Bottom + distance && moving.Bottom > target.Top - distance)
            {
                // Side by side: put the moving window's left or right edge against the target's opposite edge,
                // and once they meet, line up their tops or bottoms too.
                ConsiderSnap(target.Right - moving.Left, distance, ref snapX);
                ConsiderSnap(target.Left - moving.Right, distance, ref snapX);
                if (Math.Abs(target.Right - moving.Left) <= distance || Math.Abs(target.Left - moving.Right) <= distance)
                {
                    ConsiderSnap(target.Top - moving.Top, distance, ref snapY);
                    ConsiderSnap(target.Bottom - moving.Bottom, distance, ref snapY);
                }
            }

            if (moving.Left < target.Right + distance && moving.Right > target.Left - distance)
            {
                // Stacked: the same again for the top and bottom edges.
                ConsiderSnap(target.Bottom - moving.Top, distance, ref snapY);
                ConsiderSnap(target.Top - moving.Bottom, distance, ref snapY);
                if (Math.Abs(target.Bottom - moving.Top) <= distance || Math.Abs(target.Top - moving.Bottom) <= distance)
                {
                    ConsiderSnap(target.Left - moving.Left, distance, ref snapX);
                    ConsiderSnap(target.Right - moving.Right, distance, ref snapX);
                }
            }
        }

        private static void FindScreenSnap(WindowExtensions.RECT moving, int distance, ref int? snapX, ref int? snapY)
        {
            var monitor = MonitorFromRect(ref moving, MONITOR_DEFAULTTONEAREST);
            var monInfo = new WindowExtensions.MONITOR_INFO { cbSize = Marshal.SizeOf(typeof(WindowExtensions.MONITOR_INFO)) };
            if (monitor == IntPtr.Zero || !GetMonitorInfo(monitor, ref monInfo))
            {
                return;
            }

            var work = monInfo.rcWork;
            ConsiderSnap(work.Left - moving.Left, distance, ref snapX);
            ConsiderSnap(work.Right - moving.Right, distance, ref snapX);
            ConsiderSnap(work.Top - moving.Top, distance, ref snapY);
            ConsiderSnap(work.Bottom - moving.Bottom, distance, ref snapY);
        }

        // Keeps the smallest adjustment seen so far that is within snapping distance.
        private static void ConsiderSnap(int adjustment, int distance, ref int? best)
        {
            if (Math.Abs(adjustment) <= distance && (best == null || Math.Abs(adjustment) < Math.Abs(best.Value)))
            {
                best = adjustment;
            }
        }

        private static WindowExtensions.RECT Offset(WindowExtensions.RECT rect, int dx, int dy)
        {
            return new WindowExtensions.RECT
            {
                Left = rect.Left + dx,
                Top = rect.Top + dy,
                Right = rect.Right + dx,
                Bottom = rect.Bottom + dy
            };
        }
    }
}
