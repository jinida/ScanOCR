#include "pch.h"
#include "OCRProcessor.h"

ScanOCRLib::OCRProcessor::OCRProcessor(ScanOCRLib::OCRParameter params)
{
	_params = params;
	_factory.build(_params.modelType);

	auto detectModel = _factory.makeModel();
	_detector = Detector(detectModel.release(), _params.toDetectParameter());
	auto recognizeModel = _factory.makeModel();
	_recognizer = Recognizer(recognizeModel.release(), _params.toRecognizeParameter());
}

std::vector<ScanOCRLib::OCRBox> ScanOCRLib::OCRProcessor::inference(unsigned char* inputData, int width, int height)
{
	cv::Mat srcImage(height, width, CV_8UC3, inputData);

	auto detectData = _detector.inference(srcImage, width, height);
	auto recognizeData = _recognizer.inference(srcImage, detectData);

	return recognizeData;
}

DECLSPEC ScanOCRLib::OCRParameter createParameter(float threshold,
	float boxThreshold, std::string detModelPath, std::string recModelPath, std::string dictPath)
{
	ScanOCRLib::OCRParameter params;
	params.boxThreshold = boxThreshold;
	params.detModelPath = detModelPath;
	params.dictPath = dictPath;
	params.detResizeMethod = "max";
	params.maxSide = 960;
	params.maxValue = 255.0;
	params.minHeight = 48;
	params.modelType = "OpenVino";
	params.recModelPath = recModelPath;
	params.unclipRatio = 1.5;
	params.threshold = threshold;
	return params;
}

DECLSPEC ScanOCRLib::OCRProcessor* createOCRProcessor(ScanOCRLib::OCRParameter* params)
{
	return new ScanOCRLib::OCRProcessor(*params);
}

DECLSPEC ScanOCRLib::OCRParameter* createParameterFromC(float threshold, float boxThreshold, const char* detModelPath, const char* recModelPath, const char* dictPath)
{
	ScanOCRLib::OCRParameter* params = new ScanOCRLib::OCRParameter();
	params->boxThreshold = boxThreshold;
	params->detModelPath = std::string(detModelPath);
	params->recModelPath = std::string(recModelPath);
	params->dictPath = std::string(dictPath);
	params->maxSide = 960;
	params->maxValue = 255.0;
	params->minHeight = 48;
	params->modelType = "OpenVino";
	params->unclipRatio = 1.5;
	params->threshold = threshold;
	params->detResizeMethod = "max";
	params->maxSide = 960;

	return params;
}

DECLSPEC std::vector<ScanOCRLib::OCRBox>* inference(ScanOCRLib::OCRProcessor* processor, unsigned char* inputData, int width, int height)
{
	return new std::vector<ScanOCRLib::OCRBox>(processor->inference(inputData, width, height));
}

DECLSPEC ScanOCRLib::OCRBoxCArray inferenceFromC(ScanOCRLib::OCRProcessor* processor, unsigned char* inputData, int width, int height)
{
	auto inferenceResults = processor->inference(inputData, width, height);
	int numBoxes = inferenceResults.size();
	ScanOCRLib::OCRBoxCArray output;
	output.boxes = new ScanOCRLib::OCRBoxC[numBoxes];
	output.numBoxes = numBoxes;

	for (int i = 0; i != numBoxes; ++i)
	{
		output.boxes[i] = inferenceResults[i].toOCRBoxC();
	}

	return output;
}

DECLSPEC void releaseParameter(ScanOCRLib::OCRParameter* params)
{
	delete params;
}

DECLSPEC void deleteOCRBoxContent(ScanOCRLib::OCRBoxC* box)
{
	delete[] box->content;
}