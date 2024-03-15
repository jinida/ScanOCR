using System.Windows;

namespace CaptureWindow.UI.Views
{
    public class CaptureWindow : Window
    {
        static CaptureWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptureWindow), new FrameworkPropertyMetadata(typeof(CaptureWindow)));
        }

        public CaptureWindow()
        {
            ShowInTaskbar = false;
            Focusable = false;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
            Topmost = true;
        }
    }
}