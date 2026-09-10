using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;

namespace CorsacCosmetics.Cosmetics.Bundle.V2;

public class BundleLoaderV2(
    HatLoader hatLoader, 
    VisorLoader visorLoader, 
    NameplateLoader nameplateLoader,
    Dictionary<string, string> groupNames
    )
{
    public bool LoadBundle(string file)
    {
        if (!File.Exists(file))
        {
            Error($"Bundle file {file} does not exist! Skipping bundle.");
            return false;
        }

        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        var header = BundleHeaderV2.Read(fs);

        if (!header.IsValid)
        {
            Error($"File {file} is not a valid bundle. Skipping bundle.");
            return false;
        }

        if (!header.IsSupportedVersion)
        {
            Error($"Bundle version {header.Version} is not supported. Skipping bundle.");
            return false;
        }

        var manifestBytes = new byte[header.ManifestLength];
        if (fs.Read(manifestBytes.AsSpan()) != header.ManifestLength)
        {
            Error($"Could not read the full manifest from {file}. Skipping bundle.");
            return false;
        }

        var manifest = JsonSerializer.Deserialize(manifestBytes, BundleManifestV2Context.Default.BundleManifestV2);
        if (manifest.Groups == null)
        {
            Error($"Manifest in {file} does not contain any groups. Skipping bundle.");
            return false;
        }

        var start = fs.Position;
        
        var filename = Path.GetFileNameWithoutExtension(file).Replace(".", "-");

        foreach (var group in manifest.Groups)
        {
            var name = $"{filename}-{group.Name}";
            if (group.Name == "Custom Cosmetics")
            {
                name = "default";
            }
            else
            {
                groupNames[name] = group.Name;
            }
            
            foreach (var hatManifest in group.Hats)
            {
                LoadHat(hatManifest, start, name, file);
                Info($"Loaded {hatManifest.Name} from bundle");
            }

            foreach (var visorManifest in group.Visors)
            {
                LoadVisor(visorManifest, start, name, file);
                Info($"Loaded {visorManifest.Name} from bundle");
            }

            foreach (var nameplateManifest in group.Nameplates)
            {
                LoadNameplate(nameplateManifest, start, name, file);
                Info($"Loaded {nameplateManifest.Name} from bundle");
            }
        }

        return true;
    }

    private void LoadHat(HatManifest manifest, long start, string groupName, string bundlePath)
    {
        var id = Names.Normalize(manifest.Name, "hat", groupName);
        var hatData = new HatDataBuilder()
            .SetId(id)
            .SetName(manifest.Name)
            .SetBlocksVisors(manifest.BlocksVisors)
            .SetInFront(manifest.InFront)
            .SetNoBounce(manifest.NoBounce)
            .SetMatchPlayerColor(manifest.MatchPlayerColor)
            .Build();

        var bundleSource = new BundleSource(bundlePath, start);
        bundleSource.AddSprite("MainSprite", manifest.MainSprite);
        bundleSource.AddSprite("BackSprite", manifest.BackSprite);
        bundleSource.AddSprite("ClimbSprite", manifest.ClimbSprite);
        bundleSource.AddSprite("FloorSprite", manifest.FloorSprite);
        bundleSource.AddSprite("LeftMainSprite", manifest.LeftMainSprite);
        bundleSource.AddSprite("LeftBackSprite", manifest.LeftBackSprite);
        bundleSource.AddSprite("LeftClimbSprite", manifest.LeftClimbSprite);
        bundleSource.AddSprite("LeftFloorSprite", manifest.LeftFloorSprite);

        var previewSprite = manifest.PreviewSprite.HasData ? manifest.PreviewSprite : manifest.MainSprite;
        bundleSource.AddSprite("PreviewSprite", previewSprite);

        var customHat = new CustomHat(id, hatData, CreatePreviewViewData, CreateHatViewData);
        hatLoader.CustomHats.Add(id, customHat);
        return;

        PreviewViewData CreatePreviewViewData()
        {
            return BundleDecoder.DecodePreview(bundleSource);
        }

        HatViewData CreateHatViewData()
        {
            return BundleDecoder.DecodeHat(bundleSource, manifest.MatchPlayerColor);
        }
    }

    private void LoadVisor(VisorManifest manifest, long start, string groupName, string bundlePath)
    {
        var id = Names.Normalize(manifest.Name, "visor", groupName);
        var visorData = new VisorDataBuilder()
            .SetId(id)
            .SetName(manifest.Name)
            .SetBehindHats(manifest.BehindHats)
            .SetMatchPlayerColor(manifest.MatchPlayerColor)
            .Build();

        var bundleSource = new BundleSource(bundlePath, start);
        bundleSource.AddSprite("IdleSprite", manifest.IdleSprite);
        bundleSource.AddSprite("LeftIdleSprite", manifest.LeftIdleSprite);
        bundleSource.AddSprite("FloorSprite", manifest.FloorSprite);
        bundleSource.AddSprite("ClimbSprite", manifest.ClimbSprite);

        var previewSprite = manifest.PreviewSprite.HasData ? manifest.PreviewSprite : manifest.IdleSprite;
        bundleSource.AddSprite("PreviewSprite", previewSprite);

        var customVisor = new CustomVisor(id, visorData, CreatePreviewViewData, CreateVisorViewData);
        visorLoader.CustomVisors.Add(id, customVisor);
        return;

        PreviewViewData CreatePreviewViewData()
        {
            return BundleDecoder.DecodePreview(bundleSource);
        }

        VisorViewData CreateVisorViewData()
        {
            return BundleDecoder.DecodeVisor(bundleSource, manifest.MatchPlayerColor);
        }
    }

    private void LoadNameplate(NameplateManifest manifest, long start, string groupName, string bundlePath)
    {
        var id = Names.Normalize(manifest.Name, "nameplate", groupName);
        var namePlateData = new NamePlateDataBuilder()
            .SetId(id)
            .SetName(manifest.Name)
            .Build();

        var bundleSource = new BundleSource(bundlePath, start);
        bundleSource.AddSprite("NameplateSprite", manifest.NameplateSprite);

        var customNamePlate = new CustomNamePlate(id, namePlateData, CreateNamePlateViewData);
        nameplateLoader.CustomNamePlates.Add(id, customNamePlate);
        return;

        NamePlateViewData CreateNamePlateViewData()
        {
            return BundleDecoder.DecodeNameplate(bundleSource);
        }
    }
}