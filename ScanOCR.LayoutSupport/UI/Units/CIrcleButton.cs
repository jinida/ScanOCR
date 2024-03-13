using System.Windows;
using System.Windows.Controls;

namespace ScanOCR.LayoutSupport.UI.Units
{
    public class CircleButton : Button
    {
        static CircleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CircleButton), new FrameworkPropertyMetadata(typeof(CircleButton)));
        }
    }
}