using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CorsacCosmetics.Cosmetics.AssetReaders;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;

namespace CorsacCosmetics.Cosmetics.Sources;

public class LocalFolderSource(string basePath, string group = "default") : ICosmeticSource
{
    public string SourceId => "LocalFolder";

    public Task<IEnumerable<CosmeticDescriptor>> DiscoverAsync()
    {
        var descriptors = new List<CosmeticDescriptor>();
        var hatPath = Path.Combine(basePath, "Hats");
        var visorPath = Path.Combine(basePath, "Visors");
        var namePlatePath = Path.Combine(basePath, "NamePlates");
        Directory.CreateDirectory(hatPath);
        Directory.CreateDirectory(visorPath);
        Directory.CreateDirectory(namePlatePath);

        descriptors.AddRange(Directory.EnumerateFiles(hatPath, "*.png")
            .Select(pngFile => LoadCosmetic<HatMetadata>(pngFile, CosmeticType.Hat)));
        descriptors.AddRange(Directory.EnumerateFiles(visorPath, "*.png")
            .Select(pngFile => LoadCosmetic<VisorMetadata>(pngFile, CosmeticType.Visor)));
        descriptors.AddRange(Directory.EnumerateFiles(namePlatePath, "*.png")
            .Select(pngFile => LoadCosmetic<NamePlateMetadata>(pngFile, CosmeticType.NamePlate)));

        Info($"Loaded {descriptors.Count} cosmetics from {basePath}.");
        return Task.FromResult(descriptors.AsEnumerable());
    }

    private CosmeticDescriptor LoadCosmetic<T>(
        string filePath,
        CosmeticType cosmeticType
        ) where T : ICosmeticMetadata
    {
        var name = Path.GetFileNameWithoutExtension(filePath);
        var metadataFile = Path.ChangeExtension(filePath, ".json");
        var metadata = Activator.CreateInstance<T>();
        metadata.Name = name;

        try
        {
            if (File.Exists(metadataFile))
            {
                var metadataJson = File.ReadAllText(metadataFile);
                var newMetadata = JsonSerializer.Deserialize<T>(metadataJson);
                if (newMetadata != null)
                {
                    metadata = newMetadata;
                }
                else
                {
                    Error($"Failed to load metadata for hat {name}: {metadataJson}");
                }
            }
            else
            {
                Warning($"No metadata file found for hat {name}, using defaults.");
            }
        }
        catch (Exception e)
        {
            Error($"Failed to load metadata for hat {name}: {e.Message}");
        }

        return new CosmeticDescriptor(
            SourceId,
            group,
            name,
            cosmeticType,
            metadata,
            new FileAssetReader(filePath)
        );
    }
}