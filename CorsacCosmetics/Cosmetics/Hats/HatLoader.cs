using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Cosmetics.Bundle;
using CorsacCosmetics.Tools;
using CorsacCosmetics.Unity;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Cosmetics.Hats;

public class HatLoader : BaseLoader
{
    public Dictionary<string, CustomHat> CustomHats { get; } = [];

    public override void InstallCosmetics(ReferenceData refData)
    {
        foreach (var (id, customHat) in CustomHats)
        {
            try
            {
                refData.hats.Add(customHat.HatData);
                Info($"Added {id} to HatManager");
            }
            catch (Exception e)
            {
                Error($"Failed to load hat {id} with exception:\n{e.ToString()}");
            }
        }
    }

    public override void LoadCosmetics(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Info($"Created hats directory at {directory}");
            return;
        }

        var hatFiles = Directory.GetFiles(directory, "*.png");

        foreach (var hatFile in hatFiles)
        {
            try
            {
                if (LoadHat(hatFile))
                {
                    Info($"Loaded hat from {hatFile}");
                }
                else
                {
                    Error($"Failed to load hat from {hatFile}");
                }
            }
            catch (Exception e)
            {
                Error($"Exception while loading hat from {hatFile}: {e.Message}");
            }
        }
    }

    public override bool LocateCosmetic(string id, string type, [NotNullWhen(true)] out Il2CppSystem.Type? il2CPPType)
    {
        il2CPPType = null;
        if (!CustomHats.ContainsKey(id))
        {
            return false;
        }

        il2CPPType = type == ReferenceType.HatViewData ? Il2CppType.Of<HatViewData>() : null;
        return il2CPPType != null;
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, string type)
    {
        if (!CustomHats.TryGetValue(id, out var hat))
        {
            return false;
        }

        // Lazy decode from bundle on first access. The bundle file stays open for the
        // lifetime of the mod, so subsequent decodes for this cosmetic are no-ops and
        // decodes for other cosmetics in the same bundle are cheap seeks.
        if (hat.BundleSource != null && hat.HatViewData.MainImage == null)
        {
            Info($"Decoding hat bundle for {id}");
            BundleDecoder.DecodeHat(hat.HatViewData, hat.PreviewData, hat.BundleSource);
        }

        switch (type)
        {
            case ReferenceType.Preview:
                Debug($"Found hat preview for {id}");
                handle.Complete(hat.PreviewData, true, null);
                return true;
            case ReferenceType.HatViewData:
                Debug($"Found hat view data for {id}");
                handle.Complete(hat.HatViewData, true, null);
                return true;
            default:
                Error("Unknown hat type");
                return false;
        }
    }

    public override bool ReleaseCosmetic(IResourceLocation location, Il2CppSystem.Object obj)
    {
        var (realKey, typeName) = HatLocator.GetIdAndType(location);
        if (realKey == null || typeName == null)
        {
            Error($"Invalid location {location.InternalId}, cannot release cosmetic");
            return false;
        }

        if (!CustomHats.TryGetValue(realKey, out var hat))
        {
            Info($"No custom hat found for key {realKey}, ignoring release request");
            return false;
        }

        switch (typeName)
        {
            case ReferenceType.Preview:
                Debug($"Releasing hat preview for {realKey}");
                hat.PreviewData.Unload();
                break;
            case ReferenceType.HatViewData:
                Debug($"Releasing hat view data for {realKey}");
                hat.HatViewData.Unload();
                break;
            default:
                Info($"Unknown type {typeName}, ignoring release request");
                break;
        }
        return true;
    }

    private bool LoadHat(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath);
        var metadataFile = Path.ChangeExtension(filePath, ".json");
        var metadata = new HatMetadata
        {
            Name = name
        };
        try
        {
            if (File.Exists(metadataFile))
            {
                var metadataJson = File.ReadAllText(metadataFile);
                metadata = JsonSerializer.Deserialize<HatMetadata>(metadataJson);
            }
            else
            {
                Warning($"No metadata file found for hat {name}, using defaults.");
            }
        }
        catch (Exception e)
        {
            Error($"Failed to load metadata for hat {name}: {e.Message}");
            return false;
        }

        var fullId = Names.Normalize(name, "hat");

        var hatSprite = SpriteTools.LoadSpriteFromFile(filePath);
        if (hatSprite == null)
        {
            Error($"Error loading hat {name}");
            return false;
        }

        hatSprite.DontUnload().DontDestroy();
        
        var hatViewData = ScriptableObject.CreateInstance<HatViewData>();
        hatViewData.name = metadata.Name;
        hatViewData.MatchPlayerColor = metadata.MatchPlayerColor;
        hatViewData.MainImage = hatSprite;

        var climbSpritePath = Path.ChangeExtension(filePath, ".climb");
        var climbSprite = SpriteTools.LoadSpriteFromFile(climbSpritePath);
        if (climbSprite != null)
        {
            Info($"Found climb sprite {climbSpritePath}");
            climbSprite.DontUnload().DontDestroy();
            hatViewData.ClimbImage = climbSprite;
        }
        
        var floorSpritePath = Path.ChangeExtension(filePath, ".floor");
        var floorSprite = SpriteTools.LoadSpriteFromFile(floorSpritePath);
        if (floorSprite != null)
        {
            Info($"Found floor sprite {floorSpritePath}");
            floorSprite.DontUnload().DontDestroy();
            hatViewData.FloorImage = floorSprite;
        }

        var backSpritePath = Path.ChangeExtension(filePath, ".back");
        var backSprite = SpriteTools.LoadSpriteFromFile(backSpritePath);
        if (backSprite != null)
        {
            Info($"Found back sprite {backSpritePath}");
            backSprite.DontUnload().DontDestroy();
            hatViewData.BackImage = backSprite;
        }

        var leftMainSpritePath = Path.ChangeExtension(filePath, ".left");
        var leftMainSprite = SpriteTools.LoadSpriteFromFile(leftMainSpritePath);
        if (leftMainSprite != null)
        {
            Info($"Found left main sprite {leftMainSpritePath}");
            leftMainSprite.DontUnload().DontDestroy();
            hatViewData.LeftMainImage = leftMainSprite;
        }

        var leftBackSpritePath = Path.ChangeExtension(filePath, ".leftback");
        var leftBackSprite = SpriteTools.LoadSpriteFromFile(leftBackSpritePath);
        if (leftBackSprite != null)
        {
            Info($"Found left back sprite {leftBackSpritePath}");
            leftBackSprite.DontUnload().DontDestroy();
            hatViewData.LeftBackImage = leftBackSprite;
        }

        var leftClimbSpritePath = Path.ChangeExtension(filePath, ".leftclimb");
        var leftClimbSprite = SpriteTools.LoadSpriteFromFile(leftClimbSpritePath);
        if (leftClimbSprite != null)
        {
            Info($"Found left climb sprite {leftClimbSpritePath}");
            leftClimbSprite.DontUnload().DontDestroy();
            hatViewData.LeftClimbImage = leftClimbSprite;
        }

        var leftFloorSpritePath = Path.ChangeExtension(filePath, ".leftfloor");
        var leftFloorSprite = SpriteTools.LoadSpriteFromFile(leftFloorSpritePath);
        if (leftFloorSprite != null)
        {
            Info($"Found left floor sprite {leftFloorSpritePath}");
            leftFloorSprite.DontUnload().DontDestroy();
            hatViewData.LeftFloorImage = leftFloorSprite;
        }

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = metadata.Name;
        previewData.PreviewSprite = hatSprite;

        var hatData = ScriptableObject.CreateInstance<HatData>();
        hatData.name = hatData.StoreName = metadata.Name;
        hatData.Free = true;
        hatData.ProductId = fullId;
        hatData.BlocksVisors = metadata.BlocksVisors;
        hatData.NoBounce = metadata.NoBounce;
        hatData.InFront = metadata.InFront;
        hatData.PreviewCrewmateColor = metadata.MatchPlayerColor;
        hatData.ViewDataRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.HatViewData));
        hatData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        var customHat = new CustomHat(fullId, hatData, hatViewData, previewData);
        CustomHats.Add(fullId, customHat);

        hatData.ViewDataRef.LoadAsset<HatViewData>();
        hatData.PreviewData.LoadAsset<PreviewViewData>();
        return true;
    }
}