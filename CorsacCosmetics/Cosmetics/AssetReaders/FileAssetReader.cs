using System.IO;
using System.Threading.Tasks;
using CorsacCosmetics.Tools;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.AssetReaders;

public class FileAssetReader(string filePath) : ICosmeticAssetReader
{
    public Task<Sprite?> LoadSpriteAsync(string spriteKey)
    {
        var path = spriteKey.Length == 0 ? filePath
            : Path.ChangeExtension(filePath, spriteKey);
        var sprite = SpriteTools.LoadSpriteFromFile(path);
        return Task.FromResult(sprite);
    }
}