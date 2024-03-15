using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ScanOCR.LayoutSupport.UI.Units
{
    public class ImageCanvas : Canvas
    {
        static ImageCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageCanvas), new FrameworkPropertyMetadata(typeof(ImageCanvas)));
        }

        private System.Windows.Controls.Image _canvasImage;

        public static readonly DependencyProperty CanvasBitmapProperty =
            DependencyProperty.Register(
                nameof(CanvasBitmap),
                typeof(Bitmap),
                typeof(ImageCanvas),
                new PropertyMetadata(null, OnBitmapChanged));

        public Bitmap CanvasBitmap
        {
            get => (Bitmap)GetValue(CanvasBitmapProperty);
            set => SetValue(CanvasBitmapProperty, value);
        }

        public ImageCanvas()
        {
            _canvasImage = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Uniform
            };
            Children.Add(_canvasImage);
            SizeChanged += Canvas_SizeChanged;
        }

        private static void OnBitmapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageCanvas canvas && e.NewValue is Bitmap bitmap)
            {
                canvas._canvasImage.Source = ConvertBitmapToImageSource(bitmap);
                canvas.AdjustImageTransform();
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustImageTransform();
        }

        private void AdjustImageTransform()
        {
            if (_canvasImage.Source != null && _canvasImage.Source is BitmapSource bitmapSource)
            {
                double scaleX = ActualWidth / bitmapSource.PixelWidth;
                double scaleY = ActualHeight / bitmapSource.PixelHeight;
                double scale = Math.Min(scaleX, scaleY);

                double offsetX = (ActualWidth - bitmapSource.PixelWidth * scale) / 2;
                double offsetY = (ActualHeight - bitmapSource.PixelHeight * scale) / 2;

                MatrixTransform transform = new MatrixTransform
                {
                    Matrix = new Matrix(scale, 0, 0, scale, offsetX, offsetY)
                };

                _canvasImage.RenderTransform = transform;
            }
        }

        private static ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
    }
}