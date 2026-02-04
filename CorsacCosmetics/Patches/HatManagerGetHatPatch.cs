using CorsacCosmetics.Loader;
using HarmonyLib;

namespace CorsacCosmetics.Patches;

[HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
public static class HatManagerGetHatPatch
{
    public static bool Prefix(string hatId, ref HatData __result)
    {
        if (!HatLoader.Instance.CustomHats.TryGetValue(hatId, out var customHat))
        {
            return true;
        }

        __result = customHat.HatData;
        return false;
    }
}