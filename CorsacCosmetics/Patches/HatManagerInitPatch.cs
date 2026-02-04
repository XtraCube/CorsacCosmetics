using System;
using System.Linq;
using CorsacCosmetics.Loader;
using CorsacCosmetics.Unity;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Patches;

[HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
public static class HatManagerInitPatch
{
    public static void Postfix(HatManager __instance)
    {
        Message("Patching HatManager to include custom hats");
        var hats = __instance.allHats.ToList();

        foreach (var (id, customHat) in HatLoader.Instance.CustomHats)
        {
            try
            {
                var hatData = ScriptableObject.CreateInstance<HatData>();
                hatData.ProductId = id;
                hatData.BlocksVisors = customHat.Metadata.BlocksVisors;
                hatData.NoBounce = customHat.Metadata.NoBounce;
                hatData.InFront = customHat.Metadata.InFront;
                hatData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.HatViewData));
                hatData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Sprite));
                hats.Add(hatData);
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