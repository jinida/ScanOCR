#include "pch.h"
#include "OpenVinoModel.h"

bool ScanOCRLib::OpenVinoModel::build(const std::string& modelPath)
{
    _modelPath = modelPath;
    _deviceName = "CPU";
    try
    {
        _compiledModel = _core.compile_model(modelPath, _deviceName);
        _inferRequest = _compiledModel.create_infer_request();
        return true;
    }
    catch (const std::exception& ex)
    {
        std::cerr << "Exception occurred: " << ex.what() << std::flush;
        return false;
    }
}

std::unique_ptr<float[]> ScanOCRLib::OpenVinoModel::run()
{
    try 
    {
        _inferRequest.infer();
        _outputTensor = _inferRequest.get_tensor(_compiledModel.output().get_any_name());
        auto outputSize = ov::shape_size(_outputTensor.get_shape());
        auto pData = std::make_unique<float[]>(outputSize);
        std::memcpy(pData.get(), _outputTensor.data<float>(), outputSize * sizeof(float));
        return pData;
    }
    catch (const std::exception& ex) 
    {
        std::cerr << "Exception occurred in run: " << ex.what() << std::endl;
        return nullptr;
    }
}

void ScanOCRLib::OpenVinoModel::setInputShape(const std::vector<size_t>& inputShape)
{
    _inputTensor = ov::Tensor(ov::element::f32, inputShape);
}

void ScanOCRLib::OpenVinoModel::assignInputTensor(const float* tensorData)
{
    memcpy(_inputTensor.data(), tensorData, ov::shape_size(_inputTensor.get_shape()) * sizeof(float));
    _inferRequest.set_input_tensor(_inputTensor);
}

std::vector<size_t> ScanOCRLib::OpenVinoModel::getOutputShape()
{
    return _outputTensor.get_shape();
}
