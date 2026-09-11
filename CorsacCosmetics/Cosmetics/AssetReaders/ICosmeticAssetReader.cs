using System.Threading.Tasks;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.AssetReaders;

public interface ICosmeticAssetReader
{
    Task<Sprite?> LoadSpriteAsync(string spriteKey);
}