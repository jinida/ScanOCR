using System.Windows;
using System.Windows.Controls;

namespace ScanOCR.LayoutSupport.UI.Units
{
    public class AppendButton : Button
    {
        static AppendButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppendButton), new FrameworkPropertyMetadata(typeof(AppendButton)));
        }
    }
}