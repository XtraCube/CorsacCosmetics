using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;
using CorsacCosmetics.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

        var hatData = ScriptableObject.CreateInstance<HatData>();
        hatData.name = hatData.StoreName = manifest.Name;
        hatData.Free = true;
        hatData.ProductId = id;
        hatData.BlocksVisors = manifest.BlocksVisors;
        hatData.NoBounce = manifest.NoBounce;
        hatData.InFront = manifest.InFront;
        hatData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.HatViewData));
        hatData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        var bundleSource = new BundleSource(bundlePath, start);
        bundleSource.AddSprite("MainSprite", manifest.MainSprite);
        bundleSource.AddSprite("BackSprite", manifest.BackSprite);
        bundleSource.AddSprite("ClimbSprite", manifest.ClimbSprite);
        bundleSource.AddSprite("FloorSprite", manifest.FloorSprite);
        bundleSource.AddSprite("LeftMainSprite", manifest.LeftMainSprite);
        bundleSource.AddSprite("LeftBackSprite", manifest.LeftBackSprite);
        bundleSource.AddSprite("LeftClimbSprite", manifest.LeftClimbSprite);
        bundleSource.AddSprite("LeftFloorSprite", manifest.LeftFloorSprite);
        bundleSource.AddSprite("PreviewSprite", manifest.PreviewSprite);

        var customHat = new CustomHat(id, hatData, CreatePreviewViewData, CreateHatViewData);
        hatLoader.CustomHats.Add(id, customHat);
        return;

        PreviewViewData CreatePreviewViewData()
        {
            return BundleDecoder.DecodePreview(bundleSource);
        }

        HatViewData CreateHatViewData()
        {
            return BundleDecoder.DecodeHat(bundleSource);
        }
    }

    private void LoadVisor(VisorManifest manifest, long start, string groupName, string bundlePath)
    {
        var id = Names.Normalize(manifest.Name, "visor", groupName);
        var visorData = ScriptableObject.CreateInstance<VisorData>();
        visorData.name = manifest.Name;
        visorData.Free = true;
        visorData.ProductId = id;
        visorData.behindHats = manifest.BehindHats;
        visorData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.VisorViewData));
        visorData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        var bundleSource = new BundleSource(bundlePath, start);
        bundleSource.AddSprite("IdleSprite", manifest.IdleSprite);
        bundleSource.AddSprite("LeftIdleSprite", manifest.LeftIdleSprite);
        bundleSource.AddSprite("FloorSprite", manifest.FloorSprite);
        bundleSource.AddSprite("ClimbSprite", manifest.ClimbSprite);
        bundleSource.AddSprite("PreviewSprite", manifest.PreviewSprite);

        var customVisor = new CustomVisor(id, visorData, CreatePreviewViewData, CreateVisorViewData);
        visorLoader.CustomVisors.Add(id, customVisor);
        return;

        PreviewViewData CreatePreviewViewData()
        {
            return BundleDecoder.DecodePreview(bundleSource);
        }

        VisorViewData CreateVisorViewData()
        {
            return BundleDecoder.DecodeVisor(bundleSource);
        }
    }

    private void LoadNameplate(NameplateManifest manifest, long start, string groupName, string bundlePath)
    {
        var id = Names.Normalize(manifest.Name, "nameplate", groupName);
        var namePlateData = ScriptableObject.CreateInstance<NamePlateData>();
        namePlateData.name = manifest.Name;
        namePlateData.Free = true;
        namePlateData.ProductId = id;
        namePlateData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.NamePlateViewData));
        namePlateData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        var bundleSource = new BundleSource(bundlePath, start);
        bundleSource.AddSprite("NameplateSprite", manifest.NameplateSprite);
        bundleSource.AddSprite("PreviewSprite", manifest.PreviewSprite);

        var customNamePlate = new CustomNamePlate(id, namePlateData, CreatePreviewViewData, CreateNamePlateViewData);
        nameplateLoader.CustomNamePlates.Add(id, customNamePlate);
        return;

        PreviewViewData CreatePreviewViewData()
        {
            return BundleDecoder.DecodePreview(bundleSource);
        }

        NamePlateViewData CreateNamePlateViewData()
        {
            return BundleDecoder.DecodeNameplate(bundleSource);
        }
    }
}