using System.Windows;
using System.Windows.Controls;

namespace ScanOCR.LayoutSupport.UI.Units
{
    public class SettingButton : Button
    {
        static SettingButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingButton), new FrameworkPropertyMetadata(typeof(SettingButton)));
        }
    }
}