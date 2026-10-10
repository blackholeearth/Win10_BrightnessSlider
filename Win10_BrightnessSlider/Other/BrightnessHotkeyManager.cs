using System;
using System.Windows.Forms;

namespace Win10_BrightnessSlider
{
    public static class BrightnessHotkeyManager
    {
        public const int HOTKEY_UP = 1;
        public const int HOTKEY_DOWN = 2;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static bool _registered = false;
        private static IntPtr _hWnd = IntPtr.Zero;

        public static void Initialize(IntPtr hWnd)
        {
            _hWnd = hWnd;
            if (Settings_json.Get().Hotkey_Brightness_Enabled)
                Register();
        }

        public static void Register()
        {
            if (_hWnd == IntPtr.Zero) return;
            Unregister();
            RegisterHotKey(_hWnd, HOTKEY_UP, MOD_CONTROL | MOD_ALT, (uint)Keys.Up);
            RegisterHotKey(_hWnd, HOTKEY_DOWN, MOD_CONTROL | MOD_ALT, (uint)Keys.Down);
            _registered = true;
        }

        public static void Unregister()
        {
            if (_registered && _hWnd != IntPtr.Zero)
            {
                UnregisterHotKey(_hWnd, HOTKEY_UP);
                UnregisterHotKey(_hWnd, HOTKEY_DOWN);
                _registered = false;
            }
        }
    }
}
