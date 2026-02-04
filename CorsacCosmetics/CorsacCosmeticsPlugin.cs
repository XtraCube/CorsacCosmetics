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
        Info("Loading Corsac Cosmetics Plugin...");

        Message("Initializing HatProvider...");
        HatProvider.Initialize();
        Message("HatProvider initialized!");
        
        Message("Initializing HatLocator...");
        HatLocator.Initialize();
        Message("HatLocator initialized!");
        
        Message("Loading hats...");
        HatLoader.Instance.LoadHats();
        Message("Hats loaded!");

        Message("Loading Harmony patches...");
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
        Message("Harmony patches loaded!");
        
        Info("Loaded Corsac Cosmetics Plugin!");
    }
}