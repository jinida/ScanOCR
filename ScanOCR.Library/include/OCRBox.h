#pragma once
#include <string>
#include <vector>

#ifdef CREATEDLL_EXPORTS
#define DECLSPEC __declspec(dllexport)
#else
#define DECLSPEC __declspec(dllexport)
#endif

namespace ScanOCRLib
{
	extern "C" {
		DECLSPEC struct OCRBoxC
		{
			int box[4][2];
			float detectionScore;
			char* content;
			float recognitionScore;
		};
	}

	struct OCRBox
	{
		std::vector<std::pair<int, int>> box;
		float detectionScore;
		std::string content;
		float recognitionScore;

		OCRBoxC toOCRBoxC()
		{
			OCRBoxC ocrBox;
			for (int i = 0; i != 4; ++i)
			{
				ocrBox.box[i][0] = this->box[i].first;
				ocrBox.box[i][1] = this->box[i].second;
			}
			ocrBox.detectionScore = this->detectionScore;
			ocrBox.content = this->content.data();
			ocrBox.recognitionScore = this->recognitionScore;
			return ocrBox;
		}
	};

}