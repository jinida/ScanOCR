#pragma once
#include <string>
#include <vector>

namespace ScanOCRLib
{
	class IModel
	{
	public:
		virtual ~IModel() 
		{

		};
		virtual bool build(const std::string& modelPath) = 0;
		virtual float* run() = 0;
		virtual void setInputShape(const std::vector<size_t>& inputShape) = 0;
		virtual void assignInputTensor(const float* tensorData) = 0;
		virtual std::vector<size_t> getOutputShape() = 0;
	private:
		std::string _modelPath;
	};
}

