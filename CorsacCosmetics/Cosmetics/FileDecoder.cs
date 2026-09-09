using System.IO;
using UnityEngine;

public static class FileDecoder
{
    public static PreviewViewData DecodePreview(string filePath)
    {
        var viewData = ScriptableObject.CreateInstance<PreviewViewData>();
        var bytes = File.ReadAllBytes(filePath);
        var texture = new Texture2D(2, 2);
        texture.LoadImage(bytes, markNonReadable: true);
        viewData.PreviewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return viewData;
    }

    public static HatViewData DecodeHat(string filePath)
    {
        var viewData = ScriptableObject.CreateInstance<HatViewData>();
        var bytes = File.ReadAllBytes(filePath);
        var texture = new Texture2D(2, 2);
        texture.LoadImage(bytes, markNonReadable: true);
        viewData.MainImage = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return viewData;
    }
}