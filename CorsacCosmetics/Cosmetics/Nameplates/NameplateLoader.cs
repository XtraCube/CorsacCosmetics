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
                Error($"Failed to load nameplate {id} with exception:\n{e.ToString()}");
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
                lock (nameplate.DecodeLock)
                {
                    if (nameplate.BundleSource != null && nameplate.PreviewData.PreviewSprite == null)
                    {
                        Debug($"Decoding preview for {id}");
                        BundleDecoder.DecodePreview(nameplate.PreviewData, nameplate.BundleSource);
                    }
                }
                handle.Complete(nameplate.PreviewData, true, null);
                return true;
            case ReferenceType.NamePlateViewData:
                Debug($"Found nameplate view data for {id}");
                lock (nameplate.DecodeLock)
                {
                    if (nameplate.BundleSource != null && nameplate.NamePlateViewData.Image == null)
                    {
                        Debug($"Decoding nameplate view data for {id}");
                        BundleDecoder.DecodeNameplate(nameplate.NamePlateViewData, nameplate.BundleSource);
                    }
                }
                handle.Complete(nameplate.NamePlateViewData, true, null);
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

        if (!CustomNamePlates.TryGetValue(realKey, out var nameplate))
        {
            return false;
        }

        switch (typeName)
        {
            case ReferenceType.Preview:
                Debug($"Releasing nameplate preview for {realKey}");
                nameplate.PreviewData.Unload();
                break;
            case ReferenceType.NamePlateViewData:
                Debug($"Releasing nameplate view data for {realKey}");
                nameplate.NamePlateViewData.Unload();
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

        var namePlateSprite = SpriteTools.LoadSpriteFromFile(filePath);
        if (namePlateSprite == null)
        {
            Error($"Error loading nameplate sprite {name}");
            return false;
        }
        
        namePlateSprite.DontUnload().DontDestroy();
        var namePlateViewData = ScriptableObject.CreateInstance<NamePlateViewData>();
        namePlateViewData.name = metadata.Name;
        namePlateViewData.Image = namePlateSprite;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name = metadata.Name;
        previewData.PreviewSprite = namePlateSprite;

        var namePlateData = ScriptableObject.CreateInstance<NamePlateData>();
        namePlateData.name = metadata.Name;
        namePlateData.Free = true;
        namePlateData.ProductId = fullId;
        namePlateData.ViewDataRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.NamePlateViewData));
        namePlateData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        var customNamePlate = new CustomNamePlate(fullId, namePlateData, namePlateViewData, previewData);
        CustomNamePlates.Add(fullId, customNamePlate);
        
        namePlateData.ViewDataRef.LoadAsset<NamePlateViewData>();
        namePlateData.PreviewData.LoadAsset<PreviewViewData>();

        return true;
    }
}