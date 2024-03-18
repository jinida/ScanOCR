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
            WindowManager windowManager = Container.Resolve<WindowManager>();
            var window = windowManager.ResolveWindows<LoadingWindow>("LoadingWindow");
            return window;
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            base.RegisterTypes(containerRegistry);
            WindowManager windowManager = new WindowManager();
            ScannerController controller = new ScannerController();
            controller.loadedProcessor += Window_Activated;
            containerRegistry.RegisterInstance(windowManager);
            containerRegistry.RegisterInstance(controller);
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            WindowManager windowManager = Container.Resolve<WindowManager>();
            LoadingWindow? loadingWindow = windowManager.GetWindow("LoadingWindow") as LoadingWindow;
            MainWindow mainWindow = windowManager.ResolveWindows<MainWindow>("MainWindow");
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Closed += MainWindow_Closed;
            loadingWindow?.Close();
            windowManager.UnregisterWindow("LoadingWindow");
            mainWindow.Show();
            mainWindow.Activate();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            WindowManager windowManager = Container.Resolve<WindowManager>();
            if (windowManager != null)
            {
                windowManager.CloseAll();
            }
            Environment.Exit(0);
        }
    }
}

