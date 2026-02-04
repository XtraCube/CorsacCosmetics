using System;
using System.Linq;
using CorsacCosmetics.Loader;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace CorsacCosmetics.Patches;

[HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
public static class HatManagerInitPatch
{
    public static void Postfix(HatManager __instance)
    {
        Info("Loading hats...");
        HatLoader.Instance.LoadHats();
        Info("Hats loaded!");

        Message("Patching HatManager to include custom hats");
        var hats = __instance.allHats.ToList();

        foreach (var (id, customHat) in HatLoader.Instance.CustomHats)
        {
            try
            {
                hats.Add(customHat.HatData);
                Info($"Added {id} to HatManager");
            }
            catch (Exception e)
            {
                Error($"Failed to load hat {id} with exception:\n{e}");
            }
        }

        __instance.allHats = new Il2CppReferenceArray<HatData>(hats.Count);
        for (var i = 0; i < hats.Count; i++)
        {
            __instance.allHats[i] = hats[i];
        }
        
        Message("Loaded custom hats into HatManager");
    }
}