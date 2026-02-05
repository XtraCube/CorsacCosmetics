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

namespace CorsacCosmetics.Cosmetics.Hats;

public class HatLoader : BaseLoader
{
    public Dictionary<string, CustomHat> CustomHats { get; } = [];

    public override string GetCosmeticId(string name)
    {
        return $"corsac.hat.{name}";
    }

    public override void InstallCosmetics(HatManager hatManager)
    {
        var hats = hatManager.allHats.ToList();

        foreach (var (id, customHat) in CustomHats)
        {
            try
            {
                hats.Add(customHat.HatData);
                Info($"Added {id} to HatManager");
            }
            catch (Exception e)
            {
                Error($"Failed to load hat {id} with exception:\n{e.ToString()}");
            }
        }

        hatManager.allHats = new Il2CppReferenceArray<HatData>(hats.Count);
        for (var i = 0; i < hats.Count; i++)
        {
            hatManager.allHats[i] = hats[i];
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
            case ReferenceType.Sprite:
                Debug($"Found hat sprite for {id}");
                handle.Complete(hat.HatSprite, true, null);
                return true;
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

        var fullId = GetCosmeticId(name);

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
        hatViewData.BackImage
            = hatViewData.ClimbImage
                = hatViewData.FloorImage
                    = hatViewData.MainImage
                        = hatViewData.LeftBackImage
                            = hatViewData.LeftClimbImage
                                = hatViewData.LeftFloorImage
                                    = hatViewData.LeftMainImage
                                        = hatSprite;

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
        hatData.ViewDataRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.HatViewData));
        hatData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        var customHat = new CustomHat(fullId, metadata, hatSprite, hatData, hatViewData, previewData);
        CustomHats.Add(fullId, customHat);

        hatData.ViewDataRef.LoadAsset<HatViewData>();
        hatData.PreviewData.LoadAsset<PreviewViewData>();
        return true;
    }
}