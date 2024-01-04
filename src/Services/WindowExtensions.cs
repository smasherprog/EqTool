using System.Windows;

namespace EQTool.Services
{
    public static class WindowExtensions
    {
        public static void AdjustWindow(EQTool.Models.WindowState windowState, Window w)
        {
            w.Topmost = true;
            if (WindowBounds.isPointVisibleOnAScreen(windowState.WindowRect))
            {
                w.Left = windowState.WindowRect.Value.Left;
                w.Top = windowState.WindowRect.Value.Top;
                w.Height = windowState.WindowRect.Value.Height;
                w.Width = windowState.WindowRect.Value.Width;
                w.WindowState = windowState.State;
            }
        }

        public static void SaveWindowState(EQTool.Models.WindowState windowState, Window w)
        {
            windowState.WindowRect = new Rect
            {
                X = w.Left,
                Y = w.Top,
                Height = w.Height,
                Width = w.Width
            };
            windowState.State = w.WindowState;
        }
    }
}
