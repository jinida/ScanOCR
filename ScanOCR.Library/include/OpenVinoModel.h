#pragma once
#include "IModel.h"
#include "openvino/openvino.hpp"

namespace ScanOCRLib
{
	class OpenVinoModel : public IModel
	{
	public:
		OpenVinoModel() = delete;
		OpenVinoModel(ov::Core& core) { _core = core; }
		virtual ~OpenVinoModel() override 
		{

		};
		virtual bool build(const std::string& modelPath) override;
		virtual float* run() override;
		virtual void setInputShape(const std::vector<size_t>& inputShape) override;
		virtual void assignInputTensor(const float* tensorData) override;
		virtual std::vector<size_t> getOutputShape() override;

	private:
		std::string _modelPath;
		std::string _deviceName;
		ov::Core _core;
		ov::CompiledModel _compiledModel;
		ov::InferRequest _inferRequest;
		ov::Tensor _inputTensor;
		ov::Tensor _outputTensor;
	};
}

