using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
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

    public override bool LocateCosmetic(string id, Il2CppSystem.Type type)
    {
        if (!CustomNamePlates.ContainsKey(id))
        {
            return false;
        }

        return type == Il2CppType.Of<NamePlateViewData>();
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, Il2CppSystem.Type type)
    {
        if (!CustomNamePlates.TryGetValue(id, out var namePlate))
        {
            return false;
        }

        if (type == Il2CppType.Of<NamePlateViewData>())
        {
            Debug($"Found nameplate view data for {id}");
            NamePlateViewData viewData;
            lock (namePlate.DecodeLock)
            {
                Debug($"Decoding nameplate for {id}");
                viewData = namePlate.NamePlateViewDataFactory();
            }

            handle.Complete(viewData, true, null);
            return true;
        }

        Warning($"Could not locate nameplate data for id {id} and type {type.FullName}");
        return false;
    }

    public override bool ReleaseCosmetic(IResourceLocation location, Il2CppSystem.Object obj)
    {
        var key = location.InternalId;
        var type = location.ResourceType;

        if (!CustomNamePlates.ContainsKey(key))
        {
            return false;
        }

        if (type == Il2CppType.Of<NamePlateViewData>())
        {
            Debug($"Releasing nameplate view data for {key}");
            if (obj.TryCast<NamePlateViewData>() is { } viewData)
            {
                viewData.Unload();
            }
            else
            {
                Error($"Object {obj.GetIl2CppType().NameOrDefault} is not a NamePlateViewData, cannot release");
            }

            return true;
        }

        Warning($"Could not release nameplate data for id {key} and type {type.FullName}");
        return false;
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
        var namePlateData = new NamePlateDataBuilder()
            .SetName(metadata.Name)
            .SetId(fullId)
            .Build();

        var customNamePlate = new CustomNamePlate(fullId, namePlateData, CreateNamePlateViewData);
        CustomNamePlates.Add(fullId, customNamePlate);
        return true;

        NamePlateViewData CreateNamePlateViewData()
        {
            return FileDecoder.DecodeNameplate(filePath);
        }
    }
}