using Jamesnet.Wpf.Global.Location;

namespace ScanOCR.Settings
{
    internal class WireDataContext : ViewModelLocationScenario
    {
        protected override void Match(ViewModelLocatorCollection items)
        {
            //items.Register<ColorWindow, ColorWindowViewModel>();
            //items.Register<ColorEditor, ColorEditorViewModel>();
            //items.Register<PickerWindow, PickerViewModel>();
        }
    }
}
