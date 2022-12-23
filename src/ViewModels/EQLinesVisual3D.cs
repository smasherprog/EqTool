using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace EQTool.ViewModels
{
    public class EQLinesVisual3D : LinesVisual3D
    {
        public List<Point3D> OriginalPoints { get; internal set; }
    }
}
