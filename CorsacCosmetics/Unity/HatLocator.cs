using System;
using CorsacCosmetics.Loader;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace CorsacCosmetics.Unity;

[RegisterInIl2Cpp(typeof(IResourceLocator))]
public class HatLocator : Il2CppSystem.Object
{

    private static HatLocator? _instance;
    private static IResourceLocator? _locator;

    public static string GetTypedId(string hatId, string postfix)
    {
        return $"{hatId}.{postfix}";
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

    public bool Locate(Il2CppSystem.Object key, Il2CppSystem.Type type, out Il2CppSystem.Collections.Generic.IList<IResourceLocation> locations)
    {
        locations = null!;

        Info("Locating hat...");

        if (key.TryCast<Il2CppSystem.String>() is { } keyString &&
            keyString.StartsWith("corsac.") &&
            HatLoader.Instance.CustomHats.TryGetValue(keyString, out var customHat))
        {
            Info($"Found hat with key {keyString}");
            var postfix = type.Name switch
            {
                "Sprite" => CustomType.Sprite,
                "HatViewData" => CustomType.HatViewData,
                _ => ""
            };

            var typedId = $"{customHat.Id}.{postfix}";

            var location = new ResourceLocationBase(
                typedId,
                $"corsac://hat/{typedId}",
                ProviderId,
                type
            );

            var il2CPPList = new Il2CppSystem.Collections.Generic.List<ResourceLocationBase>();
            il2CPPList.Add(location);
            // pointer magic cuz il2cpp interfaces are broken
            locations = new Il2CppSystem.Collections.Generic.IList<IResourceLocation>(il2CPPList.Pointer);

            return true;
        }

        Info("No hat found.");
        return false;
    }
}