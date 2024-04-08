using UnityEngine;

namespace NR_AutoMachineTool;

public static class OverlayDrawHandler_UGConveyor
{
    private static int lastDrawFrame;

    public static bool ShouldDraw => lastDrawFrame + 1 >= Time.frameCount;

    public static void DrawOverlayThisFrame()
    {
        lastDrawFrame = Time.frameCount;
    }
}