#include "pch.h"
#include "Recognizer.h"

ScanOCRLib::Recognizer::Recognizer(IModel* pModel, RecognizeParameter params)
{
	_pModel = pModel;
	_params = params;

	if (!_pModel->build(_params.modelPath)) {
		std::cerr << "Model build failed." << std::endl;
		exit(0);
	}
	
	std::ifstream file(_params.dictPath);
	std::string line;
	if (file.is_open())
	{
		std::locale::global(std::locale("korean"));
		while (std::getline(file, line))
		{
			_characters.push_back(line);
		}
		file.close();
	}
	else 
	{
		std::cout << "no such label file: " << _params.dictPath << ", exit the program..." << std::endl;
		exit(0);
	}

	_characters.push_back(" ");
	_characters.push_back(" ");
}

cv::Mat ScanOCRLib::Recognizer::getRoateCropImage(const cv::Mat& srcImage, const OCRBox ocrBox)
{
	std::vector<int> xCoords, yCoords;
	for (const auto& point : ocrBox.box) {
		xCoords.push_back(point.first);
		yCoords.push_back(point.second);
	}

	int left = *std::min_element(xCoords.begin(), xCoords.end());
	int right = *std::max_element(xCoords.begin(), xCoords.end());
	int top = *std::min_element(yCoords.begin(), yCoords.end());
	int bottom = *std::max_element(yCoords.begin(), yCoords.end());

	cv::Rect cropRect(left, top, right - left, bottom - top);
	cv::Mat cropImage = srcImage(cropRect);
	
	std::vector<cv::Point2f> pointsf(4);
	for (int i = 0; i != 4; ++i) 
	{
		pointsf[i] = cv::Point2f(static_cast<float>(ocrBox.box[i].first - left), static_cast<float>(ocrBox.box[i].second - top));
	}

	float width = std::hypotf(pointsf[0].x - pointsf[1].x, pointsf[0].y - pointsf[1].y);
	float height = std::hypotf(pointsf[0].x - pointsf[3].x, pointsf[0].y - pointsf[3].y);
	
	std::vector<cv::Point2f> ptsStd{
		cv::Point2f(0.f, 0.f),
		cv::Point2f(width, 0.f),
		cv::Point2f(width, height),
		cv::Point2f(0.f, height)
	};

	cv::Mat tranformMatrix = cv::getPerspectiveTransform(pointsf, ptsStd);
	cv::Mat dstImage;
	cv::warpPerspective(cropImage, dstImage, tranformMatrix, cv::Size(static_cast<int>(width), static_cast<int>(height)), cv::BORDER_REPLICATE);

	if (dstImage.rows >= dstImage.cols * 1.5) 
	{
		cv::Mat srcCopy;
		cv::transpose(dstImage, srcCopy);
		cv::flip(srcCopy, srcCopy, 0);
		return srcCopy;
	}
	else 
	{
		return dstImage;
	}
}

std::unique_ptr<float[]> ScanOCRLib::Recognizer::getPreprocessImage(cv::Mat& srcImage)
{
	constexpr int channel = 3;
	const float ratio = static_cast<float>(_params.minHeight) / srcImage.rows;
	int newWidth = srcImage.cols * ratio;

	cv::Mat resizedImage, normalizedImage;
	cv::resize(srcImage, resizedImage, cv::Size(newWidth, _params.minHeight), 0.f, 0.f, cv::INTER_LINEAR);
	_pModel->setInputShape(std::vector<size_t>{ 1, 3, static_cast<size_t>(_params.minHeight), static_cast<size_t>(newWidth) });

	resizedImage.convertTo(normalizedImage, CV_32FC3, 1.0 / 255);
	std::vector<cv::Mat> channels(3);
	cv::split(normalizedImage, channels);

	for (int i = 0; i < 3; ++i)
	{
		cv::subtract(channels[i], cv::Scalar(_params.mean[i]), channels[i]);
		cv::divide(channels[i], cv::Scalar(_params.std[i]), channels[i]);
	}

	cv::merge(channels, normalizedImage);

	auto pData = std::make_unique<float[]>(_params.minHeight * newWidth * channel);
	for (int c = 0; c < channel; ++c)
	{
		cv::extractChannel(normalizedImage, cv::Mat(_params.minHeight, newWidth,
			CV_32FC1, pData.get() + c * _params.minHeight * newWidth), c);
	}

	return pData;
}

std::pair<std::string, float> ScanOCRLib::Recognizer::getPostprocessImage(const float* pData)
{
	auto outputShape = _pModel->getOutputShape();
	int lastIndex = 0;
	float score = 0;
	int count = 0;
	std::string result = "";
	for (int i = 0; i != outputShape[1]; ++i)
	{
		auto maxIter = std::max_element(pData + i * outputShape[2], pData + (i + 1) * outputShape[2]);
		auto argMax = std::distance(pData + i * outputShape[2], maxIter);
		if (argMax > 0 && (!(i > 0 && argMax == lastIndex)))
		{
			score += *maxIter;
			count++;
			result += _characters[argMax - 1];
		}
		lastIndex = argMax;
	}
	score /= count;
	score = std::isnan(score) ? 0.0f : score;

	return { result, score };
}

std::vector<ScanOCRLib::OCRBox> ScanOCRLib::Recognizer::inference(cv::Mat& srcImage, std::vector<OCRBox>& ocrBoxes)
{
	std::vector<OCRBox> newOcrBoxes;
	for (auto& ocrBox : ocrBoxes)
	{
		cv::Mat cropImage = getRoateCropImage(srcImage, ocrBox);
		auto pData = getPreprocessImage(cropImage);
		_pModel->assignInputTensor(pData.get());
		float* outputData = _pModel->run();
		auto result = getPostprocessImage(outputData);
		
		if (0.0f < result.second)
		{
			ocrBox.content = result.first;
			ocrBox.recognitionScore = result.second;
			newOcrBoxes.push_back(ocrBox);
		}
	}

	return newOcrBoxes;
}