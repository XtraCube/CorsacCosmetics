using System.Collections.Generic;
using System.IO;
using CorsacCosmetics.Tools;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.Bundle;

/// <summary>
/// Decodes cosmetic sprites lazily from kept-open bundle file streams.
/// One FileStream per bundle file is cached for the lifetime of the mod so that
/// repeated decodes (e.g. paging through a tab, or joining a game where many players
/// wear cosmetics from the same bundle) only pay the seek + read cost, not the
/// open/close cost. All access is serialized on a single lock for thread safety.
/// </summary>
public static class BundleDecoder
{
    private static readonly Dictionary<string, FileStream> _streams = new();
    private static readonly object _lock = new();

    public static Sprite? DecodeSprite(BundleSource source, string slot)
    {
        if (!source.TryGetSprite(slot, out var data) || !data.HasData)
        {
            return null;
        }

        lock (_lock)
        {
            if (!_streams.TryGetValue(source.BundlePath, out var stream))
            {
                stream = new FileStream(source.BundlePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.RandomAccess);
                _streams[source.BundlePath] = stream;
            }
            return SpriteTools.LoadSpriteFromStream(stream, source.DataStart + data.Offset, data.Size);
        }
    }

    public static PreviewViewData DecodePreview(BundleSource source)
    {
        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        var previewSprite = DecodeSprite(source, "PreviewSprite");
        previewData.PreviewSprite = previewSprite;
        return previewData;
    }

    public static void DecodePreview(PreviewViewData previewData, BundleSource source)
    {
        var previewSprite = DecodeSprite(source, "PreviewSprite");
        previewData.PreviewSprite = previewSprite;
    }

    public static HatViewData DecodeHat(BundleSource source)
    {
        var viewData = ScriptableObject.CreateInstance<HatViewData>();
        viewData.MainImage = DecodeSprite(source, "MainSprite");
        viewData.BackImage = DecodeSprite(source, "BackSprite");
        viewData.ClimbImage = DecodeSprite(source, "ClimbSprite");
        viewData.FloorImage = DecodeSprite(source, "FloorSprite");
        viewData.LeftMainImage = DecodeSprite(source, "LeftMainSprite");
        viewData.LeftBackImage = DecodeSprite(source, "LeftBackSprite");
        viewData.LeftClimbImage = DecodeSprite(source, "LeftClimbSprite");
        viewData.LeftFloorImage = DecodeSprite(source, "LeftFloorSprite");
        return viewData;
    }

    public static void DecodeVisor(VisorViewData viewData, BundleSource source)
    {
        viewData.IdleFrame = DecodeSprite(source, "IdleSprite");
        viewData.LeftIdleFrame = DecodeSprite(source, "LeftIdleSprite");
        viewData.FloorFrame = DecodeSprite(source, "FloorSprite");
        viewData.ClimbFrame = DecodeSprite(source, "ClimbSprite");
    }

    public static void DecodeNameplate(NamePlateViewData viewData, BundleSource source)
    {
        viewData.Image = DecodeSprite(source, "NameplateSprite");
    }

    public static void CloseAll()
    {
        lock (_lock)
        {
            foreach (var stream in _streams.Values)
            {
                stream.Dispose();
            }
            _streams.Clear();
        }
    }
}
