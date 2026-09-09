using System;
using System.Reflection;
using CorsacCosmetics.Tools;
using UnityEngine;

namespace CorsacCosmetics;

public static class Assets
{
    public static Sprite NextButton { get; private set; } = null!;
    
    public static Sprite NextButtonActive { get; private set; } = null!;

    public static void Initialize()
    {
        try
        {
            var assembly = Assembly.GetAssembly(typeof(CorsacCosmeticsPlugin))
                ?? throw new Exception("Assembly could not be found.");

            {
                using var stream = assembly.GetManifestResourceStream("CorsacCosmetics.Resources.NextButton.png")
                    ?? throw new Exception("Could not load NextButton! Resource stream was null.");
                NextButton = SpriteTools.LoadSpriteFromStream(stream)
                    ?? throw new Exception("Could not load NextButton! Sprite was null.");
                NextButton.DontUnload().DontDestroy();
            }

            {
                using var stream = assembly.GetManifestResourceStream("CorsacCosmetics.Resources.NextButtonActive.png")
                    ?? throw new Exception("Could not load NextButtonActive! Resource stream was null.");
                NextButtonActive = SpriteTools.LoadSpriteFromStream(stream)
                    ?? throw new Exception("Could not load NextButtonActive! Sprite was null.");
                NextButtonActive.DontUnload().DontDestroy();
            }
        }
        catch (Exception e)
        {
            Error($"Could not load assets! An exception was thrown:\n{e}");
        }
    }
}