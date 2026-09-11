using System.Threading.Tasks;
using CorsacCosmetics.Cosmetics.Bundle;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.AssetReaders;

public class BundleAssetReader(BundleSource source) : ICosmeticAssetReader
{
    public Task<Sprite?> LoadSpriteAsync(string spriteKey)
    {
        return Task.FromResult(BundleDecoder.DecodeSprite(source, spriteKey));
    }
}