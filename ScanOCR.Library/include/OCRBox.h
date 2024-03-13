#pragma once
#include <string>
#include <vector>
#include <iostream>

#ifdef CREATEDLL_EXPORTS
#define DECLSPEC __declspec(dllexport)
#else
#define DECLSPEC __declspec(dllexport)
#endif

namespace ScanOCRLib
{
	extern "C" 
	{
		DECLSPEC struct OCRBoxC
		{
			int box[4][2];
			float detectionScore;
			wchar_t* content;
			float recognitionScore;
		};

		DECLSPEC struct OCRBoxCArray
		{
			ScanOCRLib::OCRBoxC* boxes;
			int numBoxes;
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
			int len = MultiByteToWideChar(CP_ACP, 0, this->content.data(), -1, NULL, 0);
			ocrBox.content = new wchar_t[len];
			MultiByteToWideChar(CP_ACP, 0, this->content.data(), -1, ocrBox.content, len);
			ocrBox.recognitionScore = this->recognitionScore;
			return ocrBox;
		}
	};
}