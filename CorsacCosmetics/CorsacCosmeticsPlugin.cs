global using static CorsacCosmetics.Tools.Logger;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
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
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorCompat.ReactorID, BepInDependency.DependencyFlags.SoftDependency)]
public partial class CorsacCosmeticsPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public static CorsacCosmeticsPlugin Instance { get; private set; } = null!;

    public CorsacCosmeticsPlugin()
    {
        Instance = this;
        CosmeticPaths.EnsureDirectoriesExist();
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

        var cosmeticsCatalog = new CosmeticsCatalog();
        var sourceRegistry = new SourceRegistry();
        sourceRegistry.RegisterSource(new LocalFolderSource(CosmeticPaths.BasePath));
        foreach (var file in Directory.EnumerateFiles(CosmeticPaths.BundlePath, "*.ccb"))
        {
            sourceRegistry.RegisterSource(new LocalBundleSource(file));
        }

        var factoryRegistry = new ViewDataLoaderRegistry();
        factoryRegistry.RegisterLoader(new PreviewResourceLoader());
        factoryRegistry.RegisterLoader(new HatResourceLoader());
        factoryRegistry.RegisterLoader(new VisorResourceLoader());
        factoryRegistry.RegisterLoader(new NamePlateResourceLoader());

        var provider = new CosmeticsProvider(cosmeticsCatalog, factoryRegistry);
        Addressables.ResourceManager.ResourceProviders.Insert(0, new(provider.Pointer));

        var locator = new CosmeticsLocator(cosmeticsCatalog);
        Addressables.AddResourceLocator(new(locator.Pointer));

        Info("Starting default bundle download...");
        PluginCompat.QueueBundleDownload("https://github.com/XtraCube/CorsacCosmetics/releases/download/2.0.0/default.ccb");

        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        Info("Harmony patches installed");

        Message("Loaded Corsac Cosmetics Plugin!");
    }
}