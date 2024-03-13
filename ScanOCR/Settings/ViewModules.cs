using Prism.Ioc;
using Prism.Modularity;

namespace ScanOCR.Settings
{
    internal class ViewModules : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<IViewable, MainContent>("MainContent");
        }
    }
}
