#pragma once
#include "IModel.h"
#include "OCRBox.h"
#include "opencv2/opencv.hpp"
#include <algorithm>
#include <numeric>
#include <fstream>

namespace ScanOCRLib
{
	struct RecognizeParameter
	{
		std::string dictPath;
		std::string modelPath;
		std::vector<float> mean{ 0.5f, 0.5f, 0.5f };
		std::vector<float> std{ 0.5f, 0.5f, 0.5f };
		int minHeight = 48;
	};

	class Recognizer
	{
	public:
		Recognizer()
			:_pModel(nullptr) {}
		Recognizer(IModel* pModel, RecognizeParameter params);
		~Recognizer() { }
		std::vector<OCRBox> inference(cv::Mat& srcImage, std::vector<OCRBox>& ocrBoxes);
		std::vector<size_t> getOutShape() { return _pModel->getOutputShape(); }

	private:

		cv::Mat getRoateCropImage(const cv::Mat& srcImage, const OCRBox ocrBox);
		void setInputShape(std::vector<size_t>& inputShape) { return _pModel->setInputShape(inputShape); }
		std::unique_ptr<float[]> getPreprocessImage(cv::Mat& srcImage);
		std::pair<std::string, float> getPostprocessImage(const float* pData);

	private:
		IModel* _pModel;
		RecognizeParameter _params;
		std::vector<std::string> _characters;
	};
}

