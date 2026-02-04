global using static Reactor.Utilities.Logger<CorsacCosmetics.CorsacCosmeticsPlugin>;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using CorsacCosmetics.Loader;
using CorsacCosmetics.Unity;
using HarmonyLib;
using Reactor;

namespace CorsacCosmetics;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
public partial class CorsacCosmeticsPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        Message("Loading Corsac Cosmetics Plugin...");

        Info("Initializing HatProvider...");
        HatProvider.Initialize();
        Info("HatProvider initialized!");
        
        Info("Initializing HatLocator...");
        HatLocator.Initialize();
        Info("HatLocator initialized!");

        Info("Loading Harmony patches...");
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        Info("Harmony patches loaded!");
        
        Message("Loaded Corsac Cosmetics Plugin!");
    }
}