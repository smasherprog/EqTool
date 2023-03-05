using System.Windows;
using System.Windows.Forms;

namespace EQTool.Services
{
    public static class WindowBounds
    {
        public static bool isPointVisibleOnAScreen(Rect? p)
        {
            if (!p.HasValue)
            {
                return false;
            }

            if (!isPointVisibleOnAScreen(p.Value.TopLeft))
            {
                return false;
            }

            return isPointVisibleOnAScreen(p.Value.BottomRight);
        }

        public static bool isPointVisibleOnAScreen(Point p)
        {
            foreach (var s in Screen.AllScreens)
            {
                if (p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
