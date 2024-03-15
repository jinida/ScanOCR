using Jamesnet.Wpf.Controls;
using System.Windows;

namespace ScanOCR.Core.Manager
{
    public class CaptureManager
    {
        public event EventHandler windowShown;
        public event EventHandler windowClosed;


        private JamesWindow? _window;

        public void ShowPicker()
        {
            Show();
            OnWindowShown();
        }


        public void UnregisterPicker()
        {
            if (_window != null)
            {
                Close();
                OnWindowClosed();
            }
        }

        public JamesWindow? GetWindow()
        {
            return _window;
        }

        public T ResolveWindow<T>() where T : JamesWindow, new()
        {
            if (_window != null)
            {
                return (T)_window;
            }
            else
            {
                try
                {
                    T window = new T();
                    window.Closed += (sender, e) => UnregisterPicker();
                    _window = window;
                    return window;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to resolve the window of type {typeof(T).FullName}.", ex);
                }
            }
        }

        private void OnWindowShown()
        {
            windowShown?.Invoke(this, EventArgs.Empty);
        }

        private void OnWindowClosed()
        {
            windowClosed?.Invoke(this, EventArgs.Empty);
        }

        private void Show()
        {
            if (_window != null)
            {
                //_window.Opacity = 0.0;
                _window.Visibility = Visibility.Visible;
                _window.Show();
            }
        }

        private void Close()
        {
            if (_window != null)
            {
                _window.Close();
                _window = null;
            }
        }
    }
}
