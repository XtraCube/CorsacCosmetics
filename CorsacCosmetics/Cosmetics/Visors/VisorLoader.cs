using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CorsacCosmetics.Tools;
using Il2CppInterop.Runtime;
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

    public override bool LocateCosmetic(string id, Il2CppSystem.Type type)
    {
        if (!CustomVisors.ContainsKey(id))
        {
            return false;
        }

        return type == Il2CppType.Of<VisorViewData>() || type == Il2CppType.Of<PreviewViewData>();
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, Il2CppSystem.Type type)
    {
        if (!CustomVisors.TryGetValue(id, out var visor))
        {
            return false;
        }

        if (type == Il2CppType.Of<PreviewViewData>())
        {
            Debug($"Found visor preview for {id}");
            PreviewViewData previewData;
            lock (visor.DecodeLock)
            {
                Debug($"Decoding preview for {id}");
                previewData = visor.PreviewViewDataFactory();
            }
            handle.Complete(previewData, true, null);
            return true;
        }

        if (type == Il2CppType.Of<VisorViewData>())
        {
            Debug($"Found visor view data for {id}");
            VisorViewData viewData;
            lock (visor.DecodeLock)
            {
                Debug($"Decoding visor for {id}");
                viewData = visor.VisorViewDataFactory();
            }

            handle.Complete(viewData, true, null);
            return true;
        }

        Warning($"Could not locate visor data for id {id} and type {type.FullName}");
        return false;
    }

    public override bool ReleaseCosmetic(IResourceLocation location, Il2CppSystem.Object obj)
    {
        var key = location.InternalId;
        var type = location.ResourceType;

        if (!CustomVisors.ContainsKey(key))
        {
            return false;
        }

        if (type == Il2CppType.Of<PreviewViewData>())
        {
            Debug($"Releasing visor preview for {key}");
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

        if (type == Il2CppType.Of<VisorViewData>())
        {
            Debug($"Releasing visor view data for {key}");
            if (obj.TryCast<VisorViewData>() is { } visorData)
            {
                visorData.Unload();
            }
            else
            {
                Error($"Object {obj.GetIl2CppType().NameOrDefault} is not a VisorViewData, cannot release");
            }

            return true;
        }

        Warning($"Could not release visor data for id {key} and type {type.FullName}");
        return false;
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

        var fullId = Names.Normalize(name, "visor");
        var visorData = new VisorDataBuilder()
            .SetId(fullId)
            .SetName(metadata.Name)
            .SetMatchPlayerColor(metadata.MatchPlayerColor)
            .Build();

        var customVisor = new CustomVisor(fullId, visorData, CreatePreviewViewData, CreateVisorViewData);
        CustomVisors.Add(fullId, customVisor);
        return true;

        PreviewViewData CreatePreviewViewData()
        {
            return FileDecoder.DecodePreview(filePath);
        }

        VisorViewData CreateVisorViewData()
        {
            return FileDecoder.DecodeVisor(filePath, metadata.MatchPlayerColor);
        }
    }
}