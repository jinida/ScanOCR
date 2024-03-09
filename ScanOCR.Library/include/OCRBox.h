#pragma once
#include <string>
#include <vector>

namespace ScanOCRLib
{
	struct OCRBox
	{
		std::vector<std::pair<int, int>> box;
		float detectionScore;
		std::string content;
		float recognitionScore;
	};
}