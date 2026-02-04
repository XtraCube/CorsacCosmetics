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
    public string LocatorId => "corsac.hatlocator";

    public virtual Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>
        Keys => HatLoader.Instance.GetHatKeys();

    public string ProviderId { get; } = typeof(HatProvider).FullName!;

    public HatLocator(IntPtr ptr) : base(ptr)
    {
    }

    private static HatLocator? _instance;
    
    public HatLocator() : base(ClassInjector.DerivedConstructorPointer<HatLocator>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public static string GetTypedId(string hatId, string postfix)
    {
        return $"{hatId}.{postfix}";
    }

    public static void Initialize()
    {
        _instance = new HatLocator();
        var il2CppLocator = new IResourceLocator(_instance.Pointer);
        Addressables.AddResourceLocator(il2CppLocator);
    }

    public bool Locate(object key, Il2CppSystem.Type type, out Il2CppSystem.Collections.Generic.IList<IResourceLocation> locations)
    {
        locations = null!;

        if (key is string keyString && keyString.StartsWith("corsac.") &&
            HatLoader.Instance.CustomHats.TryGetValue(keyString, out var customHat))
        {
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

        return false;
    }
}