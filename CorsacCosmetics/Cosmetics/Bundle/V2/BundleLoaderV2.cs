using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;
using CorsacCosmetics.Tools;
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
                LoadHat(hatManifest, fs, start, name);
                Info($"Loaded {hatManifest.Name} from bundle");
            }

            foreach (var visorManifest in group.Visors)
            {
                LoadVisor(visorManifest, fs, start, name);
                Info($"Loaded {visorManifest.Name} from bundle");
            }

            foreach (var nameplateManifest in group.Nameplates)
            {
                LoadNameplate(nameplateManifest, fs, start, name);
                Info($"Loaded {nameplateManifest.Name} from bundle");
            }
        }

        return true;
    }

    private void LoadHat(HatManifest manifest, FileStream fs, long start, string groupName)
    {
        var id = Names.Normalize(manifest.Name, "hat", groupName);

        var hatViewData = ScriptableObject.CreateInstance<HatViewData>();
        hatViewData.name = manifest.Name;
        hatViewData.MatchPlayerColor = manifest.MatchPlayerColor;
        hatViewData.MainImage = SpriteTools.LoadSpriteFromStream(fs, start + manifest.MainSprite.Offset, manifest.MainSprite.Size);
        hatViewData.BackImage = SpriteTools.LoadSpriteFromStream(fs, start + manifest.BackSprite.Offset, manifest.BackSprite.Size);
        hatViewData.ClimbImage = SpriteTools.LoadSpriteFromStream(fs, start + manifest.ClimbSprite.Offset, manifest.ClimbSprite.Size);
        hatViewData.FloorImage = SpriteTools.LoadSpriteFromStream(fs, start + manifest.FloorSprite.Offset, manifest.FloorSprite.Size);
        hatViewData.LeftMainImage = SpriteTools.LoadSpriteFromStream(fs, start + manifest.LeftMainSprite.Offset, manifest.LeftMainSprite.Size);
        hatViewData.LeftBackImage = SpriteTools.LoadSpriteFromStream(fs, start + manifest.LeftBackSprite.Offset, manifest.LeftBackSprite.Size);
        hatViewData.LeftClimbImage = SpriteTools.LoadSpriteFromStream(fs, start + manifest.LeftClimbSprite.Offset, manifest.LeftClimbSprite.Size);
        hatViewData.LeftFloorImage = SpriteTools.LoadSpriteFromStream(fs, start + manifest.LeftFloorSprite.Offset, manifest.LeftFloorSprite.Size);

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = manifest.Name;
        previewData.PreviewSprite = SpriteTools.LoadSpriteFromStream(fs, start + manifest.PreviewSprite.Offset, manifest.PreviewSprite.Size);
        if (!previewData.PreviewSprite)
        {
            previewData.PreviewSprite = hatViewData.MainImage;
        }

        var hatData = ScriptableObject.CreateInstance<HatData>();
        hatData.name = hatData.StoreName = manifest.Name;
        hatData.Free = true;
        hatData.ProductId = id;
        hatData.BlocksVisors = manifest.BlocksVisors;
        hatData.NoBounce = manifest.NoBounce;
        hatData.InFront = manifest.InFront;
        hatData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.HatViewData));
        hatData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        var customHat = new CustomHat(id, hatData, hatViewData, previewData);
        hatLoader.CustomHats.Add(id, customHat);

        hatData.ViewDataRef.LoadAsset<HatViewData>();
        hatData.PreviewData.LoadAsset<PreviewViewData>();
    }

    private void LoadVisor(VisorManifest manifest, FileStream fs, long start, string groupName)
    {
        var id = Names.Normalize(manifest.Name, "visor", groupName);

        var visorViewData = ScriptableObject.CreateInstance<VisorViewData>();
        visorViewData.name = manifest.Name;
        visorViewData.MatchPlayerColor = manifest.MatchPlayerColor;
        visorViewData.IdleFrame = SpriteTools.LoadSpriteFromStream(fs, start + manifest.IdleSprite.Offset, manifest.IdleSprite.Size);
        visorViewData.LeftIdleFrame = SpriteTools.LoadSpriteFromStream(fs, start + manifest.LeftIdleSprite.Offset, manifest.LeftIdleSprite.Size);
        visorViewData.FloorFrame = SpriteTools.LoadSpriteFromStream(fs, start + manifest.FloorSprite.Offset, manifest.FloorSprite.Size);
        visorViewData.ClimbFrame = SpriteTools.LoadSpriteFromStream(fs, start + manifest.ClimbSprite.Offset, manifest.ClimbSprite.Size);

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = manifest.Name;
        previewData.PreviewSprite = SpriteTools.LoadSpriteFromStream(fs, start + manifest.PreviewSprite.Offset, manifest.PreviewSprite.Size);
        if (!previewData.PreviewSprite)
        {
            previewData.PreviewSprite = visorViewData.IdleFrame;
        }

        var visorData = ScriptableObject.CreateInstance<VisorData>();
        visorData.name = manifest.Name;
        visorData.Free = true;
        visorData.ProductId = id;
        visorData.behindHats = manifest.BehindHats;
        visorData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.VisorViewData));
        visorData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        var customVisor = new CustomVisor(id, visorData, visorViewData, previewData);
        visorLoader.CustomVisors.Add(id, customVisor);

        visorData.ViewDataRef.LoadAsset<VisorViewData>();
        visorData.PreviewData.LoadAsset<PreviewViewData>();
    }

    private void LoadNameplate(NameplateManifest manifest, FileStream fs, long start, string groupName)
    {
        var id = Names.Normalize(manifest.Name, "nameplate", groupName);

        var namePlateViewData = ScriptableObject.CreateInstance<NamePlateViewData>();
        namePlateViewData.name = manifest.Name;
        namePlateViewData.Image = SpriteTools.LoadSpriteFromStream(fs, start + manifest.NameplateSprite.Offset, manifest.NameplateSprite.Size);

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = manifest.Name;
        previewData.PreviewSprite = SpriteTools.LoadSpriteFromStream(fs, start + manifest.PreviewSprite.Offset, manifest.PreviewSprite.Size);
        if (!previewData.PreviewSprite)
        {
            previewData.PreviewSprite = namePlateViewData.Image;
        }

        var namePlateData = ScriptableObject.CreateInstance<NamePlateData>();
        namePlateData.name = manifest.Name;
        namePlateData.Free = true;
        namePlateData.ProductId = id;
        namePlateData.ViewDataRef = new AssetReference(HatLocator.GetGuid(id, ReferenceType.NamePlateViewData));
        namePlateData.PreviewData = new AssetReference(HatLocator.GetGuid(id, ReferenceType.Preview));

        var customNamePlate = new CustomNamePlate(id, namePlateData, namePlateViewData, previewData);
        nameplateLoader.CustomNamePlates.Add(id, customNamePlate);

        namePlateData.ViewDataRef.LoadAsset<NamePlateViewData>();
        namePlateData.PreviewData.LoadAsset<PreviewViewData>();
    }
}