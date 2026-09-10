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
                Error($"Failed to load hat {id} with exception:\n{e}");
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

        switch (type)
        {
            case ReferenceType.Preview:
                Debug($"Found hat preview for {id}");
                PreviewViewData previewData;
                lock (hat.DecodeLock)
                {
                    Debug($"Decoding preview for {id}");
                    previewData = hat.PreviewViewDataFactory();
                }
                handle.Complete(previewData, true, null);
                return true;
            case ReferenceType.HatViewData:
                Debug($"Found hat view data for {id}");
                HatViewData hatViewData;
                lock (hat.DecodeLock)
                {
                    Debug($"Decoding hat for {id}");
                    hatViewData = hat.HatViewDataFactory();
                }
                handle.Complete(hatViewData, true, null);
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

        if (!CustomHats.ContainsKey(realKey))
        {
            return false;
        }

        switch (typeName)
        {
            case ReferenceType.Preview:
                Debug($"Releasing hat preview for {realKey}");
                if (obj.TryCast<PreviewViewData>() is { } previewData)
                {
                    previewData.Unload();
                }
                else
                {
                    Error($"Object {obj.GetIl2CppType().NameOrDefault} is not a PreviewViewData, cannot release");
                }
                break;
            case ReferenceType.HatViewData:
                Debug($"Releasing hat view data for {realKey}");
                if (obj.TryCast<HatViewData>() is { } hatViewData)
                {
                    hatViewData.Unload();
                }
                else
                {
                    Error($"Object {obj.GetIl2CppType().NameOrDefault} is not a HatViewData, cannot release");
                }
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

        var customHat = new CustomHat(fullId, hatData, CreatePreviewViewData, CreateHatViewData);
        CustomHats.Add(fullId, customHat);
        return true;

        PreviewViewData CreatePreviewViewData()
        {
            return FileDecoder.DecodePreview(filePath);
        }

        HatViewData CreateHatViewData()
        {
            return FileDecoder.DecodeHat(filePath, metadata.MatchPlayerColor);
        }
    }
}