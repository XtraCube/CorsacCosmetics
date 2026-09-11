using System.Runtime.CompilerServices;
using HarmonyLib;
using Il2CppSystem;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Patches;

[HarmonyPatch]
public static class ResourceProviderBaseReversePatch
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(ResourceProviderBase), nameof(ResourceProviderBase.Release))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Release(ResourceProviderBase instance, IResourceLocation location, Object? asset)
    {
        // nothing needed
    }
}