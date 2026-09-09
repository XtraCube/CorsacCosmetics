using System;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;
using CorsacCosmetics.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Cosmetics.Bundle;

public class BundleLoader(HatLoader hatLoader, VisorLoader visorLoader, NameplateLoader nameplateLoader)
{
    public void LoadBundles(string directory)
    {
        foreach (var file in Directory.GetFiles(directory, "*.ccb"))
        {
            try
            {
                Info($"Loading bundle from {file}");
                LoadBundle(file);
            }
            catch (Exception e)
            {
                Error($"Error loading bundle from {file}:\n{e}");
            }
        }
    }

    public bool LoadBundle(string file)
    {
        if (!File.Exists(file))
        {
            Error($"Bundle file {file} does not exist! Skipping bundle.");
            return false;
        }

        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        var header = BundleHeader.Read(fs);

        if (!header.IsValid)
        {
            Error($"File {file} is not a valid bundle. Skipping bundle.");
            return false;
        }

        if (!header.IsSupportedVersion)
        {
            Debug($"Bundle version {header.Version} is not supported by V1 loader! Skipping bundle.");
            return false;
        }

        var manifestBytes = new byte[header.ManifestLength];
        if (fs.Read(manifestBytes.AsSpan()) != header.ManifestLength)
        {
            Error("Could not read full bundle manifest!");
            return false;
        }

        var manifest = JsonSerializer.Deserialize(manifestBytes, BundleManifestContext.Default.BundleManifest);
        if (manifest.Hats == null)
        {
            Error("Bundle data cannot be null!");
            return false;
        }

        var start = fs.Position;

        foreach (var hatManifest in manifest.Hats)
        {
            LoadHat(hatManifest, fs, start, file);
            Info($"Loaded {hatManifest.Name} from bundle");
        }

        foreach (var visorManifest in manifest.Visors)
        {
            LoadVisor(visorManifest, fs, start, file);
            Info($"Loaded {visorManifest.Name} from bundle");
        }

        foreach (var nameplateManifest in manifest.Nameplates)
        {
            LoadNameplate(nameplateManifest, fs, start, file);
            Info($"Loaded {nameplateManifest.Name} from bundle");
        }

        return true;
    }

    private void LoadHat(HatManifest manifest, FileStream fs, long start, string bundlePath)
    {
        var id = Names.Normalize(manifest.Name, "hat", "default.bundle");

        var hatViewData = ScriptableObject.CreateInstance<HatViewData>();
        hatViewData.name = manifest.Name;
        hatViewData.MatchPlayerColor = manifest.MatchPlayerColor;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = manifest.Name;

        var hatData = ScriptableObject.CreateInstance<HatData>();
        hatData.name = hatData.StoreName = manifest.Name;
        hatData.Free = true;
        hatData.ProductId = id;
        hatData.BlocksVisors = manifest.BlocksVisors;
        hatData.NoBounce = manifest.NoBounce;
        hatData.InFront = manifest.InFront;
        hatData.PreviewCrewmateColor = manifest.MatchPlayerColor;
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

        var customHat = new CustomHat(id, hatData, hatViewData, previewData, bundleSource);
        hatLoader.CustomHats.Add(id, customHat);
    }

    private void LoadVisor(VisorManifest manifest, FileStream fs, long start, string bundlePath)
    {
        var id = Names.Normalize(manifest.Name, "visor", "default.bundle");

        var visorViewData = ScriptableObject.CreateInstance<VisorViewData>();
        visorViewData.name = manifest.Name;
        visorViewData.MatchPlayerColor = manifest.MatchPlayerColor;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = manifest.Name;

        var visorData = ScriptableObject.CreateInstance<VisorData>();
        visorData.name = manifest.Name;
        visorData.Free = true;
        visorData.ProductId = id;
        visorData.behindHats = manifest.BehindHats;
        visorData.PreviewCrewmateColor = manifest.MatchPlayerColor;
        visorData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.VisorViewData));
        visorData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        var bundleSource = new BundleSource(bundlePath, start);
        bundleSource.AddSprite("IdleSprite", manifest.IdleSprite);
        bundleSource.AddSprite("LeftIdleSprite", manifest.LeftIdleSprite);
        bundleSource.AddSprite("FloorSprite", manifest.FloorSprite);
        bundleSource.AddSprite("ClimbSprite", manifest.ClimbSprite);
        bundleSource.AddSprite("PreviewSprite", manifest.PreviewSprite);

        var customVisor = new CustomVisor(id, visorData, visorViewData, previewData, bundleSource);
        visorLoader.CustomVisors.Add(id, customVisor);
    }

    private void LoadNameplate(NameplateManifest manifest, FileStream fs, long start, string bundlePath)
    {
        var id = Names.Normalize(manifest.Name, "nameplate", "default.bundle");

        var namePlateViewData = ScriptableObject.CreateInstance<NamePlateViewData>();
        namePlateViewData.name = manifest.Name;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = manifest.Name;

        var namePlateData = ScriptableObject.CreateInstance<NamePlateData>();
        namePlateData.name = manifest.Name;
        namePlateData.Free = true;
        namePlateData.ProductId = id;
        namePlateData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.NamePlateViewData));
        namePlateData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        var bundleSource = new BundleSource(bundlePath, start);
        bundleSource.AddSprite("NameplateSprite", manifest.NameplateSprite);
        bundleSource.AddSprite("PreviewSprite", manifest.PreviewSprite);

        var customNamePlate = new CustomNamePlate(id, namePlateData, namePlateViewData, previewData, bundleSource);
        nameplateLoader.CustomNamePlates.Add(id, customNamePlate);
    }
}