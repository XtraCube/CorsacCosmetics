using System.IO;
using CorsacCosmetics.Cosmetics.Sources;

namespace CorsacCosmetics;

/// <summary>
/// For mods that want to use Corsac to add cosmetics.
/// Static class with string fields in case you only want a soft-dependency.
/// </summary>
public static class PluginCompat
{
    /// <summary>
    /// Adds a new folder source to load cosmetics. Directory structure must match Corsac's default structure.
    /// </summary>
    /// <param name="cosmeticsFolder">The folder.</param>
    /// <param name="groupName">Optional group name.</param>
    public static void AddFolderSource(string cosmeticsFolder, string groupName = "default")
    {
        if (!Directory.Exists(cosmeticsFolder))
        {
            Error($"Cannot add local folder source, folder does not exist: {cosmeticsFolder}");
            return;
        }

        SourceRegistry.Instance.RegisterSource(new LocalFolderSource(cosmeticsFolder, groupName));
    }

    /// <summary>
    /// Adds a new bundle source using the given bundle file path.
    /// </summary>
    /// <param name="bundleFile">Path to .ccb file.</param>
    public static void AddBundleSource(string bundleFile)
    {
        if (!File.Exists(bundleFile))
        {
            Error($"Cannot add bundle source, bundle does not exist: {bundleFile}");
            return;
        }

        SourceRegistry.Instance.RegisterSource(new LocalBundleSource(bundleFile));
    }
}