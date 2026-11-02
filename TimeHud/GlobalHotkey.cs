using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace TimeHud;

/// <summary>
/// A system-wide hotkey via user32 <c>RegisterHotKey</c>: Windows posts <c>WM_HOTKEY</c> to this
/// window whichever app has focus. Registration fails (IsRegistered false) when another app
/// already owns the combo.
/// </summary>
public sealed class GlobalHotkey : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const uint MOD_NOREPEAT = 0x4000; // holding the keys fires once, not on auto-repeat

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly HwndSource _source;
    private readonly HwndSourceHook _hook;
    private readonly int _id;
    private readonly Action _onPressed;

    public bool IsRegistered { get; }

    // WPF's ModifierKeys flags (Alt=1, Control=2, Shift=4, Windows=8) are the Win32 MOD_* values.
    public GlobalHotkey(Window window, int id, ModifierKeys modifiers, Key key, Action onPressed)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(hwnd);
        _id = id;
        _onPressed = onPressed;
        _hook = WndProc;
        _source.AddHook(_hook);
        IsRegistered = RegisterHotKey(hwnd, id, (uint)modifiers | MOD_NOREPEAT,
            (uint)KeyInterop.VirtualKeyFromKey(key));
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == _id)
        {
            _onPressed();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (IsRegistered) UnregisterHotKey(_source.Handle, _id);
        _source.RemoveHook(_hook);
    }
}
