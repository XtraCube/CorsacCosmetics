using System;
using System.Linq;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace CorsacCosmetics.Tools;

public static class ReactorCompat
{
    public const string ReactorID = "gg.reactor.api";
    private static Func<int, bool> ShowCredits { get; } = location => location == 0;

    public static void RegisterCredits()
    {
        try
        {
            if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(ReactorID, out var value))
            {
                Info("Reactor not found, skipping credits registration.");
                return;
            }

            var reactorPlugin = (value.Instance as BasePlugin)!;
            var reactorAssembly = reactorPlugin.GetType().Assembly;
            var reactorTypes = AccessTools.GetTypesFromAssembly(reactorAssembly);
            var reactorCreds = reactorTypes.First(t => t.Name == "ReactorCredits");
            var registerMethod = AccessTools
                .GetDeclaredMethods(reactorCreds)
                .Single(m => m.Name == "Register" && m.IsGenericMethodDefinition)
                ?.MakeGenericMethod(typeof(CorsacCosmeticsPlugin));

            var showCreditsType = registerMethod!.GetParameters().First().ParameterType;
            var showCreditsDelegate = Delegate.CreateDelegate(showCreditsType, ShowCredits.Target, ShowCredits.Method);
            registerMethod.Invoke(null, [showCreditsDelegate]);
            Message("Reactor was detected and Corsac was registered successfully.");
        }
        catch (Exception e)
        {
            Error($"Could not register credits with Reactor! An exception was thrown:\n{e}");
        }
    }
}