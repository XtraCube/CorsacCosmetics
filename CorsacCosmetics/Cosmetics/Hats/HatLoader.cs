using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Tools;
using Il2CppInterop.Runtime;
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

    public override bool LocateCosmetic(string id, Il2CppSystem.Type type)
    {
        if (!CustomHats.ContainsKey(id))
        {
            return false;
        }

        return type == Il2CppType.Of<HatViewData>() || type == Il2CppType.Of<PreviewViewData>();
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, Il2CppSystem.Type type)
    {
        if (!CustomHats.TryGetValue(id, out var hat))
        {
            return false;
        }

        Debug($"Processing data for {id} and type {type.FullName}");
        if (type == Il2CppType.Of<HatViewData>())
        {
            Debug($"Found hat view data for {id}");
            HatViewData viewData;
            lock (hat.DecodeLock)
            {
                Debug($"Decoding hat for {id}");
                viewData = hat.HatViewDataFactory();
            }

            handle.Complete(viewData, true, null);
            return true;
        }

        if (type == Il2CppType.Of<PreviewViewData>())
        {
            Debug($"Found hat preview for {id}");
            PreviewViewData previewData;
            lock (hat.DecodeLock)
            {
                Debug($"Decoding preview for {id}");
                previewData = hat.PreviewViewDataFactory();
            }
            handle.Complete(previewData, true, null);
            return true;
        }

        Warning($"Could not locate hat data for id {id} and type {type.FullName}");
        return false;
    }

    public override bool ReleaseCosmetic(IResourceLocation location, Il2CppSystem.Object obj)
    {
        var key = location.InternalId;
        var type = location.ResourceType;

        if (!CustomHats.ContainsKey(key))
        {
            return false;
        }

        if (type == Il2CppType.Of<PreviewViewData>())
        {
            Debug($"Releasing hat preview for {key}");
            if (obj.TryCast<PreviewViewData>() is { } previewData)
            {
                previewData.Unload();
            }
            else
            {
                Error($"Object {obj.GetIl2CppType().NameOrDefault} is not a PreviewViewData, cannot release");
            }

            return true;
        }

        if (type == Il2CppType.Of<HatViewData>())
        {
            Debug($"Releasing hat view data for {key}");
            if (obj.TryCast<HatViewData>() is { } viewData)
            {
                viewData.Unload();
            }
            else
            {
                Error($"Object {obj.GetIl2CppType().NameOrDefault} is not a HatViewData, cannot release");
            }

            return true;
        }

        Warning($"Could not release hat data for id {key} and type {type.FullName}");
        return false;
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
        var hatData = new HatDataBuilder()
            .SetName(metadata.Name)
            .SetId(fullId)
            .SetMatchPlayerColor(metadata.MatchPlayerColor)
            .SetBlocksVisors(metadata.BlocksVisors)
            .SetInFront(metadata.InFront)
            .SetNoBounce(metadata.NoBounce)
            .Build();

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