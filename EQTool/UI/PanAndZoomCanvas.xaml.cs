using System;
using System.Windows;
using System.Windows.Controls;

namespace EQTool.UI
{
    public partial class PanAndZoomCanvas : Canvas
    {
        public event EventHandler<EventArgs> CancelTimerEvent;

        public string ZoneName = "freportw";

        public PanAndZoomCanvas()
        {
            InitializeComponent();
            ContextMenuOpening += TimerMenu_ContextMenuOpening1;
        }

        private void TimerMenu_ContextMenuOpening1(object sender, ContextMenuEventArgs e)
        {
            if (e.Source is MapWidget)
            {
                DeleteTimerMenuItem.Visibility = Visibility.Visible;
            }
            else
            {
                TimerMenu.IsOpen = false;
                DeleteTimerMenuItem.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void DeleteTimer(object sender, RoutedEventArgs e)
        {
            CancelTimerEvent?.Invoke(this, new EventArgs());
        }

        public event EventHandler<RoutedEventArgs> TimerMenu_ClosedEvent;
        private void TimerMenu_Closed(object sender, RoutedEventArgs e)
        {
            TimerMenu_ClosedEvent?.Invoke(this, e);
        }

        public event EventHandler<RoutedEventArgs> TimerMenu_OpenedEvent;
        private void TimerMenu_Opened(object sender, RoutedEventArgs e)
        {
            TimerMenu_OpenedEvent?.Invoke(this, e);
        }
    }
}
