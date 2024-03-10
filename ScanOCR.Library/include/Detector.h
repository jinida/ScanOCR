#pragma once
#include "IModel.h"
#include "OCRBox.h"
#include "opencv2/opencv.hpp"
#include <algorithm>
#include <numeric>

namespace ScanOCRLib
{
	struct DetectParameter
	{
		std::string modelPath;
		std::string resizeMethod;
		int maxSide;
		double threshold;
		double maxValue;
		float boxThreshold;
		float unclipRatio;
		std::vector<float> mean{ 0.485f, 0.456f, 0.406f };
		std::vector<float> std{ 0.229f, 0.224f, 0.225f };
		int minSize = 5;
		int maxCandidates = 1000;
	};

	class Detector
	{
	public:
		Detector()
			:_pModel(nullptr) {}
		Detector(IModel* pModel, DetectParameter params);

		std::vector<OCRBox> inference(const cv::Mat& srcImage, int width, int height);
		std::vector<size_t> getOutShape() { return _pModel->getOutputShape(); }

	private:
		cv::RotatedRect unclip(std::vector<std::pair<float, float>> box);
		std::vector<std::pair<float, float>> getMiniBox(cv::RotatedRect& box);
		float getBoxScore(std::vector<std::pair<float, float>> boxArray, const cv::Mat& prediction);
		float getContourArea(std::vector<std::pair<float, float>> box);
		void setInputShape(std::vector<size_t>& inputShape) { return _pModel->setInputShape(inputShape); }
		std::unique_ptr<float[]> getPreprocessImage(const cv::Mat& srcImage);
		std::vector<OCRBox> getPostprocessImage(const float* pData, std::pair<int, int> inputShape);

	private:
		IModel* _pModel;
		DetectParameter _params;
	};
}
