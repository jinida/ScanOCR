using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ScanOCR.LayoutSupport.UI.Units
{
    public class StateButton : ToggleButton
    {
        static StateButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StateButton), new FrameworkPropertyMetadata(typeof(StateButton)));
        }
    }
}