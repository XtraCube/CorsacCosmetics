using System;
using System.Threading.Tasks;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.AssetReaders;

public interface ICosmeticAssetReader
{
    Task<Sprite?> LoadSpriteAsync(string spriteKey)
    {
        throw new NotSupportedException();
    }

    Task<GameObject?> LoadGameObjectAsync(string objectKey)
    {
        throw new NotSupportedException();
    }
}