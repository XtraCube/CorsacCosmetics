using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Unity;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public class NameplateLoader : BaseLoader
{
    public Dictionary<string, CustomNamePlate> CustomNamePlates { get; } = [];

    public override void InstallCosmetics(ReferenceData refData)
    {
        foreach (var (id, customNamePlate) in CustomNamePlates)
        {
            try
            {
                refData.nameplates.Add(customNamePlate.NamePlateData);
                Info($"Added nameplate {id} to HatManager");
            }
            catch (Exception e)
            {
                Error($"Failed to load nameplate {id} with exception:\n{e}");
            }
        }
    }

    public override void LoadCosmetics(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Info($"Created namePlates directory at {directory}");
            return;
        }

        var namePlateFiles = Directory.GetFiles(directory, "*.png");

        foreach (var namePlateFile in namePlateFiles)
        {
            try
            {
                if (LoadNamePlate(namePlateFile))
                {
                    Info($"Loaded nameplate from {namePlateFile}");
                }
                else
                {
                    Error($"Failed to load nameplate from {namePlateFile}");
                }
            }
            catch (Exception e)
            {
                Error($"Exception while loading nameplate from {namePlateFile}: {e.Message}");
            }
        }
    }

    public override bool LocateCosmetic(string id, string type, [NotNullWhen(true)] out Il2CppSystem.Type? il2CPPType)
    {
        il2CPPType = null;
        if (!CustomNamePlates.ContainsKey(id))
        {
            return false;
        }

        il2CPPType = type == ReferenceType.NamePlateViewData ? Il2CppType.Of<NamePlateViewData>() : null;
        return il2CPPType != null;
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, string type)
    {
        if (!CustomNamePlates.TryGetValue(id, out var nameplate))
        {
            return false;
        }

        switch (type)
        {
            case ReferenceType.Preview:
                Debug($"Found nameplate preview for {id}");
                PreviewViewData previewViewData;
                lock (nameplate.DecodeLock)
                {
                    Debug($"Decoding preview for {id}");
                    previewViewData = nameplate.PreviewViewDataFactory();
                }
                handle.Complete(previewViewData, true, null);
                return true;
            case ReferenceType.NamePlateViewData:
                Debug($"Found nameplate view data for {id}");
                NamePlateViewData viewData;
                lock (nameplate.DecodeLock)
                {
                    Debug($"Decoding nameplate for {id}");
                    viewData = nameplate.NamePlateViewDataFactory();
                }
                handle.Complete(viewData, true, null);
                return true;
            default:
                Error("Unknown nameplate type");
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

        if (!CustomNamePlates.ContainsKey(realKey))
        {
            return false;
        }

        switch (typeName)
        {
            case ReferenceType.Preview:
                Debug($"Releasing nameplate preview for {realKey}");
                if (obj.TryCast<PreviewViewData>() is { } previewData)
                {
                    previewData.Unload();
                }
                else
                {
                    Error($"Object {obj} is not a PreviewViewData, cannot release");
                }
                break;
            case ReferenceType.NamePlateViewData:
                Debug($"Releasing nameplate view data for {realKey}");
                if (obj.TryCast<NamePlateViewData>() is { } viewData)
                {
                    viewData.Unload();
                }
                else
                {
                    Error($"Object {obj} is not a NamePlateViewData, cannot release");
                }
                break;
            default:
                Info($"Unknown type {typeName}, ignoring release request");
                break;
        }
        return true;
    }

    private bool LoadNamePlate(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath);
        var metadataFile = Path.ChangeExtension(filePath, ".json");
        var metadata = new NameplateMetadata
        {
            Name = name
        };
        try
        {
            if (File.Exists(metadataFile))
            {
                var metadataJson = File.ReadAllText(metadataFile);
                metadata = JsonSerializer.Deserialize<NameplateMetadata>(metadataJson);
            }
            else
            {
                Warning($"No metadata file found for nameplate {name}, using defaults.");
            }
        }
        catch (Exception e)
        {
            Error($"Failed to load metadata for nameplate {name}: {e.Message}");
            return false;
        }

        var fullId = Names.Normalize(name, "nameplate");
        var namePlateData = ScriptableObject.CreateInstance<NamePlateData>();
        namePlateData.name = metadata.Name;
        namePlateData.Free = true;
        namePlateData.ProductId = fullId;
        namePlateData.ViewDataRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.NamePlateViewData));
        namePlateData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        var customNamePlate = new CustomNamePlate(fullId, namePlateData, CreatePreviewViewData, CreateNamePlateViewData);
        CustomNamePlates.Add(fullId, customNamePlate);
        
        namePlateData.ViewDataRef.LoadAsset<NamePlateViewData>();
        namePlateData.PreviewData.LoadAsset<PreviewViewData>();

        return true;

        PreviewViewData CreatePreviewViewData()
        {
            return FileDecoder.DecodePreview(filePath);
        }

        NamePlateViewData CreateNamePlateViewData()
        {
            return FileDecoder.DecodeNameplate(filePath);
        }
    }
}