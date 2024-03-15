using Jamesnet.Wpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ScanOCR.Capture.UI.Views
{
    public class CaptureWindow : JamesWindow
    {
        static CaptureWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CaptureWindow), new FrameworkPropertyMetadata(typeof(CaptureWindow)));
        }

        public CaptureWindow()
        {
        }
    }
}