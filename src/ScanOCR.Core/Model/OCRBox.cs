using System.Runtime.InteropServices;

namespace ScanOCR.Core.Model
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

    public struct OCRBoxArray
    {
        public int numBoxes;
        public string[] contents;
        public int[][] boxes;
        public float[] detScores;
        public float[] recScores;

        public OCRBoxArray(OCRBoxCArray ocrBoxArray)
        {
            numBoxes = ocrBoxArray.numBoxes;
            contents = new string[numBoxes];
            detScores = new float[numBoxes];
            recScores = new float[numBoxes];
            boxes = new int[numBoxes][];

            for (int i = 0; i < ocrBoxArray.numBoxes; i++)
            {
                IntPtr boxPtr = new IntPtr(ocrBoxArray.boxes.ToInt64() + Marshal.SizeOf<OCRBoxC>() * i);
                OCRBoxC ocrBox = Marshal.PtrToStructure<OCRBoxC>(boxPtr);
                
                boxes[i] = new int[8];
                contents[i] = Marshal.PtrToStringUni(ocrBox.content);
                detScores[i] = ocrBox.detectionScore;
                recScores[i] = ocrBox.recognitionScore;
                for (int j = 0; j != 8; j += 2)
                {
                    boxes[i][j] = ocrBox.box[j];
                    boxes[i][j + 1] = ocrBox.box[j + 1];
                }
            }
        }
    }
}
