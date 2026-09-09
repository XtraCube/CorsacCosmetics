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

namespace CorsacCosmetics.Cosmetics.Visors;

public class VisorLoader : BaseLoader
{
    public Dictionary<string, CustomVisor> CustomVisors { get; } = [];

    public override void InstallCosmetics(ReferenceData refData)
    {
        foreach (var (id, customVisor) in CustomVisors)
        {
            try
            {
                refData.visors.Add(customVisor.VisorData);
                Info($"Added visor {id} to HatManager");
            }
            catch (Exception e)
            {
                Error($"Failed to load visor {id} with exception:\n{e.ToString()}");
            }
        }
    }

    public override void LoadCosmetics(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Info($"Created visors directory at {directory}");
            return;
        }

        var visorFiles = Directory.GetFiles(directory, "*.png");

        foreach (var visorFile in visorFiles)
        {
            try
            {
                if (LoadVisor(visorFile))
                {
                    Info($"Loaded visor from {visorFile}");
                }
                else
                {
                    Error($"Failed to load visor from {visorFile}");
                }
            }
            catch (Exception e)
            {
                Error($"Exception while loading visor from {visorFile}: {e.Message}");
            }
        }
    }

    public override bool LocateCosmetic(string id, string type, [NotNullWhen(true)] out Il2CppSystem.Type? il2CPPType)
    {
        il2CPPType = null;
        if (!CustomVisors.ContainsKey(id))
        {
            return false;
        }

        il2CPPType = type == ReferenceType.VisorViewData ? Il2CppType.Of<VisorViewData>() : null;
        return il2CPPType != null;
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, string type)
    {
        if (!CustomVisors.TryGetValue(id, out var visor))
        {
            return false;
        }

        switch (type)
        {
            case ReferenceType.Preview:
                Debug($"Found visor preview for {id}");
                lock (visor.DecodeLock)
                {
                    if (visor.BundleSource != null && visor.PreviewData.PreviewSprite == null)
                    {
                        Debug($"Decoding preview for {id}");
                        BundleDecoder.DecodePreview(visor.PreviewData, visor.BundleSource);
                    }
                }
                handle.Complete(visor.PreviewData, true, null);
                return true;
            case ReferenceType.VisorViewData:
                Debug($"Found visor view data for {id}");
                lock (visor.DecodeLock)
                {
                    if (visor.BundleSource != null && visor.VisorViewData.IdleFrame == null)
                    {
                        Debug($"Decoding visor view data for {id}");
                        BundleDecoder.DecodeVisor(visor.VisorViewData, visor.BundleSource);
                    }
                }
                handle.Complete(visor.VisorViewData, true, null);
                return true;
            default:
                Error("Unknown visor type");
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

        if (!CustomVisors.TryGetValue(realKey, out var visor))
        {
            return false;
        }

        switch (typeName)
        {
            case ReferenceType.Preview:
                Debug($"Releasing visor preview for {realKey}");
                visor.PreviewData.Unload();
                break;
            case ReferenceType.VisorViewData:
                Debug($"Releasing visor view data for {realKey}");
                visor.VisorViewData.Unload();
                break;
            default:
                Info($"Unknown type {typeName}, ignoring release request");
                break;
        }
        return true;
    }

    private bool LoadVisor(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath);
        var metadataFile = Path.ChangeExtension(filePath, ".json");
        var metadata = new VisorMetadata
        {
            Name = name
        };
        try
        {
            if (File.Exists(metadataFile))
            {
                var metadataJson = File.ReadAllText(metadataFile);
                metadata = JsonSerializer.Deserialize<VisorMetadata>(metadataJson);
            }
            else
            {
                Warning($"No metadata file found for visor {name}, using defaults.");
            }
        }
        catch (Exception e)
        {
            Error($"Failed to load metadata for visor {name}: {e.Message}");
            return false;
        }

        var fullId = Names.Normalize(name, "nameplate");

        var visorSprite = SpriteTools.LoadSpriteFromFile(filePath);
        if (visorSprite == null)
        {
            Error($"Error loading visor sprite {name}");
            return false;
        }
        
        visorSprite.DontUnload().DontDestroy();
        var visorViewData = ScriptableObject.CreateInstance<VisorViewData>();
        visorViewData.name = metadata.Name;
        visorViewData.MatchPlayerColor = metadata.MatchPlayerColor;
        visorViewData.ClimbFrame = SpriteTools.EmptySprite;
        visorViewData.IdleFrame
            = visorViewData.LeftIdleFrame
                    = visorViewData.FloorFrame
                        = visorSprite;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = metadata.Name;
        previewData.PreviewSprite = visorSprite;

        var visorData = ScriptableObject.CreateInstance<VisorData>();
        visorData.name = metadata.Name;
        visorData.Free = true;
        visorData.ProductId = fullId;
        visorData.behindHats = metadata.BehindHats;
        visorData.PreviewCrewmateColor = metadata.MatchPlayerColor;
        visorData.ViewDataRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.VisorViewData));
        visorData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        var customVisor = new CustomVisor(fullId, visorData, visorViewData, previewData);
        CustomVisors.Add(fullId, customVisor);
        
        visorData.ViewDataRef.LoadAsset<VisorViewData>();
        visorData.PreviewData.LoadAsset<PreviewViewData>();

        return true;
    }
}