using System.Drawing;

namespace ScanOCR.Core.Model
{
    public class InputTensor
    {
        public byte[] Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        
        public InputTensor(Bitmap bitmap)
        {
            Data = GetPixelsWithoutPadding(bitmap);
            Width = bitmap.Width;
            Height = bitmap.Height;
        }

        public InputTensor(string imagePath)
        {
            Bitmap bitmap = new Bitmap(imagePath);
            Data = GetPixelsWithoutPadding(bitmap);
            Width = bitmap.Width;
            Height = bitmap.Height;
        }

        private byte[] GetPixelsWithoutPadding(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            byte[] pixels = new byte[width * height * 3];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color clr = bitmap.GetPixel(x, y);

                    int index = (y * width + x) * 3;
                    pixels[index + 0] = clr.B;
                    pixels[index + 1] = clr.G;
                    pixels[index + 2] = clr.R;
                }
            }

            return pixels;
        }
    }
}
