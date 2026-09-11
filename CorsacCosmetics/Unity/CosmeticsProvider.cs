using System;
using CorsacCosmetics.Cosmetics;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Unity;

public class CosmeticsProvider : ResourceProviderBase
{
    private readonly CosmeticsCatalog _catalog;
    private readonly ViewDataFactoryRegistry _factoryRegistry;

    public override string ProviderId => nameof(CosmeticsProvider);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public CosmeticsProvider(IntPtr intPtr) : base(intPtr) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public CosmeticsProvider(CosmeticsCatalog catalog, ViewDataFactoryRegistry factoryRegistry) 
        : base(ClassInjector.DerivedConstructorPointer<CosmeticsProvider>())
    {
        _catalog = catalog;
        _factoryRegistry = factoryRegistry;
    }

    public override async void Provide(ProvideHandle provideHandle)
    {
        try
        {
            string id = provideHandle.Location.PrimaryKey;
            var descriptor = _catalog.Get(id);

            if (descriptor == null)
            {
                provideHandle.Complete<UnityEngine.Object>(null!, false, new Il2CppSystem.Exception($"Cosmetic ID {id} not found."));
                return;
            }
        
            try
            {
                var factory = _factoryRegistry.GetFactory(descriptor.Type);

                var viewData = await factory.CreateViewData(descriptor);
                if (viewData != null)
                {
                    provideHandle.Complete(viewData, true, null);
                }
                else
                {
                    provideHandle.Complete<UnityEngine.Object>(
                        null!, 
                        false, 
                        new Il2CppSystem.Exception($"Factory returned null ViewData for ID {descriptor.Id}")
                    );
                }
            }
            catch (Exception ex)
            {
                provideHandle.Complete<UnityEngine.Object>(null!, false, new Il2CppSystem.Exception(ex.Message));
            }
        }
        catch (Exception e)
        {
            provideHandle.Complete<UnityEngine.Object>(null!, false, new Il2CppSystem.Exception(e.Message));
        }
    }

    public override void Release(IResourceLocation location, Il2CppSystem.Object asset)
    {
        var descriptor = _catalog.Get(location.PrimaryKey);
        
        if (descriptor != null && asset is UnityEngine.Object unityAsset)
        {
            var factory = _factoryRegistry.GetFactory(descriptor.Type);
            factory.ReleaseViewData(unityAsset);
        }

        base.Release(location, asset);
    }
}