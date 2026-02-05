using CorsacCosmetics.Cosmetics;
using HarmonyLib;

namespace CorsacCosmetics.Patches;

[HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
public static class InstallCosmeticsPatch
{
    public static void Postfix(HatManager __instance)
    {
        Info("Loading cosmetics...");
        CosmeticsLoader.Instance.LoadCosmetics();
        Info("Cosmetics loaded");

        Info("Patching HatManager to include custom cosmetics");
        CosmeticsLoader.Instance.InstallCosmetics(__instance);
        Info("Loaded custom cosmetics into HatManager");
    }
}