using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ScanOCR.Core.Model;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media.Converters;
using System.Security.Cryptography.Xml;

namespace ScanOCR.LayoutSupport.UI.Units
{
    public class OCRInfo
    {
        public string Content { get; set; }
        public float DetScore { get; set; }
        public float RecScore { get; set; }
        public bool IsControlVisible { get; set; } = false;
    }

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

        public static readonly DependencyProperty BoxesProperty =
            DependencyProperty.Register(
                nameof(OCRBoxes),
                typeof(OCRBoxArray),
                typeof(ImageCanvas),
                new PropertyMetadata(new OCRBoxArray(), OnBoxesChanged));


        public OCRBoxArray OCRBoxes
        {
            get => (OCRBoxArray) GetValue(BoxesProperty);
            set => SetValue(BoxesProperty, value);
        }

        public Bitmap CanvasBitmap
        {
            get => (Bitmap)GetValue(CanvasBitmapProperty);
            set => SetValue(CanvasBitmapProperty, value);
        }

        private MatrixTransform _transform;

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
                canvas._transform = new MatrixTransform();
                
                canvas.AdjustImageTransform();
            }
        }

        private static void OnBoxesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageCanvas canvas && e.NewValue is OCRBoxArray boxes)
            {
                var toRemove = canvas.Children.OfType<Polygon>().ToList();
                foreach (var polygon in toRemove)
                {
                    canvas.Children.Remove(polygon);
                }

                var toRemoveTextBox = canvas.Children.OfType<TextBox>().ToList();
                foreach (var textBox in toRemoveTextBox)
                {
                    canvas.Children.Remove(textBox);
                }

                for (int i = 0; i < boxes.numBoxes; i++)
                {
                    Polygon polygon = new Polygon
                    {
                        Stroke = System.Windows.Media.Brushes.Red,
                        StrokeThickness = 0.51,
                        Points = new PointCollection(),
                        Fill = System.Windows.Media.Brushes.Transparent,
                        Tag = new OCRInfo{ Content = boxes.contents[i], DetScore = boxes.detScores[i], RecScore = boxes.recScores[i] }
                    };

                    var box = boxes.boxes[i];

                    for (int j = 0; j != box.Length; j += 2)
                    {
                        int x = box[j];
                        int y = box[j + 1];
                        polygon.Points.Add(new System.Windows.Point(x, y));
                    }

                    polygon.MouseLeftButtonDown += Polygon_MouseLeftButtonDown;
                    polygon.RenderTransform = canvas._transform;
                    canvas.Children.Add(polygon);
                }
            }
        }

        private static void Polygon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Polygon polygon && polygon.Tag is OCRInfo ocrInfo)
            {
                Canvas canvas = (Canvas)polygon.Parent;
                if (ocrInfo.IsControlVisible)
                {
                    var controlsToRemove = canvas.Children.OfType<FrameworkElement>().Where(x => x.Tag == polygon).ToList();
                    foreach (var control in controlsToRemove)
                    {
                        canvas.Children.Remove(control);
                    }
                    ocrInfo.IsControlVisible = false;
                }
                else
                {
                    TextBox textBox = new TextBox
                    {
                        Text = ocrInfo.Content,
                        IsReadOnly = true,
                        Tag = polygon,
                        FontWeight = FontWeights.Bold,
                        FontSize = 15,
                        Padding = new Thickness(2)
                    };

                    double minX = polygon.Points.Min(p => p.X);
                    double minY = polygon.Points.Min(p => p.Y);

                    var m11 = polygon.RenderTransform.Value.M11;
                    var m12 = polygon.RenderTransform.Value.M12;
                    var m21 = polygon.RenderTransform.Value.M21;
                    var m22 = polygon.RenderTransform.Value.M22;
                    var offsetX = polygon.RenderTransform.Value.OffsetX;
                    var offsetY = polygon.RenderTransform.Value.OffsetY;

                    minX = m11 * minX + m12 * minY + offsetX;
                    minY = m21 * minX + m22 * minY + offsetY;

                    SetLeft(textBox, minX);
                    SetTop(textBox, minY - 25);

                    canvas.Children.Add(textBox);
                    ocrInfo.IsControlVisible = true;
                }
            }
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustImageTransform();

            var polygons = Children.OfType<Polygon>().ToList();
            foreach (var polygon in polygons)
            {
                if(polygon.Tag is OCRInfo ocrInfo)
                {
                    ocrInfo.IsControlVisible = false;
                }
            }

            var toRemoveTextBox = Children.OfType<TextBox>().ToList();
            foreach (var textBox in toRemoveTextBox)
            {
                Children.Remove(textBox);
            }
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
                _transform = transform;
                AdjustPolygonTransform();
            }
        }

        private void AdjustPolygonTransform()
        {
            var polygons = Children.OfType<Polygon>().ToList();
            if (polygons != null)
            {
                foreach (var polygon in polygons)
                {
                    polygon.RenderTransform = _transform;
                }
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