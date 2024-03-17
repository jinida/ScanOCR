using Jamesnet.Wpf.Controls;

namespace ScanOCR.Core.Manager
{
    public class WindowManager
    {
        private readonly Dictionary<string, JamesWindow> _windows;
        
        public WindowManager()
        {
            _windows = new Dictionary<string, JamesWindow>();
        }

        public void UnregisterWindow(string key)
        {
            if (_windows.ContainsKey(key))
            {
                _windows.Remove(key);
            }
        }

        public List<KeyValuePair<string, JamesWindow>> GetAllWindows()
        {
            return _windows.ToList();
        }

        public JamesWindow? GetWindow(string key)
        {
            return _windows.ContainsKey(key) ? _windows[key] : null;
        }

        public T ResolveWindows<T>(string key) where T : JamesWindow, new()
        {
            if (_windows.ContainsKey(key))
            {
                return (T)_windows[key];
            }
            else
            {
                try
                {
                    T window = new T();
                    window.Closed += (sender, e) => UnregisterWindow(key);
                    _windows.Add(key, window);
                    return window;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to resolve the window of type {typeof(T).FullName}.", ex);
                }
            }
        }
        public void CloseAll()
        {
            foreach (KeyValuePair<string, JamesWindow> kvp in _windows)
            {
                kvp.Value.Close();
            }
        }
    }
}
