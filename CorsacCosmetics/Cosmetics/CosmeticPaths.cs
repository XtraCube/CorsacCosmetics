using System;
using System.IO;
using BepInEx;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics;

public static class CosmeticPaths
{
    public static string BasePath { get; } = Path.Combine(
        OperatingSystem.IsAndroid() ? Application.persistentDataPath : Paths.GameRootPath,
        "CorsacCosmetics"
    );

    public static string HatPath { get; } = Path.Combine(BasePath, "Hats");

    public static string VisorPath { get; } = Path.Combine(BasePath, "Visors");

    public static string NameplatePath { get; } = Path.Combine(BasePath, "Nameplates");
}