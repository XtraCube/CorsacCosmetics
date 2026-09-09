using System;
using System.IO;
using BepInEx;

namespace CorsacCosmetics.Cosmetics;

public static class CosmeticPaths
{
    public static string StarlightPath => Environment.GetEnvironmentVariable("STAR_DATA_PATH")!;
    
    public static string BasePath { get; } = Path.Combine(
        OperatingSystem.IsAndroid() ? StarlightPath : Paths.GameRootPath,
        "CorsacCosmetics"
    );

    public static string BundlePath { get; } = Path.Combine(BasePath, "Bundles");

    public static string HatPath { get; } = Path.Combine(BasePath, "Hats");

    public static string VisorPath { get; } = Path.Combine(BasePath, "Visors");

    public static string NameplatePath { get; } = Path.Combine(BasePath, "Nameplates");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(BundlePath);
        Directory.CreateDirectory(HatPath);
        Directory.CreateDirectory(VisorPath);
        Directory.CreateDirectory(NameplatePath);
    }
}