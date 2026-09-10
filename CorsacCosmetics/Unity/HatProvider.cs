using System;
using CorsacCosmetics.Cosmetics;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Unity;

public class HatProvider : ResourceProviderBase
{
    private static HatProvider? _instance;
    private static IResourceProvider? _provider;
    
    public static void Initialize()
    {
        _instance = new HatProvider();
        // interfaces r broken in il2cpp so we have to use pointer magic
        _provider = new IResourceProvider(_instance.Pointer);
        Addressables.ResourceManager.ResourceProviders.Insert(0, _provider);
    }

    public HatProvider(IntPtr intPtr) : base(intPtr) { }

    public HatProvider() : base(ClassInjector.DerivedConstructorPointer<HatProvider>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public override bool CanProvide(Il2CppSystem.Type t, IResourceLocation location)
    {
        return location.InternalId.StartsWith("corsac.");
    }

    public override Il2CppSystem.Type GetDefaultType(IResourceLocation location)
    {
        return location.ResourceType;
    }

    public override void Provide(ProvideHandle provideHandle)
    {
        var id = provideHandle.Location.InternalId;
        var type = provideHandle.Location.ResourceType;
        Debug($"Processing {id} of type {type.FullName}");

        if (!id.StartsWith("corsac"))
        {
            Error($"{id} is not a Corsac cosmetic");
            provideHandle.Complete<UnityEngine.Object>(null!, false, new Il2CppSystem.Exception("Not a Corsac cosmetic"));
            return;
        }

        if (CosmeticsLoader.Instance.ProvideCosmetic(provideHandle, id, type, out var exception))
        {
            Debug($"Successfully provided cosmetic {id} of type {type}");
        }
        else
        {
            Error($"Failed to provide cosmetic {id} of type {type}:\n{exception}");
            provideHandle.Complete<UnityEngine.Object>(null!, false, 
                new Il2CppSystem.Exception(exception.ToString()));
        }
    }

    public override void Release(IResourceLocation location, Il2CppSystem.Object obj)
    {
        if (CosmeticsLoader.Instance.ReleaseCosmetic(location, obj))
        {
            Debug($"Successfully released cosmetic for location {location.InternalId} and object {obj.GetIl2CppType().NameOrDefault}");
        }
        else
        {
            Error($"Failed to release cosmetic for location {location.InternalId} and object {obj.GetIl2CppType().NameOrDefault}");
        }
    }
}