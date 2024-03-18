#pragma once
#include <memory>
#include <openvino/openvino.hpp>
#include "IModel.h"
#include "OpenVinoModel.h"

namespace ScanOCRLib
{
    class Factory
    {
    public:
        virtual ~Factory() {}
        virtual std::unique_ptr<IModel> make() = 0;
    };

    class OpenVinoFactory : public Factory
    {
    public:
        OpenVinoFactory() : _core(ov::Core()) {}

        std::unique_ptr<IModel> make() override
        {
            return std::unique_ptr<IModel>(new OpenVinoModel(_core));
        }
    private:
        ov::Core _core;
    };

    class ModelFactory
    {
    public:
        void build(const std::string& name)
        {
            if (modelFactory != nullptr)
            {
                modelFactory.release();
            }

            if (name == "OpenVino")
            {
                modelFactory = std::make_unique<OpenVinoFactory>();
            }
            else
            {
                std::cerr << "Not Implement";
                exit(0);
            }
        }

        std::unique_ptr<IModel> makeModel()
        {
            return modelFactory->make();
        }
    private:
        std::unique_ptr<Factory> modelFactory;
    };
}