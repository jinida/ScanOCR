using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Ioc;
using Prism.Regions;
using ScanOCR.Capture.UI.Views;
using ScanOCR.Core.Manager;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScanOCR.Forms.Local.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IRegionManager _regionManager;
        private readonly IContainerProvider _containerProvider;
        private readonly CaptureManager _captureManager;

        private bool _isDrawing = false;
        private Border _currentRect;
        private System.Windows.Point _startPoint;
        private System.Windows.Point _endPoint;
        private Canvas? _canvas;
        BitmapImage _image;

        [ObservableProperty]
        private Bitmap _captureImage;

        [ObservableProperty]
        private bool _mode;

        [ObservableProperty]
        private WindowState _isMinimize;

        public MainWindowViewModel(IRegionManager regionManager, IContainerProvider containerProvider, CaptureManager captureManager)
        {
            _regionManager = regionManager;
            _containerProvider = containerProvider;
            _captureManager = captureManager;
            IsMinimize = WindowState.Normal;
        }

        [RelayCommand]
        public async Task Append()
        {
            if (!_mode)
            {
                IsMinimize = WindowState.Minimized;
                await Task.Delay(500);
                SetupCaptureWindow();
            }
        }

        private void SetupCaptureWindow()
        {
            var captureWindow = _captureManager.ResolveWindow<CaptureWindow>();
            captureWindow.WindowStyle = WindowStyle.None;
            captureWindow.AllowsTransparency = true;
            _captureManager.ShowPicker();

            _canvas = captureWindow.Template.FindName("PART_Canvas", captureWindow) as Canvas;
            if (_canvas != null)
            {
                _canvas.Children.Clear();
                _canvas.Children.Add(CaptureScreenToImageControl());
                RegisterCanvasEvents();
            }
        }

        private void RegisterCanvasEvents()
        {
            _canvas.MouseDown += Canvas_MouseDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseUp += Canvas_MouseUp;
        }

        private void UnregisterCanvasEvents()
        {
            if (_canvas != null)
            {
                _canvas.MouseDown -= Canvas_MouseDown;
                _canvas.MouseMove -= Canvas_MouseMove;
                _canvas.MouseUp -= Canvas_MouseUp;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing)
            {
                _startPoint = e.GetPosition(_canvas);
                _currentRect = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(2)
                };
                Canvas.SetLeft(_currentRect, _startPoint.X);
                Canvas.SetTop(_currentRect, _startPoint.Y);
                _canvas.Children.Add(_currentRect);
                _isDrawing = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                var currentPoint = e.GetPosition(_canvas);
                var width = currentPoint.X - _startPoint.X;
                var height = currentPoint.Y - _startPoint.Y;

                _currentRect.Width = Math.Abs(width);
                _currentRect.Height = Math.Abs(height);
                Canvas.SetLeft(_currentRect, width > 0 ? _startPoint.X : currentPoint.X);
                Canvas.SetTop(_currentRect, height > 0 ? _startPoint.Y : currentPoint.Y);
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing)
            {
                _endPoint = e.GetPosition(_canvas);
                _isDrawing = false;
                IsMinimize = WindowState.Normal;
                
                CaptureImage = CropBitmap(BitmapImage2Bitmap(_image), _startPoint, _endPoint);
                UnregisterCanvasEvents();
                _captureManager.UnregisterPicker();
            }
        }

        public System.Windows.Controls.Image CaptureScreenToImageControl()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            using (var bmp = new Bitmap((int)screenWidth, (int)screenHeight))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                }

                var bitmapImage = new BitmapImage();
                using (var memory = new MemoryStream())
                {
                    bmp.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }
                
                _image = bitmapImage;

                return new System.Windows.Controls.Image { Source = bitmapImage, Stretch = Stretch.Uniform };
            }
        }

        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (var outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        private Bitmap CropBitmap(Bitmap source, System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            int x = Math.Min((int)startPoint.X, (int)endPoint.X);
            int y = Math.Min((int)startPoint.Y, (int)endPoint.Y);
            int width = Math.Abs((int)endPoint.X - (int)startPoint.X);
            int height = Math.Abs((int)endPoint.Y - (int)startPoint.Y);

            x = Math.Max(x, 0);
            y = Math.Max(y, 0);
            width = Math.Min(width, source.Width - x);
            height = Math.Min(height, source.Height - y);

            Rectangle cropArea = new Rectangle(x, y, width, height);

            // 안전하게 cropArea가 source 이미지 내에 있는지 확인 후 클론
            if (width > 0 && height > 0 && cropArea.Right <= source.Width && cropArea.Bottom <= source.Height)
            {
                return source.Clone(cropArea, source.PixelFormat);
            }
            else
            {
                throw new ArgumentException("Crop area is out of the source image bounds.");
            }
        }
    }
}
