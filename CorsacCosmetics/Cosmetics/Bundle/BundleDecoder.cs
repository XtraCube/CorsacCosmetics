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
    private static readonly Dictionary<string, FileStream> Streams = new();
    private static readonly object Lock = new();

    public static Sprite? DecodeSprite(BundleSource source, string slot)
    {
        if (!source.TryGetSprite(slot, out var data) || !data.HasData)
        {
            return null;
        }

        lock (Lock)
        {
            if (!Streams.TryGetValue(source.BundlePath, out var stream))
            {
                stream = new FileStream(source.BundlePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.RandomAccess);
                Streams[source.BundlePath] = stream;
            }
            return SpriteTools.LoadSpriteFromStream(stream, source.DataStart + data.Offset, data.Size);
        }
    }

    public static void CloseAll()
    {
        lock (Lock)
        {
            foreach (var stream in Streams.Values)
            {
                stream.Dispose();
            }
            Streams.Clear();
        }
    }
}
