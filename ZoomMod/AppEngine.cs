namespace ZoomMod;

using System.Runtime.InteropServices;

using static ZoomMod.Config;
using static ZoomMod.System32.User32;
using static ZoomMod.System32.Magnification;
using System.Diagnostics;

public class AppEngine
{
    static Keymap keymap;
    public static void Run()
    {
        keymap = new Keymap();
        keymap.OnKeyPress += OnKeyPress;

        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                Thread.Sleep(1);//64hz

                keymap.Tick();
            }
        });
    }

    public static void OnKeyPress(KeyEvent evnt)
    {
        RECT winRect = default;
        if (!GetFocusRect(ref winRect))
        {
            Thread.Sleep(1000);
            return;
        }

        if (evnt.Key == 27)
        {
            Process.GetCurrentProcess().Kill();
        }

        var centerX = winRect.Left + winRect.Width / 2.0f;
        var centerY = winRect.Top + winRect.Height / 2.0f;

        var offsetX = centerX - (centerX / ZoomFactor);
        var offsetY = centerY - (centerY / ZoomFactor);
        int[] offsets = [(int)offsetX, (int)offsetY];

        if (!CanUseMoveKeys())
        {
            SetZoomFactor(1.0f, offsets, false);
            return;
        }

        if (evnt.Key == 0x02 && evnt.VKey == VKeyCodes.KeyHeld)
            SetZoomFactor(ZoomFactor, offsets, Config.SmoothIn);

        if (!keymap.GetDown(0x02))
            SetZoomFactor(1.0f, offsets, Config.SmoothOut);
    }

    private static bool GetFocusRect(ref RECT winRect)
    {
        var foregroundWindow = GetForegroundWindow();

        if (foregroundWindow == IntPtr.Zero)
            return false;

        if (!GetWindowRect(foregroundWindow, out winRect))
            return false;

        return true;
    }

    const int CURSOR_SHOWING = 0x00000001;
    private static bool CanUseMoveKeys()
    {
        CURSORINFO cursorInfo = new();
        cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
        if (!GetCursorInfo(ref cursorInfo))
        {
            return false;
        }
        return (cursorInfo.flags & CURSOR_SHOWING) == 0;
    }
}
