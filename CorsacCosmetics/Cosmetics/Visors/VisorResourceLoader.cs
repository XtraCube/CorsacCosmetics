using System;
using System.Threading.Tasks;
using CorsacCosmetics.Tools;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.Visors;

public class VisorResourceLoader : BaseCosmeticResourceLoader<VisorViewData>
{
    public override CosmeticType CosmeticType => CosmeticType.Visor;

    protected override async Task<VisorViewData> CreateViewDataAsync(CosmeticDescriptor descriptor)
    {
        if (descriptor.Type != CosmeticType) 
            throw new ArgumentException($"Descriptor is not of type {CosmeticType}");

        var metadata = (VisorMetadata)descriptor.Metadata;
        var visorViewData = ScriptableObject.CreateInstance<VisorViewData>();
        visorViewData.IdleFrame = await descriptor.AssetReader.LoadSpriteAsync("");
        visorViewData.ClimbFrame = await descriptor.AssetReader.LoadSpriteAsync("climb");
        visorViewData.FloorFrame = await descriptor.AssetReader.LoadSpriteAsync("floor");
        visorViewData.LeftIdleFrame = await descriptor.AssetReader.LoadSpriteAsync("left");
        visorViewData.MatchPlayerColor = metadata.MatchPlayerColor;

        return visorViewData;
    }

    protected override void ReleaseViewData(VisorViewData viewData)
    {           
        viewData.Release();
    }
}