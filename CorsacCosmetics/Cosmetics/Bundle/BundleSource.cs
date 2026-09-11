using System.Collections.Generic;

namespace CorsacCosmetics.Cosmetics.Bundle;

/// <summary>
/// Per-cosmetic decode source for lazy bundle loading. Instead of decoding all sprites
/// up-front in LoadBundle, we store the bundle file path + data-section start offset and
/// the per-slot SpriteData (offset/size). Sprites are decoded on first access via
/// BundleDecoder, which keeps the bundle file open for the lifetime of the mod.
/// </summary>
public class BundleSource(string bundlePath, long dataStart)
{
    public string BundlePath { get; } = bundlePath;
    public long DataStart { get; } = dataStart;

    private readonly Dictionary<string, SpriteData> _sprites = new();

    public BundleSource AddSprite(string slot, SpriteData data)
    {
        _sprites[slot] = data;
        return this;
    }

    public bool TryGetSprite(string slot, out SpriteData data)
    {
        return _sprites.TryGetValue(slot, out data);
    }
}
