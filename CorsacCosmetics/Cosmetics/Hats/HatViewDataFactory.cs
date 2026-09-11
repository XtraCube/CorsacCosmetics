using System;
using System.Threading.Tasks;
using CorsacCosmetics.Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CorsacCosmetics.Cosmetics.Hats;

public class HatViewDataFactory : ICosmeticViewDataFactory
{
    public CosmeticType SupportedType => CosmeticType.Hat;

    public async Task<Object> CreateViewData(CosmeticDescriptor descriptor)
    {
        if (descriptor.Type != SupportedType) throw new ArgumentException($"Descriptor is not of type {SupportedType}");

        var metadata = (HatMetadata)descriptor.Metadata;
        var hatViewData = ScriptableObject.CreateInstance<HatViewData>();
        hatViewData.MatchPlayerColor = metadata.MatchPlayerColor;

        hatViewData.MainImage = await descriptor.AssetReader.LoadSpriteAsync("");
        hatViewData.BackImage = await descriptor.AssetReader.LoadSpriteAsync("back");
        hatViewData.ClimbImage = await descriptor.AssetReader.LoadSpriteAsync("climb");
        hatViewData.FloorImage = await descriptor.AssetReader.LoadSpriteAsync("floor");
        hatViewData.LeftMainImage = await descriptor.AssetReader.LoadSpriteAsync("left");
        hatViewData.LeftBackImage = await descriptor.AssetReader.LoadSpriteAsync("leftback");
        hatViewData.LeftClimbImage = await descriptor.AssetReader.LoadSpriteAsync("leftclimb");
        hatViewData.LeftFloorImage = await descriptor.AssetReader.LoadSpriteAsync("leftfloor");

        return hatViewData;
    }

    public void ReleaseViewData(Object viewData)
    {
        if (viewData.TryCast<HatViewData>() is { } hatViewData)
        {
            hatViewData.Release();
        }
    }
}