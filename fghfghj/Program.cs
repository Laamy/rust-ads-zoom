namespace ZoomMod;

using System;
using System.Runtime.InteropServices;
using System.Threading;

using static ZoomMod.System32.User32;
using static ZoomMod.System32.Magnification;

class Program
{
    const float zoomFactor = 1.5f;

    [STAThread]
    static void Main(string[] args)
    {
        Keymap keymap = new Keymap();

        keymap.OnKeyPress += OnKeyPress;

        LogNote("Zoom client activated");

        while (true)
        {
            Thread.Sleep(1);//64hz

            keymap.Tick();
        }
    }

    private static void LogNote(string note)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {note}");
    }

    private static void OnKeyPress(KeyEvent evnt)
    {
        RECT winRect = default;
        if (!GetFocusRect(ref winRect))
        {
            Thread.Sleep(1000);
            return;
        }

        var centerX = winRect.Left + winRect.Width / 2.0f;
        var centerY = winRect.Top + winRect.Height / 2.0f;

        if (evnt.Key == 0x02)
        {
            if (!CanUseMoveKeys())
            {
                SetZoomFactor(1.0f, [0, 0]);
                return;
            }

            if (evnt.VKey == VKeyCodes.KeyDown)
            {
                var offsetX = centerX - (centerX / zoomFactor);
                var offsetY = centerY - (centerY / zoomFactor);

                SetZoomFactor(zoomFactor, [(int)offsetX, (int)offsetY]);
            }

            if (evnt.VKey == VKeyCodes.KeyUp)
            {
                SetZoomFactor(1.0f, [0, 0]);
            }
        }
    }

    private static bool GetFocusRect(ref RECT winRect)
    {
        IntPtr foregroundWindow = GetForegroundWindow();

        if (foregroundWindow == IntPtr.Zero)
            return false;

        if (!GetWindowRect(foregroundWindow, out winRect))
            return false;

        return true;
    }

    const int CURSOR_SHOWING = 0x00000001;
    private static bool CanUseMoveKeys()
    {
        CURSORINFO cursorInfo = new CURSORINFO();
        cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
        if (!GetCursorInfo(ref cursorInfo))
        {
            return false;
        }
        return (cursorInfo.flags & CURSOR_SHOWING) == 0;
    }
}
