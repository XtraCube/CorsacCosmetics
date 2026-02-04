using HarmonyLib;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Patches;

[HarmonyPatch(typeof(AssetReference), nameof(AssetReference.RuntimeKeyIsValid))]
public static class PreviewDataRuntimeKeyPatch
{
    public static bool Prefix(AssetReference __instance, ref bool __result)
    {
        if (__instance.AssetGUID.StartsWith("corsac."))
        {
            __result = true;
            return false;
        }

        return true;
    }
}