using Jamesnet.Wpf.Controls;
using Prism.Ioc;
using ScanOCR.Core.Manager;
using ScanOCR.Forms.UI.Views;
using System.Windows;

namespace ScanOCR
{
    internal class App : JamesApplication
    {
        protected override Window CreateShell()
        {
            var window = Container.Resolve<MainWindow>();
            // 현재 등록된 OpenVinoModel을 로딩하는 동안
            // Loading Window를 띄워놓는 Logic 구현
            return window;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            base.RegisterTypes(containerRegistry);
            containerRegistry.RegisterInstance(new CaptureManager());

        }
    }
}
