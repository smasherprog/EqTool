using EQTool.Shapes;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EQTool
{
    public partial class PanAndZoomCanvas : Canvas
    {
        public MatrixTransform Transform = new MatrixTransform();
        private Point _initialMousePosition;

        public PanAndZoomCanvas()
        {
            InitializeComponent();
            MouseDown += PanAndZoomCanvas_MouseDown;
            MouseMove += PanAndZoomCanvas_MouseMove;
            MouseWheel += PanAndZoomCanvas_MouseWheel;
        }

        public void Reset()
        {
            Transform = new MatrixTransform();
            CurrentScaling = 1.0f;
            foreach (UIElement child in Children)
            {
                child.RenderTransform = Transform;
            }
        }

        public float CurrentScaling { get; set; } = 1.0f;
        public float Zoomfactor { get; set; } = 1.1f;

        private void PanAndZoomCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _initialMousePosition = Transform.Inverse.Transform(e.GetPosition(this));
            }
        }


        private void PanAndZoomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = Transform.Inverse.Transform(e.GetPosition(this));
                var delta = Point.Subtract(mousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                Transform.Matrix = translate.Value * Transform.Matrix;
                foreach (UIElement child in Children)
                {
                    if (child is ArrowLine c)
                    {
                        var transform = new MatrixTransform();
                        var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                        transform.Matrix = c.RotateTransform.Value * translation.Value;
                        c.RenderTransform = transform;
                    }
                    else
                    {
                        child.RenderTransform = Transform;
                    }
                }
            }
        }

        private void PanAndZoomCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scaleFactor = Zoomfactor;
            if (e.Delta < 0)
            {
                scaleFactor = 1f / scaleFactor;
            }

            var mousePostion = e.GetPosition(this);

            var scaleMatrix = Transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            if (CurrentScaling * scaleFactor < 1 || CurrentScaling * scaleFactor > 40)
            {
                // dont allow zooming out too far
                return;
            }

            Debug.WriteLine(CurrentScaling);

            Transform.Matrix = scaleMatrix;
            CurrentScaling *= scaleFactor;
            foreach (UIElement child in Children)
            {
                var x = Canvas.GetLeft(child);
                var y = Canvas.GetTop(child);

                var sx = x * scaleFactor;
                var sy = y * scaleFactor;

                Canvas.SetLeft(child, sx);
                Canvas.SetTop(child, sy);
                if (child is ArrowLine c)
                {
                    var transform = new MatrixTransform();
                    var translation = new TranslateTransform(Transform.Value.OffsetX, Transform.Value.OffsetY);
                    transform.Matrix = c.RotateTransform.Value * translation.Value;
                    c.RenderTransform = transform;
                }
                else
                {
                    child.RenderTransform = Transform;
                }
            }
        }
    }
}
