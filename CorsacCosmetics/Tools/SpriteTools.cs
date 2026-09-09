using System;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace CorsacCosmetics.Tools;

public static class SpriteTools
{
    public static void CleanUp(this Sprite sprite)
    {
        if (sprite == null) return;

        var texture = sprite.texture;
        sprite.DestroyImmediate(); 
        texture?.DestroyImmediate();
    }

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

    public static Sprite? LoadSpriteFromStream(Stream stream)
    {
        return LoadSpriteFromStream(stream, 0, (uint)stream.Length);
    }

    public static Texture2D TextureFromStream(Stream stream, long start, uint length)
    {
        stream.Seek(start, SeekOrigin.Begin);

        var il2CppBytes = new Il2CppStructArray<byte>(length);
        il2CppBytes.CopyFromStream(stream, (int)length);

        var texture = new Texture2D(2, 2);
        // markNonReadable: true -- we never read pixels back from managed code
        texture.LoadImage(il2CppBytes, markNonReadable: true);
        return texture;
    }

    public static Sprite? LoadSpriteFromStream(Stream stream, long start, uint length)
    {
        if (length == 0)
        {
            return null;
        }

        try
        {
            var texture = TextureFromStream(stream, start, length);

            var sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            return sprite;
        }
        catch (Exception e)
        {
            Error($"Error while loading sprite from stream:\n{e}");
            return null;
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
            var sprite = LoadSpriteFromStream(fs);
            if (sprite == null)
            {
                Error("Failed to load sprite from file.");
                return null;
            }

            sprite.name = Path.GetFileName(filePath);
            return sprite;
        }
        catch (Exception e)
        {
            Error($"Error while loading sprite from {filePath}:\n{e.ToString()}");
            return null;
        }
    }
}