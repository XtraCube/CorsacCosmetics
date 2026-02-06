using System;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace CorsacCosmetics;

public static class SpriteTools
{
    public static Sprite EmptySprite
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            var emptyTexture = new Texture2D(1, 1);
            emptyTexture.SetPixel(0, 0, Color.clear);
            emptyTexture.Apply();

            field = Sprite.Create(
                emptyTexture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                100f
            );
            return field;
        }
    }

    public static Sprite? LoadSpriteFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var hatData = new Il2CppStructArray<byte>(fs.Length);
            for (var i = 0; i < fs.Length; i++)
            {
                hatData[i] = (byte)fs.ReadByte();
            }

            var texture = new Texture2D(2, 2);
            texture.LoadImage(hatData);
            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            sprite.name = Path.GetFileNameWithoutExtension(filePath);
            return sprite;
        }
        catch (Exception e)
        {
            Error($"Error while loading sprite from {filePath}:\n{e.ToString()}");
            return null;
        }
    }
}