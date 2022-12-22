using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;

namespace EQTool.Services
{
    public static class WindowBounds
    {
        public static bool isPointVisibleOnAScreen(Rect p)
        {
            if (!isPointVisibleOnAScreen(p.TopLeft)) return false;
            if (!isPointVisibleOnAScreen(p.BottomRight)) return false;
            return true;
        }

        public static bool isPointVisibleOnAScreen(Point p)
        {
            foreach (Screen s in Screen.AllScreens)
            {
                if (p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
                    return true;
            }
            return false;
        }
    }
}
