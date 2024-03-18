using Jamesnet.Wpf.Global.Location;
using ScanOCR.Forms.Local.ViewModels;
using ScanOCR.Forms.UI.Views;

namespace ScanOCR.Settings
{
    internal class WireDataContext : ViewModelLocationScenario
    {
        protected override void Match(ViewModelLocatorCollection items)
        {
            items.Register<MainWindow, MainWindowViewModel>();
            items.Register<LoadingWindow, LoadingWindowViewModel>();
        }
    }
}
