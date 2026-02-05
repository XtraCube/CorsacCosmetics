using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx;
using CorsacCosmetics.Unity;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Loader;

public class HatLoader
{
    private static HatLoader? _hatLoader;
    public static HatLoader Instance => _hatLoader ??= new HatLoader();

    private string BasePath { get; } = Path.Combine(
        OperatingSystem.IsAndroid() ? Application.persistentDataPath : Paths.GameRootPath,
        "CorsacCosmetics"
    );

    private string HatsPath => Path.Combine(BasePath, "Hats");

    public Dictionary<string, CustomHat> CustomHats { get; } = [];

    private Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>? _keys;

    public Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object> GetHatKeys()
    {
        if (_keys == null)
        {
            // this doesn't actually need to be filled with anything.
            var il2CPPList = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object>();
            // pointer magic cuz il2cpp interfaces are broken
            _keys = new(il2CPPList.Pointer);
        }

        return _keys;
    }

    public void LoadHats()
    {
        if (!Directory.Exists(HatsPath))
        {
            Directory.CreateDirectory(HatsPath);
            Info($"Created hats directory at {HatsPath}");
            return;
        }

        var hatFiles = Directory.GetFiles(HatsPath, "*.png");

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

    private bool LoadHat(string filePath)
    {
        var id = Path.GetFileNameWithoutExtension(filePath);
        var metadataFile = Path.ChangeExtension(filePath, ".json");
        var metadata = new HatMetadata
        {
            Name = id
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
                Warning($"No metadata file found for hat {id}, using defaults.");
            }
        }
        catch (Exception e)
        {
            Error($"Failed to load metadata for hat {id}: {e.Message}");
            return false;
        }

        var fullId = $"corsac.{id}";

        var hatSprite = LoadHatSprite(filePath);
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
        hatData.name = metadata.Name;
        hatData.StoreName = id;
        hatData.Free = true;
        hatData.ProductId = fullId;
        hatData.BlocksVisors = metadata.BlocksVisors;
        hatData.NoBounce = metadata.NoBounce;
        hatData.InFront = metadata.InFront;
        hatData.ViewDataRef = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.HatViewData));
        hatData.PreviewData = new AssetReference(HatLocator.GetGuid(fullId, ReferenceType.Preview));

        var customHat = new CustomHat(fullId, metadata, hatSprite, hatViewData, previewData, hatData);
        CustomHats.Add(fullId, customHat);

        hatData.ViewDataRef.LoadAsset<HatViewData>();
        hatData.PreviewData.LoadAsset<PreviewViewData>();
        return true;
    }

    private static Sprite LoadHatSprite(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var hatData = new Il2CppStructArray<byte>(fs.Length);
        for (var i = 0; i < fs.Length; i++)
        {
            hatData[i] = (byte)fs.ReadByte();
        }

        var texture = new Texture2D(2, 2);
        texture.LoadImage(hatData);
        var sprite =  Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
            );
        sprite.name = Path.GetFileNameWithoutExtension(filePath);
        return sprite;
    }
}