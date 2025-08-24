namespace ZoomMod.System32;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

internal class Magnification
{
    [DllImport("Magnification.dll")]
    private static extern bool MagInitialize();

    [DllImport("Magnification.dll")]
    private static extern bool MagUninitialize();

    [DllImport("Magnification.dll")]
    private static extern bool MagSetFullscreenTransform(float magLevel, int xOffset, int yOffset);

    private static float curMag = 1.0f;
    private static bool hasInit = false;

    static Stopwatch smoothStart;
    public static void SetZoomFactor(float zoomFactor, int[] offsets, bool smooth)
    {
        if (zoomFactor < 1.0f || zoomFactor > 10.0f)
            throw new ArgumentOutOfRangeException(nameof(zoomFactor), "Zoom factor must be between 1.0 and 10.0.");

        if (!hasInit)
        {
            if (MagInitialize())
                hasInit = true;
            else
            {
                throw new InvalidOperationException("Failed to initialize Magnification.");
            }
        }

        if (smooth)
        {
            if (curMag != zoomFactor && smoothStart == null)
                smoothStart = Stopwatch.StartNew();

            if (curMag == zoomFactor)
            {
                smoothStart = null;
                return;
            }

            var tickFactor = smoothStart?.ElapsedMilliseconds / 1000.0f ?? 0.0f;
            var smoothFactor = Math.Clamp(tickFactor / 2, 0.0f, 1.0f);

            var finalFactor = curMag + (zoomFactor - curMag) * smoothFactor;

            if (Math.Abs(finalFactor - zoomFactor) < 0.02f)
            {
                finalFactor = zoomFactor;
                smoothStart = null;
            }

            if (MagSetFullscreenTransform(finalFactor, offsets[0], offsets[1]))
                curMag = finalFactor;
            else
            {
                throw new InvalidOperationException("Failed to set fullscreen transform.");
            }
        }
        else
        {
            if (MagSetFullscreenTransform(zoomFactor, offsets[0], offsets[1]))
            {
                curMag = zoomFactor;
                smoothStart = null;
            }
            else
            {
                throw new InvalidOperationException("Failed to set fullscreen transform.");
            }
        }
    }
}
