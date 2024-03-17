using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using Prism.Regions;
using ScanOCR.Capture.UI.Views;
using ScanOCR.Core.Manager;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScanOCR.Forms.Local.ViewModels
{
    public partial class LoadingWindowViewModel : ObservableObject
    {
        private readonly IRegionManager _regionManager;
        private readonly IContainerProvider _containerProvider;
        private readonly ScannerController _scannerController;

        public LoadingWindowViewModel(IRegionManager regionManager, IContainerProvider containerProvider, ScannerController _scannerController)
        {
            _ = _scannerController.loadProcessor();
        }
    }
}
