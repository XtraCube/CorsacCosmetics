using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

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
            var il2CPPList = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object>();
            foreach (var hatId in CustomHats.Keys)
            {
                il2CPPList.Add(new Il2CppSystem.String(hatId.ToCharArray()));
            }

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
        var metadata = new  HatMetadata();
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
        var hatViewData = ScriptableObject.CreateInstance<HatViewData>();
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

        var customHat = new CustomHat(fullId, metadata, hatSprite, hatViewData);
        CustomHats.Add(fullId, customHat);
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
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}