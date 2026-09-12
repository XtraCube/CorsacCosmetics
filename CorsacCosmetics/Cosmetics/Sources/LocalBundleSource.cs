using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using CorsacCosmetics.Cosmetics.AssetReaders;
using CorsacCosmetics.Cosmetics.Bundle;
using CorsacCosmetics.Cosmetics.Bundle.V2;

namespace CorsacCosmetics.Cosmetics.Sources;

public class LocalBundleSource(string bundleFile) : ICosmeticSource
{
    public string SourceId => "LocalBundle";

    private const string DefaultGroup = "default";

    public Task<IEnumerable<CosmeticDescriptor>> DiscoverAsync()
    {
        var cosmetics = new List<CosmeticDescriptor>();

        if (!File.Exists(bundleFile))
        {
            Error($"Bundle file {bundleFile} does not exist! Skipping bundle.");
            return Task.FromResult(cosmetics.AsEnumerable());
        }

        TryV1Bundle(cosmetics);
        TryV2Bundle(cosmetics);
        Info($"Loaded {cosmetics.Count} cosmetics from {bundleFile}");
        return Task.FromResult(cosmetics.AsEnumerable());
    }

    private void TryV1Bundle(List<CosmeticDescriptor> cosmetics)
    {
        using var fs = new FileStream(bundleFile, FileMode.Open, FileAccess.Read);
        var header = BundleHeader.Read(fs);

        if (!header.IsValid)
        {
            Error($"File {bundleFile} is not a valid bundle. Skipping bundle.");
            return;
        }

        if (!header.IsSupportedVersion)
        {
            Debug($"Bundle version {header.Version} is not supported by V1 loader! Skipping bundle.");
            return;
        }
        
        var manifestBytes = new byte[header.ManifestLength];
        if (fs.Read(manifestBytes.AsSpan()) != header.ManifestLength)
        {
            Error("Could not read full bundle manifest!");
            return;
        }

        var manifest = JsonSerializer.Deserialize(manifestBytes, BundleManifestContext.Default.BundleManifest);
        if (manifest.Hats == null)
        {
            Error("Bundle data cannot be null!");
            return;
        }

        var sourceId = new Guid(MD5.HashData(manifestBytes)).ToString();
        var start = fs.Position;

        foreach (var hatManifest in manifest.Hats)
        {
            cosmetics.Add(new CosmeticDescriptor(
                sourceId,
                DefaultGroup,
                hatManifest.Name,
                CosmeticType.Hat,
                hatManifest.ToMetadata(),
                new BundleAssetReader(hatManifest.ToBundleSource(bundleFile, start))
            ));
            Debug($"Loaded {hatManifest.Name} from bundle");
        }

        foreach (var visorManifest in manifest.Visors)
        {
            cosmetics.Add(new CosmeticDescriptor(
                sourceId,
                DefaultGroup,
                visorManifest.Name,
                CosmeticType.Visor,
                visorManifest.ToMetadata(),
                new BundleAssetReader(visorManifest.ToBundleSource(bundleFile, start))
            ));
            Debug($"Loaded {visorManifest.Name} from bundle");
        }

        foreach (var nameplateManifest in manifest.Nameplates)
        {
            cosmetics.Add(new CosmeticDescriptor(
                sourceId,
                DefaultGroup,
                nameplateManifest.Name,
                CosmeticType.NamePlate,
                nameplateManifest.ToMetadata(),
                new BundleAssetReader(nameplateManifest.ToBundleSource(bundleFile, start))
            ));
            Debug($"Loaded {nameplateManifest.Name} from bundle");
        }
    }

    private void TryV2Bundle(List<CosmeticDescriptor> cosmetics)
    {
        using var fs = new FileStream(bundleFile, FileMode.Open, FileAccess.Read);
        var header = BundleHeaderV2.Read(fs);

        if (!header.IsValid)
        {
            Error($"File {bundleFile} is not a valid bundle. Skipping bundle.");
            return;
        }

        if (!header.IsSupportedVersion)
        {
            Debug($"Bundle version {header.Version} is not supported by V2 loader! Skipping bundle.");
            return;
        }

        var manifestBytes = new byte[header.ManifestLength];
        if (fs.Read(manifestBytes.AsSpan()) != header.ManifestLength)
        {
            Error("Could not read full bundle manifest!");
            return;
        }

        var manifest = JsonSerializer.Deserialize(manifestBytes, BundleManifestV2Context.Default.BundleManifestV2);
        if (manifest.Groups == null)
        {
            Error("Bundle data cannot be null!");
            return;
        }

        var sourceId = new Guid(MD5.HashData(manifestBytes)).ToString();
        var start = fs.Position;

        foreach (var group in manifest.Groups)
        {
            foreach (var hatManifest in group.Hats)
            {
                cosmetics.Add(new CosmeticDescriptor(
                    sourceId,
                    group.Name,
                    hatManifest.Name,
                    CosmeticType.Hat,
                    hatManifest.ToMetadata(),
                    new BundleAssetReader(hatManifest.ToBundleSource(bundleFile, start))
                ));
                Debug($"Loaded {hatManifest.Name} from bundle");
            }

            foreach (var visorManifest in group.Visors)
            {
                cosmetics.Add(new CosmeticDescriptor(
                    sourceId,
                    group.Name,
                    visorManifest.Name,
                    CosmeticType.Visor,
                    visorManifest.ToMetadata(),
                    new BundleAssetReader(visorManifest.ToBundleSource(bundleFile, start))
                ));
                Debug($"Loaded {visorManifest.Name} from bundle");
            }

            foreach (var nameplateManifest in group.Nameplates)
            {
                cosmetics.Add(new CosmeticDescriptor(
                    sourceId,
                    group.Name,
                    nameplateManifest.Name,
                    CosmeticType.NamePlate,
                    nameplateManifest.ToMetadata(),
                    new BundleAssetReader(nameplateManifest.ToBundleSource(bundleFile, start))
                ));
                Debug($"Loaded {nameplateManifest.Name} from bundle");
            }
        }
    }
}