using UnityEngine;

namespace CorsacCosmetics.Tools;

public static class Utilities
{
    public static float GetTargetFrameTimeMilliseconds()
    {
        var fps = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;
        return 1000f / fps;
    }
}