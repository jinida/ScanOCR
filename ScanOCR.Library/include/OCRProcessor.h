#pragma once
#include "ModelFactory.h"
#include "Detector.h"
#include "Recognizer.h"
#include "OCRBox.h"
#include "OCRParams.h"

namespace ScanOCRLib
{
	class OCRProcessor
	{
	public:
		OCRProcessor(OCRParameter params);
		std::vector<ScanOCRLib::OCRBox> inference(unsigned char* inputData, int width, int height);

	private:
		ModelFactory _factory;
		OCRParameter _params;
		Detector _detector;
		Recognizer _recognizer;
	};
}

#ifdef CREATEDLL_EXPORTS
#define DECLSPEC __declspec(dllexport)
#else
#define DECLSPEC __declspec(dllexport)
#endif

extern "C"
{

	DECLSPEC ScanOCRLib::OCRParameter createParameter(float threshold,
		float boxThreshold, std::string detModelPath, std::string recModelPath, std::string dictPath);
	DECLSPEC ScanOCRLib::OCRParameter* createParameterFromC(float threshold, 
		float boxThreshold, const char* detModelPath, const char* recModelPath, const char* dictPath);
	
	DECLSPEC ScanOCRLib::OCRProcessor* createOCRProcessor(ScanOCRLib::OCRParameter* params);
	DECLSPEC std::vector<ScanOCRLib::OCRBox>* inference(ScanOCRLib::OCRProcessor* processor, unsigned char* inputData, int width, int height);
	DECLSPEC ScanOCRLib::OCRBoxCArray inferenceFromC(ScanOCRLib::OCRProcessor* processor, unsigned char* inputData, int width, int height);
	DECLSPEC void releaseParameter(ScanOCRLib::OCRParameter* params);
}

