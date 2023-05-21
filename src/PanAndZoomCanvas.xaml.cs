using EQTool.Models;
using EQTool.Shapes;
using EQTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static EQTool.Services.MapLoad;

namespace EQTool
{
    public partial class PanAndZoomCanvas : Canvas
    {
        public MatrixTransform Transform = new MatrixTransform();
        private Point _initialMousePosition;
        private readonly PanAndZoomViewModel panAndZoomViewModel;
        private readonly List<MapWidget> MapWidgets = new List<MapWidget>();
        private bool _dragging;
        private UIElement _selectedElement;
        private Vector _draggingDelta;

        public PanAndZoomCanvas()
        {
            InitializeComponent();
            DataContext = panAndZoomViewModel = new PanAndZoomViewModel(Children);
            MouseDown += PanAndZoomCanvas_MouseDown;
            MouseUp += PanAndZoomCanvas_MouseUp;
            MouseMove += PanAndZoomCanvas_MouseMove;
            MouseWheel += PanAndZoomCanvas_MouseWheel;
        }

        private bool IsNumericInput(string text)
        {
            foreach (var c in text)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsNumericInput(e.Text))
            {
                e.Handled = true;
                return;
            }
        }
        private void AddTimer(object sender, RoutedEventArgs e)
        {
            var w = new MapWidget()
            {
                Width = 200,
                Height = 150
            };
            MapWidgets.Add(w);
            _ = Children.Add(w);
            w.Header.Text = "Widget 1";
            Canvas.SetTop(w, 100);
            Canvas.SetLeft(w, 100);
            TimerMenu.IsOpen = false;
        }

        private void DeleteTimer(object sender, RoutedEventArgs e)
        {
            if (_selectedElement is MapWidget w)
            {
                _ = MapWidgets.Remove(w);
                Children.Remove(w);
                _dragging = false;
                _selectedElement = null;
            }
        }

        public void Reset(double dims)
        {
            MaxDims = dims;
            Transform = new MatrixTransform();
            CurrentScaling = 1.0f;
            foreach (UIElement child in Children)
            {
                child.RenderTransform = Transform;
            }
        }

        private double MaxDims { get; set; } = 1.0f;

        public float CurrentScaling { get; set; } = 1.0f;
        public float Zoomfactor { get; set; } = 1.1f;
        public bool TimerOpen { get; private set; }

        private void PanAndZoomCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (e.Source is MapWidget)
                {
                    _selectedElement = (UIElement)e.Source;
                    AddTimerMenuItem.Visibility = Visibility.Collapsed;
                    DeleteTimerMenuItem.Visibility = Visibility.Visible;
                }
                else
                {
                    _selectedElement = null;
                    AddTimerMenuItem.Visibility = Visibility.Visible;
                    DeleteTimerMenuItem.Visibility = Visibility.Collapsed;
                }
            }
            if (TimerOpen)
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.Source is MapWidget)
                {
                    _selectedElement = (UIElement)e.Source;
                    var mousePosition = Mouse.GetPosition(this);
                    var x = Canvas.GetLeft(_selectedElement);
                    var y = Canvas.GetTop(_selectedElement);
                    var elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePosition;
                    _dragging = true;
                }
                else
                {
                    _dragging = false;
                    _selectedElement = null;
                }
            }

            if (!_dragging && e.ChangedButton == MouseButton.Left)
            {
                _initialMousePosition = Transform.Inverse.Transform(e.GetPosition(this));
            }
        }

        private void PanAndZoomCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
        }

        public void MoveMap(int x, int y)
        {
            var translate = new TranslateTransform(x, y);
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

        private void PanAndZoomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (TimerOpen)
            {
                return;
            }

            if (!_dragging && e.LeftButton == MouseButtonState.Pressed)
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

            if (_dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var x = Mouse.GetPosition(this).X;
                var y = Mouse.GetPosition(this).Y;

                if (_selectedElement != null)
                {
                    Canvas.SetLeft(_selectedElement, x + _draggingDelta.X);
                    Canvas.SetTop(_selectedElement, y + _draggingDelta.Y);
                }
            }
        }
        private static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            return val.CompareTo(min) < 0 ? min : val.CompareTo(max) > 0 ? max : val;
        }

        private void PanAndZoomCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (TimerOpen)
            {
                return;
            }

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

            Transform.Matrix = scaleMatrix;
            CurrentScaling *= scaleFactor;
            Debug.WriteLine(CurrentScaling);
            var currentlabelscaling = (CurrentScaling / 40 * -1) + 1;
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
                else if (child is TextBlock t)
                {
                    var textdata = t.Tag as MapLabel;
                    if (textdata.LabelSize == LabelSize.Large)
                    {
                        var largescaling = MathHelper.ChangeRange(MaxDims, 500, 35000, 40, 120);
                        largescaling *= currentlabelscaling;
                        largescaling = (int)Clamp(largescaling, 5, 200);
                        if (t.FontSize != largescaling)
                        {
                            t.FontSize = largescaling;
                        }
                    }
                    else
                    {
                        var smallscaling = MathHelper.ChangeRange(MaxDims, 500, 35000, 20, 80);
                        smallscaling *= currentlabelscaling;
                        smallscaling = (int)Clamp(smallscaling, 5, 100);
                        if (t.FontSize != smallscaling)
                        {
                            t.FontSize = smallscaling;
                        }
                    }
                }
                else
                {
                    child.RenderTransform = Transform;
                }
            }
        }

        private void TimerMenu_Closed(object sender, RoutedEventArgs e)
        {
            TimerOpen = false;
        }

        private void TimerMenu_Opened(object sender, RoutedEventArgs e)
        {
            TimerOpen = true;
        }
    }
}
