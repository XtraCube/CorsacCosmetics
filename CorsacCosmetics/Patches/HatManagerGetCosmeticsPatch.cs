using CorsacCosmetics.Cosmetics;
using HarmonyLib;

namespace CorsacCosmetics.Patches;

[HarmonyPatch(typeof(HatManager))]
public static class HatManagerGetCosmeticsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
    public static bool GetHatPrefix(string hatId, ref HatData __result)
    {
        if (!CosmeticsLoader.Instance.TryGetHat(hatId, out var customHat))
        {
            return true;
        }

        __result = customHat.HatData;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetVisorById))]
    public static bool GetVisorPrefix(string visorId, ref VisorData __result)
    {
        if (!CosmeticsLoader.Instance.TryGetVisor(visorId, out var customVisor))
        {
            return true;
        }

        __result = customVisor.VisorData;
        return false;
    }
}