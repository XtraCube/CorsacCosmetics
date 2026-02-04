using System;
using CorsacCosmetics.Loader;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace CorsacCosmetics.Unity;

[RegisterInIl2Cpp(typeof(IResourceLocator))]
public class HatLocator : Il2CppSystem.Object
{

    private static HatLocator? _instance;
    private static IResourceLocator? _locator;

    public static string GetGuid(string hatId, string type)
    {
        return $"{hatId}/{type}";
    }

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

    public virtual Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>
        Keys => HatLoader.Instance.GetHatKeys();

    private string ProviderId { get; } = typeof(HatProvider).FullName!;

    public bool Locate(Il2CppSystem.Object key, Il2CppSystem.Type type,
        out Il2CppSystem.Collections.Generic.IList<IResourceLocation> locations)
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

        var split = keyString.Split('/');
        if (split.Length != 2)
        {
            Error($"Invalid format: {keyString}");
            return false;
        }

        var realKey = split[0];
        var typeName = split[1];

        if (!HatLoader.Instance.CustomHats.TryGetValue(realKey, out var customHat))
        {
            Error($"{keyString} not found in custom hats.");
            return false;
        }

        Info($"Found hat {customHat.Id}, type {typeName}");

        var realType = typeName switch
        {
            ReferenceType.Sprite => Il2CppType.Of<Sprite>(),
            ReferenceType.HatViewData => Il2CppType.Of<HatViewData>(),
            _ => throw new InvalidOperationException("Unsupported type")
        };

        var location = new ResourceLocationBase(
            keyString,
            keyString,
            ProviderId,
            realType
        );

        var il2CPPList = new Il2CppSystem.Collections.Generic.List<ResourceLocationBase>();
        il2CPPList.Add(location);
        // pointer magic cuz il2cpp interfaces are broken
        locations = new Il2CppSystem.Collections.Generic.IList<IResourceLocation>(il2CPPList.Pointer);

        return true;
    }
}