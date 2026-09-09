using System.IO;
using CorsacCosmetics.Tools;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics;

public static class FileDecoder
{  
    public static PreviewViewData DecodePreview(string filePath)
    {
        var viewData = ScriptableObject.CreateInstance<PreviewViewData>();
        viewData.PreviewSprite = SpriteTools.LoadSpriteFromFile(filePath);
        return viewData;
    }

    public static HatViewData DecodeHat(string filePath)
    {
        var viewData = ScriptableObject.CreateInstance<HatViewData>();
        viewData.MainImage = SpriteTools.LoadSpriteFromFile(filePath);
        viewData.ClimbImage = SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".climb"));
        viewData.FloorImage = SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".floor"));
        viewData.BackImage = SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".back"));
        viewData.LeftMainImage = SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".left"));
        viewData.LeftClimbImage = SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".leftclimb"));
        viewData.LeftFloorImage = SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".leftfloor"));
        viewData.LeftBackImage = SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".leftback"));
        return viewData;
    }

    public static VisorViewData DecodeVisor(string filePath)
    {
        var viewData = ScriptableObject.CreateInstance<VisorViewData>();
        viewData.IdleFrame = SpriteTools.LoadSpriteFromFile(filePath);
        viewData.LeftIdleFrame =
            SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".leftidle"))
            ?? viewData.IdleFrame;
        viewData.ClimbFrame =
            SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".climb"))
            ?? viewData.IdleFrame;
        viewData.FloorFrame =
            SpriteTools.LoadSpriteFromFile(Path.ChangeExtension(filePath, ".floor"))
            ?? viewData.IdleFrame;
        return viewData;
    }

    public static NamePlateViewData DecodeNameplate(string filePath)
    {
        var viewData = ScriptableObject.CreateInstance<NamePlateViewData>();
        viewData.Image = SpriteTools.LoadSpriteFromFile(filePath);
        return viewData;
    }
}