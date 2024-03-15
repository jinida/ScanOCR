using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using Prism.Regions;
using ScanOCR.Core.Manager;
using System.Drawing;
using ScanOCR.Capture.UI.Views;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScanOCR.Forms.Local.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IRegionManager _regionManager;
        private readonly IContainerProvider _containerProvider;
        private readonly CaptureManager _captureManager;

        [ObservableProperty]
        private Bitmap _captureImage;

        [ObservableProperty]
        private bool _mode;

        public MainWindowViewModel(IRegionManager regionManager, IContainerProvider containerProvider, CaptureManager captureManager)
        {
            _regionManager = regionManager;
            _containerProvider = containerProvider;
            _captureManager = captureManager;
        }

        [RelayCommand]
        private void Append(object obj)
        {
            if (_mode == false)
            {
                var captureWindow = _captureManager.ResolveWindow<CaptureWindow>();
                captureWindow.WindowStyle = System.Windows.WindowStyle.None;
                captureWindow.AllowsTransparency = true;

                _captureManager.ShowPicker();
                Canvas canvas = captureWindow.Template.FindName("PART_Canvas", captureWindow) as Canvas;
                canvas.Children.Add(CaptureScreenToImageControl());
                // 윈도우 매니저를 통해 메인 윈도우와 캡처 윈도우를 히든 처리하고 캡처하는 로직 구현
            }
        }

        public System.Windows.Controls.Image CaptureScreenToImageControl()
        {
            var screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            var screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            using (var bmp = new Bitmap((int)screenWidth, (int)screenHeight))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }

                using (var memory = new MemoryStream())
                {
                    bmp.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    var imageControl = 
                        new System.Windows.Controls.Image();
                    imageControl.Source = bitmapImage;
                    imageControl.Stretch = Stretch.Uniform;

                    return imageControl;
                }
            }
        }
    }
}
