using ScanOCR.Core.Model;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ScanOCR.Core.Manager
{
    public class ScannerController
    {
        IntPtr _pScannerParams;
        IntPtr _pScanProcessor;
        ScannerParam _scannerParams;
        public event EventHandler loadedProcessor;

        public ScannerController() 
        {
            _scannerParams = new ScannerParam();
            _pScannerParams = createParameterFromC(_scannerParams.RecThreshold, _scannerParams.BoxThreshold, 
                _scannerParams.BoxModelPath, _scannerParams.RecModelPath, _scannerParams.DictPath);
        }

        public async Task loadProcessor()
        {
            _pScanProcessor = await CreateOCRProcessorAsync();
            onLoadedProcessor();
        }

        private async Task<IntPtr> CreateOCRProcessorAsync()
        {
            return await Task.Run(() =>
            {
                return createOCRProcessor(_pScannerParams);
            });
        }
        
        public OCRBoxArray inference(Bitmap bitmap)
        {
            InputTensor inputTensor = new InputTensor(bitmap);
            var result = inferenceFromC(_pScanProcessor, inputTensor.Data, inputTensor.Width, inputTensor.Height);
            return new OCRBoxArray(result);
        }

        private void onLoadedProcessor()
        {
            loadedProcessor?.Invoke(this, EventArgs.Empty);
        }

        [DllImport("ScanOCR.Library.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr createParameterFromC(float threshold, float boxThreshold, string detModelPath, string recModelPath, string dictPath);

        [DllImport("ScanOCR.Library.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr createOCRProcessor(IntPtr pParam);

        [DllImport("ScanOCR.Library.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern OCRBoxCArray inferenceFromC(IntPtr processor, byte[] inputData, int width, int height);
    }
}
