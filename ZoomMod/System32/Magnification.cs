namespace ZoomMod.System32;

using System;
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

    public static void SetZoomFactor(float zoomFactor, int[] offsets)
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

        if (curMag == zoomFactor)
            return;

        if (MagSetFullscreenTransform(zoomFactor, offsets[0], offsets[1]))
            curMag = zoomFactor;
        else
        {
            throw new InvalidOperationException("Failed to set fullscreen transform.");
        }
    }
}
