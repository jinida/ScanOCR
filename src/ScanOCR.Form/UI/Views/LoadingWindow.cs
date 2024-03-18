using Jamesnet.Wpf.Controls;
using System.Windows;

namespace ScanOCR.Forms.UI.Views
{
    public class LoadingWindow : JamesWindow
    {
        static LoadingWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingWindow), new FrameworkPropertyMetadata(typeof(LoadingWindow)));
        }

        public LoadingWindow()
        {
            Width = 200;
            Height = 100;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

    }
}