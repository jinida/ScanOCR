#include "pch.h"
#include "Detector.h"
#include "clipper.h"

ScanOCRLib::Detector::Detector(IModel* pModel, DetectParameter params)
{
	_pModel = pModel;
	_params = params;
	
	if (!_pModel->build(_params.modelPath)) 
	{
        std::cerr << "Model build failed." << std::endl;
        exit(0);
    }
}

std::vector<std::pair<float, float>> ScanOCRLib::Detector::getMiniBox(cv::RotatedRect& box)
{
	cv::Mat points;
	cv::boxPoints(box, points);

	std::vector<std::pair<float, float>> vec2D(points.rows);

	for (int i = 0; i !=4; ++i)
	{
		vec2D[i].first = points.at<float>(i, 0);
		vec2D[i].second = points.at<float>(i, 1);
	}

	std::sort(vec2D.begin(), vec2D.end(), [](auto& a, auto& b)
	{
		if (a.first != b.first)
		{
			return a.first < b.first;
		}
		return false;
	});

	std::pair<float, float> idx1 = vec2D[0], idx2 = vec2D[1], idx3 = vec2D[2], idx4 = vec2D[3];
	if (vec2D[3].second <= vec2D[2].second)
	{
		idx2 = vec2D[3];
		idx3 = vec2D[2];
	}
	else
	{
		idx2 = vec2D[2];
		idx3 = vec2D[3];
	}

	if (vec2D[1].second <= vec2D[0].second)
	{
		idx1 = vec2D[1];
		idx4 = vec2D[0];
	}
	else
	{
		idx1 = vec2D[0];
		idx4 = vec2D[1];
	}

	vec2D[0] = idx1;
	vec2D[1] = idx2;
	vec2D[2] = idx3;
	vec2D[3] = idx4;

	return vec2D;
}

float ScanOCRLib::Detector::getBoxScore(std::vector<std::pair<float, float>> boxArray, const cv::Mat& prediction)
{
	int width = prediction.cols;
	int height = prediction.rows;

	float boxX[4] = { boxArray[0].first, boxArray[1].first, boxArray[2].first, boxArray[3].first };
	float boxY[4] = { boxArray[0].second, boxArray[1].second, boxArray[2].second, boxArray[3].second };
	int xMin = std::clamp(static_cast<int>(std::floor(*(std::min_element(boxX, boxX + 4)))), 0, width - 1);
	int xMax = std::clamp(static_cast<int>(std::floor(*(std::max_element(boxX, boxX + 4)))), 0, width - 1);
	int yMin = std::clamp(static_cast<int>(std::floor(*(std::min_element(boxY, boxY + 4)))), 0, height - 1);
	int yMax = std::clamp(static_cast<int>(std::floor(*(std::max_element(boxY, boxY + 4)))), 0, height - 1);

	cv::Mat mask = cv::Mat::zeros(yMax - yMin + 1, xMax - xMin + 1, CV_8UC1);
	cv::Point rootPoint[4];
	rootPoint[0] = cv::Point(int(boxArray[0].first) - xMin, int(boxArray[0].second) - yMin);
	rootPoint[1] = cv::Point(int(boxArray[1].first) - xMin, int(boxArray[1].second) - yMin);
	rootPoint[2] = cv::Point(int(boxArray[2].first) - xMin, int(boxArray[2].second) - yMin);
	rootPoint[3] = cv::Point(int(boxArray[3].first) - xMin, int(boxArray[3].second) - yMin);

	const cv::Point* ppt[1] = { rootPoint };
	int npt[] = { 4 };
	cv::fillPoly(mask, ppt, npt, 1, cv::Scalar(1));

	cv::Mat croppedImg;
	prediction(cv::Rect(xMin, yMin, xMax - xMin + 1, yMax - yMin + 1)).copyTo(croppedImg);

	auto score = cv::mean(croppedImg, mask)[0];
	return score;
}

float ScanOCRLib::Detector::getContourArea(std::vector<std::pair<float, float>> box)
{
	float area = 0.0f, dist = 0.0f;
	for (size_t i = 0; i < box.size(); ++i) 
	{
		const auto& [x1, y1] = box[i];
		const auto& [x2, y2] = box[(i + 1) % box.size()];
		area += (x1 * y2 - y1 * x2);
		dist += std::hypot(x2 - x1, y2 - y1);
	}
	area = std::fabs(area / 2.0f);
	return area * _params.unclipRatio / dist;
}

std::unique_ptr<float[]> ScanOCRLib::Detector::getPreprocessImage(const cv::Mat& srcImage)
{
	const auto [w, h] = std::make_pair(srcImage.cols, srcImage.rows);
	const auto minMaxSide = (_params.resizeMethod == "min") ? std::min(h, w) : std::max(h, w);
	const float ratio = (_params.resizeMethod == "min" && minMaxSide < _params.maxSide) || (_params.resizeMethod != "min" && minMaxSide > _params.maxSide)
		? static_cast<float>(_params.maxSide) / minMaxSide : 1.0f;

	const int resizeWidth = std::max(static_cast<int>(std::roundf(w * ratio / 32) * 32), 32);
	const int resizeHeight = std::max(static_cast<int>(std::roundf(h * ratio / 32) * 32), 32);

	cv::Mat resizedImage;
	cv::resize(srcImage, resizedImage, { resizeWidth, resizeHeight });
	_pModel->setInputShape(std::vector<size_t>{ 1, 3, static_cast<size_t>(resizeHeight), static_cast<size_t>(resizeWidth) });

	cv::Mat normalizedImage;
	resizedImage.convertTo(normalizedImage, CV_32FC3, 1.0 / 255);
	std::vector<cv::Mat> channels(3);
	cv::split(normalizedImage, channels);

	cv::waitKey(0);
	for (int i = 0; i < 3; ++i) 
	{
		cv::subtract(channels[i], cv::Scalar(_params.mean[i]), channels[i]);
		cv::divide(channels[i], cv::Scalar(_params.std[i]), channels[i]);
	}

	cv::merge(channels, normalizedImage);	
	auto pData = std::make_unique<float[]>(resizeHeight * resizeWidth * normalizedImage.channels());
	for (int c = 0; c < normalizedImage.channels(); ++c) 
	{
		cv::extractChannel(normalizedImage, cv::Mat(resizeHeight, resizeWidth, 
			CV_32FC1, pData.get() + c * resizeHeight * resizeWidth), c);
	}

	return pData;
}

cv::RotatedRect ScanOCRLib::Detector::unclip(std::vector<std::pair<float, float>> box)
{
	float distance = getContourArea(box);
	ClipperLib::ClipperOffset offset;

	ClipperLib::Path path;
	path << ClipperLib::IntPoint(static_cast<int>(box[0].first), static_cast<int>(box[0].second))
		<< ClipperLib::IntPoint(static_cast<int>(box[1].first), static_cast<int>(box[1].second))
		<< ClipperLib::IntPoint(static_cast<int>(box[2].first), static_cast<int>(box[2].second))
		<< ClipperLib::IntPoint(static_cast<int>(box[3].first), static_cast<int>(box[3].second));

	offset.AddPath(path, ClipperLib::jtRound, ClipperLib::etClosedPolygon);
	ClipperLib::Paths paths;
	offset.Execute(paths, distance);

	std::vector<cv::Point2f> points;
	for (int j = 0; j < paths.size(); j++)
	{
		for (int i = 0; i < paths[paths.size() - 1].size(); i++)
		{
			points.emplace_back(paths[j][i].X, paths[j][i].Y);
		}
	}
	cv::RotatedRect res;
	if (points.size() <= 0)
	{
		res = cv::RotatedRect(cv::Point2f(0, 0), cv::Size2f(1, 1), 0);
	}
	else
	{
		res = cv::minAreaRect(points);
	}

	return res;
}

std::vector<std::pair<int, int>> getOrderPointsClockwise(std::vector<std::pair<int, int>>& points)
{
	std::sort(points.begin(), points.end(), [](auto& a, auto& b) {
		if (a.first != b.first)
		{
			return a.first < b.first;
		}
		return false;
	});

	std::vector<std::pair<int, int>> leftmost = { points[0], points[1] };
	std::vector<std::pair<int, int>> rightmost = { points[2], points[3] };

	if (leftmost[0].second > leftmost[1].second)
	{
		std::swap(leftmost[0], leftmost[1]);
	}

	if (rightmost[0].second > rightmost[1].second)
	{
		std::swap(rightmost[0], rightmost[1]);
	}

	std::vector<std::pair<int, int>> rect = { leftmost[0], rightmost[0], rightmost[1], leftmost[1] };
	return rect;
}

std::vector<ScanOCRLib::OCRBox> ScanOCRLib::Detector::getPostprocessImage(const float* pData, std::pair<int, int> inputShape)
{
	auto outShape = _pModel->getOutputShape();
	int product = std::accumulate(outShape.begin(), outShape.end(), 1, [](size_t a, size_t b) { return a * b; });
	
	std::vector<unsigned char> cbuf(product);
	for (int i = 0; i != product; ++i)
	{
		cbuf[i] = pData[i] > _params.threshold ? static_cast<unsigned char>(pData[i] * 255) : 0;
	}

	cv::Mat bitMap(static_cast<int>(outShape[2]), static_cast<int>(outShape[3]), CV_8UC1, cbuf.data());
	cv::Mat predMap(static_cast<int>(outShape[2]), static_cast<int>(outShape[3]), CV_32F, (float*) pData);

	int width = bitMap.cols;
	int height = bitMap.rows;
	std::vector<std::vector<cv::Point>> contours;
	std::vector<cv::Vec4i> hierarchy;
	
	cv::findContours(bitMap, contours, hierarchy, cv::RETR_LIST, cv::CHAIN_APPROX_SIMPLE);
	int numContour = contours.size() >= _params.maxCandidates ? _params.maxCandidates : contours.size();
	std::vector<ScanOCRLib::OCRBox> ocrBoxes;

	int rectWidth, rectHeight;
	float ratioWidth = static_cast<float>(width) / inputShape.first;
	float ratioHeight = static_cast<float>(height) / inputShape.second;

	for (int i = 0; i != numContour; ++i)
	{
		if (contours[i].size() <= 2)
		{
			continue;
		}

		cv::RotatedRect box = cv::minAreaRect(contours[i]);
		float maxSide = std::max(box.size.width, box.size.height);

		if (maxSide < _params.minSize)
		{
			continue;
		}

		auto miniBoxes = getMiniBox(box);
		float score = getBoxScore(miniBoxes, predMap);

		if (score < _params.boxThreshold)
		{
			continue;
		}

		cv::RotatedRect clipBox = unclip(miniBoxes);

		if (clipBox.size.height < 1.001 && clipBox.size.width < 1.001)
		{
			continue;
		}

		maxSide = std::max(clipBox.size.width, clipBox.size.height);

		auto clipArray = getMiniBox(clipBox);
		if (maxSide < _params.unclipRatio * _params.minSize)
		{
			continue;
		}

		std::vector<std::pair<int, int>> iClipArray(4);
		for (int j = 0; j != 4; ++j)
		{
			iClipArray[j].first = static_cast<int>(std::clamp(roundf(clipArray[j].first), 0.f, static_cast<float>(width)));
			iClipArray[j].second = static_cast<int>(std::clamp(roundf(clipArray[j].second), 0.f, static_cast<float>(height)));
		}
		
		iClipArray = getOrderPointsClockwise(iClipArray);
		for (int i = 0; i != 4; ++i)
		{
			iClipArray[i].first /= ratioWidth;
			iClipArray[i].second /= ratioHeight;
			iClipArray[i].first = static_cast<int>(std::min(std::max(iClipArray[i].first, 0), inputShape.first - 1));
			iClipArray[i].second = static_cast<int>(std::min(std::max(iClipArray[i].second, 0), inputShape.second - 1));
		}

		rectWidth = static_cast<int>(std::hypot(iClipArray[1].second - iClipArray[0].second, iClipArray[1].first - iClipArray[0].first));
		rectHeight = static_cast<int>(std::hypot(iClipArray[3].second - iClipArray[0].second, iClipArray[3].first - iClipArray[0].first));

		if (rectWidth < 5 || rectHeight < 5)
		{
			continue;
		}

		ScanOCRLib::OCRBox ocrBox;
		ocrBox.detectionScore = score;
		ocrBox.box = iClipArray;
		ocrBoxes.push_back(ocrBox);
	}

	return ocrBoxes;
}

std::vector<ScanOCRLib::OCRBox> ScanOCRLib::Detector::inference(const cv::Mat& srcImage, int width, int height)
{
	auto pData = getPreprocessImage(srcImage);
	_pModel->assignInputTensor(pData.get());
	float* outputData = _pModel->run();
	std::pair<int, int> inputShape = { width, height };
	std::vector<ScanOCRLib::OCRBox> ocrBoxes = getPostprocessImage(outputData, inputShape);
	return ocrBoxes;
}