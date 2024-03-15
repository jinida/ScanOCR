using System.Windows;
using System.Windows.Controls;

namespace ScanOCR.LayoutSupport.UI.Units
{
    public class InitialContents : Control
    {
        static InitialContents()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InitialContents), new FrameworkPropertyMetadata(typeof(InitialContents)));
        }
    }
}