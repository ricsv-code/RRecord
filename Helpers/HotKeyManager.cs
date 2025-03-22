using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace RRecord
{
    public class HotKeyManager : IDisposable
    {
        private HwndSource _source;
        private IntPtr _windowHandle;
        private int _hotkeyId;

        [Flags]
        public enum ModifierKeys : uint
        {
            None = 0,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Windows = 0x0008
        }

        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public event EventHandler HotKeyPressed;

        public HotKeyManager(ModifierKeys modifiers, uint key)
        {
            CreateMessageWindow();

            _hotkeyId = GetHashCode();
            if (!RegisterHotKey(_windowHandle, _hotkeyId, (uint)modifiers, key))
            {
                throw new InvalidOperationException("Couldn't register hotkey");
            }
        }

        private void CreateMessageWindow()
        {
            HwndSourceParameters parameters = new HwndSourceParameters("HiddenHotKeyListener")
            {
                Width = 1,
                Height = 1,
                ParentWindow = IntPtr.Zero,
                WindowStyle = unchecked((int)0x80000000) // WS_POPUP
            };

            _source = new HwndSource(parameters);
            _source.AddHook(HwndHook);
            _windowHandle = _source.Handle;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _hotkeyId)
            {
                HotKeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterHotKey(_windowHandle, _hotkeyId);
            _source.RemoveHook(HwndHook);
            _source.Dispose();
        }
    }
}
