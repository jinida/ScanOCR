using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class Program
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OCRBoxC
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] box;
        public float detectionScore;
        public IntPtr content;
        public float recognitionScore;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OCRBoxCArray
    {
        public IntPtr boxes;
        public int numBoxes;
    }

    [DllImport("ScanOCR.Library.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr createParameterFromC(float threshold, float boxThreshold, string detModelPath, string recModelPath, string dictPath);

    [DllImport("ScanOCR.Library.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr createOCRProcessor(IntPtr pProcessor);
    [DllImport("ScanOCR.Library.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern OCRBoxCArray inferenceFromC(IntPtr processor, byte[] inputData, int width, int height);
    
    static void Main()
    {
        IntPtr pParams = createParameterFromC(0.3f, 0.6f, "model_weight\\det\\openvino\\det_model_simple.xml",
            "model_weight\\rec\\openvino\\rec_model_simple.xml", "korean_dict.txt");
        string imagePath = "asd.png";

        Bitmap bitmap = new Bitmap(imagePath);
        byte[] rgbValues = GetPixelsWithoutPadding(bitmap);

        IntPtr processor = createOCRProcessor(pParams);
        OCRBoxCArray ocrBoxArr = inferenceFromC(processor, rgbValues, bitmap.Width, bitmap.Height);
        for (int i = 0; i < ocrBoxArr.numBoxes; i++)
        {
            IntPtr boxPtr = new IntPtr(ocrBoxArr.boxes.ToInt64() + Marshal.SizeOf<OCRBoxC>() * i);
            OCRBoxC ocrBox = Marshal.PtrToStructure<OCRBoxC>(boxPtr);

            string content = Marshal.PtrToStringAnsi(ocrBox.content);
            Console.WriteLine($"Box {i}: {content}, Score: {ocrBox.detectionScore}");

            for (int j = 0; j != 6; j+=2)
            {
                Console.Write($"[{ocrBox.box[j]}, {ocrBox.box[j + 1]}], ");
            }
            Console.WriteLine($"[{ocrBox.box[6]}, {ocrBox.box[7]}]");
        }

        DrawBoxesAndSaveImage(bitmap, ocrBoxArr, "result.png");
    }

    public static byte[] GetPixelsWithoutPadding(Bitmap bitmap)
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
    static void DrawBoxesAndSaveImage(Bitmap bitmap, OCRBoxCArray ocrBoxArr, string savePath)
    {
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            Pen pen = new Pen(Color.Red, 3);
            for (int i = 0; i < ocrBoxArr.numBoxes; i++)
            {
                IntPtr boxPtr = new IntPtr(ocrBoxArr.boxes.ToInt64() + Marshal.SizeOf<OCRBoxC>() * i);
                OCRBoxC ocrBox = Marshal.PtrToStructure<OCRBoxC>(boxPtr);

                Point[] points =
                {
                new Point(ocrBox.box[0], ocrBox.box[1]),
                new Point(ocrBox.box[2], ocrBox.box[3]),
                new Point(ocrBox.box[4], ocrBox.box[5]),
                new Point(ocrBox.box[6], ocrBox.box[7])
            };
                graphics.DrawPolygon(pen, points);
            }
        }

        bitmap.Save(savePath, ImageFormat.Png);
    }
}

