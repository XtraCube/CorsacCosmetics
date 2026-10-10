using System;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Cosmetics.Sources;
using CorsacCosmetics.Tools;
using HarmonyLib;

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

        while (PluginCompat.BeforeDiscoveryCoroutines.TryDequeue(out var coroutine))
        {
            yield return coroutine;
        }

        Info("Starting discovery task...");
        var discoveryTask = SourceRegistry.Instance.DiscoverAllAsync();
        yield return discoveryTask.AsIEnumerator();
        Info("Finished discovery task");

        var discoveredCosmetics = discoveryTask.Result;
        foreach (var cosmetic in discoveredCosmetics)
        {
            try
            {
                Debug($"Installing {cosmetic.DisplayName}...");
                CosmeticsCatalog.Instance.Register(cosmetic);
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
            catch (Exception e)
            {
                Error($"Failed to install {cosmetic.DisplayName}, type {cosmetic.Type}, metadata {cosmetic.Metadata} : {e}");
            }
        }
        Info($"Installed {discoveredCosmetics.Count} cosmetics.");
    }
}