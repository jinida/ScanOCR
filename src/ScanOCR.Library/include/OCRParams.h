#pragma once
#include <vector>
#include <string>

namespace ScanOCRLib
{
	struct OCRParameter
	{
		std::string modelType;

		std::string detModelPath;
		std::string detResizeMethod;
		int maxSide;
		double threshold;
		double maxValue;
		float boxThreshold;
		float unclipRatio;

		std::vector<float> detectorMean{ 0.485f, 0.456f, 0.406f };
		std::vector<float> detectorStd{ 0.229f, 0.224f, 0.225f };
		int minSize = 5;

		std::string dictPath;
		std::string recModelPath;
		std::vector<float> recognizerMean{ 0.5f, 0.5f, 0.5f };
		std::vector<float> recognizerStd{ 0.5f, 0.5f, 0.5f };
		int minHeight = 48;

		DetectParameter toDetectParameter() const
		{
			DetectParameter param;
			param.modelPath = this->detModelPath;
			param.resizeMethod = this->detResizeMethod;
			param.maxSide = this->maxSide;
			param.threshold = this->threshold;
			param.maxValue = this->maxValue;
			param.boxThreshold = this->boxThreshold;
			param.unclipRatio = this->unclipRatio;
			param.mean = this->detectorMean;
			param.std = this->detectorStd;
			param.minSize = this->minSize;
			param.maxCandidates = 1000;
			return param;
		}

		RecognizeParameter toRecognizeParameter() const
		{
			RecognizeParameter param;
			param.modelPath = this->recModelPath;
			param.dictPath = this->dictPath;
			param.minHeight = this->minHeight;
			param.mean = this->recognizerMean;
			param.std = this->recognizerStd;
			return param;
		}
	};
}