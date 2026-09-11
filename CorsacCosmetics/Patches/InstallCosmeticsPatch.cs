using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CorsacCosmetics.Tools;
using HarmonyLib;
using UnityEngine;

namespace CorsacCosmetics.Patches;

[HarmonyPatch(typeof(ReferenceDataManager), nameof(ReferenceDataManager.Initialize))]
public static class InstallCosmeticsPatch
{
    public static void Postfix(ReferenceDataManager __instance, ref Il2CppSystem.Collections.IEnumerator __result)
    {
        var original = __result;
        __result = CoInstallCosmetics(__instance, original).WrapToIl2Cpp();
    }

    private static IEnumerator CoInstallCosmetics(
        ReferenceDataManager referenceDataManager,
        Il2CppSystem.Collections.IEnumerator original)
    {
        // run original coroutine
        while (original.MoveNext())
        {
            yield return original.Current;
        }

        var discoveryTask = CorsacCosmeticsPlugin.Instance.SourceRegistry.DiscoverAllAsync();
        yield return discoveryTask.AsIEnumerator();

        var cosmeticGroup = ScriptableObject.CreateInstance<CosmeticReleaseGroup>();
        cosmeticGroup.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var discoveredCosmetics = discoveryTask.Result;
        foreach (var cosmetic in discoveredCosmetics)
        {
            cosmeticGroup.ids.Add(cosmetic.Id);
            CorsacCosmeticsPlugin.Instance.CosmeticsCatalog.Register(cosmetic);
            switch (cosmetic.Type)
            {
                case Cosmetics.CosmeticType.Hat:
                    referenceDataManager.Refdata.hats.Add(cosmetic.ToCosmeticData<HatData>());
                    break;
                case Cosmetics.CosmeticType.Visor:
                    referenceDataManager.Refdata.visors.Add(cosmetic.ToCosmeticData<VisorData>());
                    break;
                case Cosmetics.CosmeticType.NamePlate:
                    referenceDataManager.Refdata.nameplates.Add(cosmetic.ToCosmeticData<NamePlateData>());
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}