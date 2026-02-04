using System.Linq;
using CorsacCosmetics.Loader;
using CorsacCosmetics.Unity;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Patches;

[HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
public static class HatManagerInitPatch
{
    public static void Postfix(HatManager __instance)
    {
        Info("Patching HatManager to include custom hats");
        var hats = __instance.allHats.ToList();
        foreach (var (id, customHat) in HatLoader.Instance.CustomHats)
        {
            var hatData = ScriptableObject.CreateInstance<HatData>();
            hatData.ProductId = id;
            hatData.BlocksVisors = customHat.Metadata.BlocksVisors;
            hatData.NoBounce = customHat.Metadata.NoBounce;
            hatData.InFront = customHat.Metadata.InFront;
            hatData.ViewDataRef = new AssetReference(HatLocator.GetTypedId(id, CustomType.HatViewData));
            hatData.PreviewData = new AssetReference(HatLocator.GetTypedId(id, CustomType.Sprite));
            hats.Add(hatData);
            Message("Added custom hat to HatManager: " + id);
        }

        __instance.allHats = hats.ToArray();
        Info("Loaded custom hats into HatManager");
    }
}