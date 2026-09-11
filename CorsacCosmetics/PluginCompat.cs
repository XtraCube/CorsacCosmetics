using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Cosmetics.Sources;
using CorsacCosmetics.Tools;

namespace CorsacCosmetics;

/// <summary>
/// For mods that want to use Corsac to add cosmetics.
/// Static class with string fields in case you only want a soft-dependency.
/// </summary>
public static class PluginCompat
{
    internal static readonly Queue<IEnumerator> BeforeDiscoveryCoroutines = [];

    /// <summary>
    /// Adds a coroutine to run before Corsac discovers and installs cosmetics.
    /// </summary>
    /// <param name="coroutine">The coroutine to run.</param>
    public static void QueueDiscoveryCoroutine(IEnumerator coroutine)
    {
        BeforeDiscoveryCoroutines.Enqueue(coroutine);
    }

    /// <summary>
    /// Adds an asynchronous task to wait for before Corsac discovers and installs cosmetics.
    /// </summary>
    /// <param name="asyncTask">The task to wait upon.</param>
    public static void QueueDiscoveryTask(Task asyncTask)
    {
        BeforeDiscoveryCoroutines.Enqueue(asyncTask.AsIEnumerator());
    }

    /// <summary>
    /// Downloads a bundle from a URL and queues it to be added as a source before discovery.
    /// The bundle is saved to the CorsacCosmetics/Bundles folder with the filename from the URL.
    /// </summary>
    /// <param name="url">The URL of the .ccb bundle file to download.</param>
    public static void QueueBundleDownload(string url)
    {
        var outputFolder = CosmeticPaths.BundlePath;
        var outputPath = Path.Combine(outputFolder, Path.GetFileName(url));
        QueueDiscoveryTask(DownloadBundle(url, outputPath));
    }

    /// <summary>
    /// Downloads a bundle from a URL and saves it to the specified output path.
    /// </summary>
    /// <param name="url">The URL of the .ccb bundle file to download.</param>
    /// <param name="outputPath">The local file path where the bundle will be saved.</param>
    /// <returns>A task representing the asynchronous download operation.</returns>
    public static async Task DownloadBundle(string url, string outputPath)
    {
        try
        {
            using var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(outputPath, bytes);
            AddBundleSource(outputPath);
            Info($"Saved bundle from {url} to {outputPath}");
        }
        catch (Exception e)
        {
            Error(e.Message);
        }
    }

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