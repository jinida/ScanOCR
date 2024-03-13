#include <opencv2/opencv.hpp>
#include <OCRProcessor.h>

void drawBox(cv::Mat& dstImage, std::vector<std::pair<int, int>> box)
{
	cv::line(dstImage, cv::Point2i(box[0].first, box[0].second), cv::Point2i(box[1].first, box[1].second), cv::Scalar(255, 0, 0));
	cv::line(dstImage, cv::Point2i(box[0].first, box[0].second), cv::Point2i(box[3].first, box[3].second), cv::Scalar(255, 0, 0));
	cv::line(dstImage, cv::Point2i(box[2].first, box[2].second), cv::Point2i(box[1].first, box[1].second), cv::Scalar(255, 0, 0));
	cv::line(dstImage, cv::Point2i(box[2].first, box[2].second), cv::Point2i(box[3].first, box[3].second), cv::Scalar(255, 0, 0));
}

int main()
{
	constexpr float threshold = 0.3f;
	constexpr float boxThreshold = 0.6f;
	std::string detectionPath = "model_weight\\det\\openvino\\det_model_simple.xml";
	std::string recognitionPath = "model_weight\\rec\\openvino\\rec_model_simple.xml";
	std::string dictPath = "korean_dict.txt";

	ScanOCRLib::OCRParameter params = createParameter(threshold, boxThreshold, detectionPath, recognitionPath, dictPath);
	ScanOCRLib::OCRProcessor* processor = createOCRProcessor(&params);

	cv::Mat srcImage = cv::imread("asd.png");
	int height = srcImage.rows;
	int width = srcImage.cols;
	unsigned char* inputData = new unsigned char[width * height * srcImage.channels()];
	
	if (srcImage.isContinuous()) 
	{
		memcpy(inputData, srcImage.data, width * height * srcImage.channels());
	}
	else 
	{
		for (int i = 0; i < height; ++i) {
			memcpy(inputData + i * width * srcImage.channels(), srcImage.ptr<unsigned char>(i), width * srcImage.channels());
		}
	}

	auto result = *inference(processor, inputData, width, height);
	
	for (auto& ocrBox : result)
	{
		drawBox(srcImage, ocrBox.box);
		auto a = ocrBox.content.data();
		std::cout << "Box\nContent:" << ocrBox.content << "\n";
	}
	cv::imshow("draw", srcImage);
	cv::waitKey();

	delete processor;
	delete[] inputData;
}