namespace AutoClicker;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

class Program
{
    const float zoomFactor = 2.0f;

    static Process Rust = null;

    [STAThread]
    static void Main(string[] args)
    {
        Keymap keymap = new Keymap();

        keymap.OnKeyPress += OnKeyPress;

        MagInitialize();
        while (true)
        {
            Thread.Sleep(1);//64hz

            keymap.Tick();
        }
        MagUninitialize();
    }

    private static void LogNote(string note)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {note}");
        Console.ResetColor();
    }

    private static void OnKeyPress(KeyEvent evnt)
    {
        //if (evnt.Key == 0x1B)
        //{
        //    MagUninitialize();
        //    Process.GetCurrentProcess().Kill();
        //}

        if (Rust == null || Rust.HasExited)
        {
            var procs = Process.GetProcessesByName("RustClient");

            if (procs.Length == 0)
            {
                LogNote("RustClient not found, please start the game.");
                Thread.Sleep(1000);
                return;
            }

            Rust = procs[0];
        }

        if (Rust.MainWindowHandle == IntPtr.Zero)
        {
            LogNote("RustClient window not found, please start the game.");
            Thread.Sleep(1000);
            return;
        }

        var rustRect = new RECT();
        if (!GetWindowRect(Rust.MainWindowHandle, out rustRect))
        {
            LogNote("Failed to get RustClient window rectangle.");
            Thread.Sleep(1000);
            return;
        }

        var centerX = rustRect.Left + rustRect.Width / 2.0f;
        var centerY = rustRect.Top + rustRect.Height / 2.0f;

        if (evnt.Key == 0x02)
        {
            if (!CanUseMoveKeys())
                return;

            if (evnt.VKey == VKeyCodes.KeyDown)
            {
                var offsetX = centerX - (centerX / zoomFactor);
                var offsetY = centerY - (centerY / zoomFactor);

                MagSetFullscreenTransform(zoomFactor, (int)offsetX, (int)offsetY);
                LogNote("Fullscreen Magnification enabled.");
            }

            if (evnt.VKey == VKeyCodes.KeyUp)
            {
                MagSetFullscreenTransform(1.0f, 0, 0);
                LogNote("Fullscreen Magnification disabled.");
            }

        }
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

    #region Imports

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("Magnification.dll")]
    public static extern bool MagInitialize();

    [DllImport("Magnification.dll")]
    public static extern bool MagUninitialize();

    [DllImport("Magnification.dll")]
    public static extern bool MagSetFullscreenTransform(float magLevel, int xOffset, int yOffset);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }

    [DllImport("user32.dll")]
    public static extern bool GetCursorInfo(ref CURSORINFO pci);

    #endregion
}