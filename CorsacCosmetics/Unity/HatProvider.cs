using System;
using System.Linq;
using CorsacCosmetics.Loader;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Unity;

[RegisterInIl2Cpp(typeof(IResourceProvider))]
public class HatProvider : ResourceProviderBase
{
    private static HatProvider? _instance;

    public HatProvider(IntPtr intPtr) : base(intPtr) { }

    public HatProvider() : base(ClassInjector.DerivedConstructorPointer<HatProvider>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
    
    public static void Initialize()
    {
        _instance = new HatProvider();
        // interfaces r broken in il2cpp so we have to use pointer magic
        var hatProvider = new IResourceProvider(_instance.Pointer);
        Addressables.ResourceManager.ResourceProviders.Insert(0, hatProvider);
    }

    public override void Provide(ProvideHandle provideHandle)
    {
        string internalId = provideHandle.Location.InternalId;

        if (!internalId.StartsWith("corsac://hat/"))
        {
            provideHandle.Complete<Sprite>(null!, false, new Il2CppSystem.Exception("Not a Corsac hat"));
            return;
        }

        var lastSegment = internalId.Split('/').Last();
        var idAndType = lastSegment.Split('.');
        if (idAndType.Length != 2) 
        {
            provideHandle.Complete<Sprite>(null!, false, new Il2CppSystem.Exception("Invalid Corsac hat ID"));
            return;
        }

        var hatId = idAndType[0];
        var type = idAndType[1];

        if (!HatLoader.Instance.CustomHats.TryGetValue(hatId, out var customHat))
        {
            provideHandle.Complete<Sprite>(null!, false, new Il2CppSystem.Exception("Corsac hat not found"));
            return;
        }

        switch (type)
        {
            case CustomType.Sprite:
                provideHandle.Complete(customHat.HatSprite, true, null);
                return;
            case CustomType.HatViewData:
                provideHandle.Complete(customHat.HatViewData, true, null);
                return;
            default:
                provideHandle.Complete<Sprite>(null!, false, new Il2CppSystem.Exception("Invalid Corsac hat type"));
                return;
        }
    }
}