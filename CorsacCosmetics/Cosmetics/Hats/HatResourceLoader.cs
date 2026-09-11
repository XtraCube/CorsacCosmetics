using System;
using System.Threading.Tasks;
using CorsacCosmetics.Tools;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.Hats;

public class HatResourceLoader : BaseCosmeticResourceLoader<HatViewData>
{
    public override CosmeticType CosmeticType => CosmeticType.Hat;

    protected override async Task<HatViewData> CreateViewDataAsync(CosmeticDescriptor descriptor)
    {
        if (descriptor.Type != CosmeticType) 
            throw new ArgumentException($"Descriptor is not of type {CosmeticType}");

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

    protected override void ReleaseViewData(HatViewData viewData)
    {
        viewData.Release();
    }
}