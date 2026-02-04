using System;
using CorsacCosmetics.Loader;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Unity;

[RegisterInIl2Cpp(typeof(IResourceProvider))]
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
        string internalId = provideHandle.Location.InternalId;
        Info($"Processing {internalId}");

        if (!internalId.StartsWith("corsac"))
        {
            Error($"{internalId} is not a Corsac hat");
            provideHandle.Complete<Sprite>(null!, false, new Il2CppSystem.Exception("Not a Corsac hat"));
            return;
        }

        var idAndType = internalId.Split("/");
        if (idAndType.Length != 2) 
        {
            Error($"Invalid hat identifier: {idAndType}");
            provideHandle.Complete<Sprite>(null!, false, new Il2CppSystem.Exception("Invalid Corsac hat ID"));
            return;
        }

        var hatId = idAndType[0];
        var type = idAndType[1];

        if (!HatLoader.Instance.CustomHats.TryGetValue(hatId, out var customHat))
        {
            Error($"Could not find hat: {hatId}");
            provideHandle.Complete<Sprite>(null!, false, new Il2CppSystem.Exception("Corsac hat not found"));
            return;
        }

        switch (type)
        {
            case ReferenceType.Sprite:
                Info($"Found hat sprite for {hatId}");
                provideHandle.Complete(customHat.HatSprite, true, null);
                return;
            case ReferenceType.Preview:
                Info($"Found hat preview for {hatId}");
                provideHandle.Complete(customHat.PreviewData, true, null);
                return;
            case ReferenceType.HatViewData:
                Info($"Found hat view data for {hatId}");
                provideHandle.Complete(customHat.HatViewData, true, null);
                return;
            default:
                Error("Unknown hat type");
                provideHandle.Complete<Sprite>(null!, false, new Il2CppSystem.Exception("Invalid Corsac hat type"));
                return;
        }
    }

    public override void Release(IResourceLocation location, Il2CppSystem.Object obj)
    {
        Warning("I don't know how to release hat yet");
    }
}