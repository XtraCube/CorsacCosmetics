using System;
using System.Collections.Generic;
using CorsacCosmetics.Cosmetics;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace CorsacCosmetics.Unity;

public class HatLocator : Il2CppSystem.Object
{
    private static HatLocator? _instance;
    private static IResourceLocator? _locator;

    private static readonly Dictionary<string, Il2CppSystem.Collections.Generic.IList<IResourceLocation>> LocationCache = [];
    public static string ProviderId { get; } = typeof(HatProvider).FullName!;

    public static void Initialize()
    {
        _instance = new HatLocator();
        _locator = new IResourceLocator(_instance.Pointer);
        Addressables.AddResourceLocator(_locator);
    }

    public HatLocator(IntPtr ptr) : base(ptr)
    {
    }

    public HatLocator() : base(ClassInjector.DerivedConstructorPointer<HatLocator>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public string LocatorId => GetType().FullName!;

    public Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object> Keys => CosmeticsLoader.Instance.EmptyKeys;


    public bool Locate(Il2CppSystem.Object key, Il2CppSystem.Type type, out Il2CppSystem.Collections.Generic.IList<IResourceLocation> locations)
    {
        locations = null!;

        if (key.ToString() is not { } keyString)
        {
            return false;
        }

        if (!keyString.StartsWith("corsac."))
        {
            return false;
        }

        if (!CosmeticsLoader.Instance.LocateCosmetic(keyString, type))
        {
            Error($"{keyString} not found in custom cosmetics.");
            return false;
        }

        Debug($"Found cosmetic {keyString}, type {type.FullName}");

        var cacheKey = keyString + type.FullName;
        if (LocationCache.TryGetValue(cacheKey, out var cachedLocations))
        {
            locations = cachedLocations;
            return true;
        }

        var location = new ResourceLocationBase(
            keyString,
            keyString,
            ProviderId,
            type
        );

        var il2CPPList = new Il2CppSystem.Collections.Generic.List<ResourceLocationBase>();
        il2CPPList.Add(location);
        // pointer magic cuz il2cpp interfaces are broken
        locations = new Il2CppSystem.Collections.Generic.IList<IResourceLocation>(il2CPPList.Pointer);
        LocationCache.Add(cacheKey, locations);

        return true;
    }
}