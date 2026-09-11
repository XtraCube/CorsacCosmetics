using System;
using System.Collections.Generic;
using CorsacCosmetics.Cosmetics;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace CorsacCosmetics.Unity;

public class CosmeticsLocator : Il2CppSystem.Object
{
    private readonly CosmeticsCatalog _catalog;
    private readonly Dictionary<string, Il2CppSystem.Collections.Generic.IList<IResourceLocation>> _locationCache = [];
    private readonly Il2CppSystem.Collections.Generic.List<IResourceLocation> _locations = new();
    private readonly Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> _keys = new();

    public string LocatorId => typeof(CosmeticsLocator).FullName!;
 
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Il2CppSystem.Collections.Generic.IEnumerable<IResourceLocation> Locations { get; }
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object> Keys { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CosmeticsLocator(IntPtr intPtr) : base(intPtr) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public CosmeticsLocator(CosmeticsCatalog catalog)
        : base(ClassInjector.DerivedConstructorPointer<CosmeticsLocator>())
    {
        Keys = new(_keys.Pointer);
        Locations = new(_locations.Pointer);
        _catalog = catalog;
    }

    public bool Locate(Il2CppSystem.Object key, Il2CppSystem.Type type, out Il2CppSystem.Collections.Generic.IList<IResourceLocation> locations)
    {
        locations = null!;
        var id = key.ToString();

        if (id == null || _catalog.Get(id) is null)
        {
            return false;
        }

        var cacheKey = id + type.FullName;
        if (_locationCache.TryGetValue(cacheKey, out var cachedLocations))
        {
            locations = cachedLocations;
            return true;
        }
            
        var location = new ResourceLocationBase(
            id, 
            id, 
            typeof(CosmeticsProvider).FullName,
            type
        );

        // Pointer magic because Il2CPP interfaces r broken
        var il2CPPList = new Il2CppSystem.Collections.Generic.List<ResourceLocationBase>();
        il2CPPList.Add(location);
        locations = new Il2CppSystem.Collections.Generic.IList<IResourceLocation>(il2CPPList.Pointer);

        // Save to collections
        _locationCache.Add(cacheKey, locations);
        _locations.Add(location.Cast<IResourceLocation>());
        _keys.Add(key);
        return true;
    }
}