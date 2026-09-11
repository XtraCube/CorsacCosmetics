using System;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Patches;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = Il2CppSystem.Object;

namespace CorsacCosmetics.Unity;

public class CosmeticsProvider : ResourceProviderBase
{
    private readonly CosmeticsCatalog _catalog;
    private readonly ViewDataLoaderRegistry _loaderRegistry;
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CosmeticsProvider(IntPtr intPtr) : base(intPtr) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public CosmeticsProvider(CosmeticsCatalog catalog, ViewDataLoaderRegistry loaderRegistry) 
        : base(ClassInjector.DerivedConstructorPointer<CosmeticsProvider>())
    {
        ClassInjector.DerivedConstructorBody(this);
        _catalog = catalog;
        _loaderRegistry = loaderRegistry;
    }

    public override void Provide(ProvideHandle provideHandle)
    {
        var id = provideHandle.Location.PrimaryKey;
        var descriptor = _catalog.Get(id);

        if (descriptor == null)
        {
            provideHandle.Complete<UnityEngine.Object>(null!, false, new Il2CppSystem.Exception($"Cosmetic ID {id} not found."));
            return;
        }

        var loader = _loaderRegistry.GetLoader(provideHandle.Location.ResourceType);
        _ = loader.LoadAndCompleteAsync(provideHandle, descriptor);
    }

    public override void Release(IResourceLocation location, Object? asset)
    {
        var descriptor = _catalog.Get(location.PrimaryKey);

        if (descriptor != null && asset != null)
        {
            var loader = _loaderRegistry.GetLoader(location.ResourceType);
            loader.Release(asset);
        }

        ResourceProviderBaseReversePatch.Release(this, location, asset);
    }
}