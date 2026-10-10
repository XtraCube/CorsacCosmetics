using System.Threading.Tasks;
using CorsacCosmetics.Cosmetics.Bundle;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.AssetReaders;

/// <summary>
/// Loads an asset from a Corsac Cosmetics Bundle.
/// </summary>
/// <param name="source">The BundleSource object representing the Corsac Cosmetics Bundle.</param>
public class BundleAssetReader(BundleSource source) : ICosmeticAssetReader
{
    public Task<Sprite?> LoadSpriteAsync(string spriteKey)
    {
        return Task.FromResult(BundleDecoder.DecodeSprite(source, spriteKey));
    }
}