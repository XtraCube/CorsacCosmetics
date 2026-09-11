global using static CorsacCosmetics.Tools.Logger;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using CorsacCosmetics.Components;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Sources;
using CorsacCosmetics.Cosmetics.Visors;
using CorsacCosmetics.Tools;
using CorsacCosmetics.Unity;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorCompat.ReactorID, BepInDependency.DependencyFlags.SoftDependency)]
public partial class CorsacCosmeticsPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public static CorsacCosmeticsPlugin Instance { get; private set; } = null!;

    public CosmeticsCatalog CosmeticsCatalog { get; }
    public SourceRegistry SourceRegistry { get; }

    public CorsacCosmeticsPlugin()
    {
        Instance = this;

        CosmeticsCatalog = new CosmeticsCatalog();
        SourceRegistry = new SourceRegistry();
        SourceRegistry.RegisterSource(new LocalFolderSource(CosmeticPaths.BasePath));
    }

    public override void Load()
    {
        Message("Loading Corsac Cosmetics Plugin...");
        
        Assets.Initialize();

        ReactorCompat.RegisterCredits();
        
        ClassInjector.RegisterTypeInIl2Cpp<InventoryTabPaginationBehaviour>();

        ClassInjector.RegisterTypeInIl2Cpp<CosmeticsLocator>(new RegisterTypeOptions
        {
            Interfaces = new Il2CppInterfaceCollection([typeof(IResourceLocator)])
        });

        ClassInjector.RegisterTypeInIl2Cpp<CosmeticsProvider>(new RegisterTypeOptions
        {
            Interfaces = new Il2CppInterfaceCollection([typeof(IResourceProvider)])
        });
        Info("Injected IL2CPP types");

        var factoryRegistry = new ViewDataLoaderRegistry();
        factoryRegistry.RegisterLoader(new PreviewResourceLoader());
        factoryRegistry.RegisterLoader(new HatResourceLoader());
        factoryRegistry.RegisterLoader(new VisorResourceLoader());
        factoryRegistry.RegisterLoader(new NamePlateResourceLoader());

        var provider = new CosmeticsProvider(CosmeticsCatalog, factoryRegistry);
        Addressables.ResourceManager.ResourceProviders.Insert(0, new(provider.Pointer));

        var locator = new CosmeticsLocator(CosmeticsCatalog);
        Addressables.AddResourceLocator(new(locator.Pointer));

        CosmeticPaths.EnsureDirectoriesExist();
        Info("Necessary directories created");

        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        Info("Harmony patches installed");
        
        Message("Loaded Corsac Cosmetics Plugin!");
    }
}