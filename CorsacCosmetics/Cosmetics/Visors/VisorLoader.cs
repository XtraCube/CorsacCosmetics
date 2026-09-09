using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Cosmetics.Bundle;
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
                Error($"Failed to load visor {id} with exception:\n{e}");
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
                PreviewViewData previewData;
                lock (visor.DecodeLock)
                {
                    if (visor.BundleSource != null)
                    {
                        Debug($"Decoding preview for {id}");
                        previewData = BundleDecoder.DecodePreview(visor.BundleSource);
                    }
                    else if (visor.FileSource != null)
                    {
                        Debug($"Decoding preview for {id}");
                        previewData = FileDecoder.DecodePreview(visor.FileSource);
                    }
                    else
                    {
                        Error($"No source for preview for {id}");
                        return false;
                    }
                }
                handle.Complete(previewData, true, null);
                return true;
            case ReferenceType.VisorViewData:
                Debug($"Found visor view data for {id}"); 
                VisorViewData viewData;
                lock (visor.DecodeLock)
                {
                    if (visor.BundleSource != null)
                    {
                        Debug($"Decoding visor data for {id}");
                        viewData = BundleDecoder.DecodeVisor(visor.BundleSource);
                    }
                    else if (visor.FileSource != null)
                    {
                        Debug($"Decoding preview for {id}");
                        viewData = FileDecoder.DecodeVisor(visor.FileSource);
                    }
                    else
                    {
                        Error($"No source for preview for {id}");
                        return false;
                    }
                }
                handle.Complete(viewData, true, null);
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

        if (!CustomVisors.ContainsKey(realKey))
        {
            return false;
        }

        switch (typeName)
        {
            case ReferenceType.Preview:
                Debug($"Releasing visor preview for {realKey}");
                if (obj.TryCast<PreviewViewData>() is { } previewData)
                {
                    previewData.Unload();
                }
                else
                {
                    Error($"Object {obj} is not a PreviewViewData, cannot release");
                }
                break;
            case ReferenceType.VisorViewData:
                Debug($"Releasing visor view data for {realKey}");
                if (obj.TryCast<VisorViewData>() is { } visorData)
                {
                    visorData.Unload();
                }
                else
                {
                    Error($"Object {obj} is not a VisorViewData, cannot release");
                }
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
        var visorData = ScriptableObject.CreateInstance<VisorData>();
        visorData.name = metadata.Name;
        visorData.Free = true;
        visorData.ProductId = fullId;
        visorData.behindHats = metadata.BehindHats;
        visorData.PreviewCrewmateColor = metadata.MatchPlayerColor;
        visorData.ViewDataRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.VisorViewData));
        visorData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        var customVisor = new CustomVisor(fullId, visorData, fileSource: filePath);
        CustomVisors.Add(fullId, customVisor);
        return true;
    }
}