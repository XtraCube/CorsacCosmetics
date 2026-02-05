using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using CorsacCosmetics.Unity;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Cosmetics.Visors;

public class VisorLoader : BaseLoader
{
    public Dictionary<string, CustomVisor> CustomVisors { get; } = [];

    public override string GetCosmeticId(string name)
    {
        return $"corsac.visor.{name}";
    }

    public override void InstallCosmetics(HatManager hatManager)
    {
        var visors = hatManager.allVisors.ToList();

        foreach (var (id, customVisor) in CustomVisors)
        {
            try
            {
                visors.Add(customVisor.VisorData);
                Info($"Added visor {id} to HatManager");
            }
            catch (Exception e)
            {
                Error($"Failed to load visor {id} with exception:\n{e.ToString()}");
            }
        }

        hatManager.allVisors = new Il2CppReferenceArray<VisorData>(visors.Count);
        for (var i = 0; i < visors.Count; i++)
        {
            hatManager.allVisors[i] = visors[i];
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
                handle.Complete(visor.PreviewData, true, null);
                return true;
            case ReferenceType.VisorViewData:
                Debug($"Found visor view data for {id}");
                handle.Complete(visor.VisorViewData, true, null);
                return true;
            default:
                Error("Unknown visor type");
                return false;
        }
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

        var fullId = GetCosmeticId(name);

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
        visorViewData.IdleFrame
            = visorViewData.LeftIdleFrame
                = visorViewData.ClimbFrame
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
        visorData.ViewDataRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.VisorViewData));
        visorData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        var customVisor = new CustomVisor(fullId, metadata, visorData, visorViewData, previewData);
        CustomVisors.Add(fullId, customVisor);
        
        visorData.ViewDataRef.LoadAsset<VisorViewData>();
        visorData.PreviewData.LoadAsset<PreviewViewData>();

        return true;
    }
}