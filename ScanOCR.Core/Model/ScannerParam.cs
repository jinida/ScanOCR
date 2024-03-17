namespace ScanOCR.Core.Model
{
    public struct ScannerParam
    { 
        public float BoxThreshold { get; set; }
        public float RecThreshold { get; set; }
        public string BoxModelPath { get; set; }
        public string RecModelPath { get; set; }
        public string DictPath { get; set; }
        
        public ScannerParam(float boxThreshold, float recThreshold, string boxModelPath, string recModelPath, string dictPath)
        {
            BoxThreshold = boxThreshold;
            RecThreshold = recThreshold;    
            BoxModelPath = boxModelPath;
            RecModelPath = recModelPath;
            DictPath = dictPath;
        }

        public ScannerParam()
        {
            BoxThreshold = 0.3f;
            RecThreshold = 0.6f;
            BoxModelPath = "model_weight\\det\\openvino\\det_model_simple.xml";
            RecModelPath = "model_weight\\rec\\openvino\\rec_model_simple.xml";
            DictPath = "korean_dict.txt";
        }
    }
}
