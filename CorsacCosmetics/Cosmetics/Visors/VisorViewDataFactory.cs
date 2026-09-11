using System;
using System.Threading.Tasks;
using CorsacCosmetics.Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CorsacCosmetics.Cosmetics.Visors;

public class VisorViewDataFactory : ICosmeticViewDataFactory
{
    public CosmeticType SupportedType => CosmeticType.Visor;

    public async Task<Object> CreateViewData(CosmeticDescriptor descriptor)
    {
        if (descriptor.Type != SupportedType) throw new ArgumentException($"Descriptor is not of type {SupportedType}");

        var metadata = (VisorMetadata)descriptor.Metadata;
        var visorViewData = ScriptableObject.CreateInstance<VisorViewData>();
        visorViewData.IdleFrame = await descriptor.AssetReader.LoadSpriteAsync("");
        visorViewData.ClimbFrame = await descriptor.AssetReader.LoadSpriteAsync("climb");
        visorViewData.FloorFrame = await descriptor.AssetReader.LoadSpriteAsync("floor");
        visorViewData.LeftIdleFrame = await descriptor.AssetReader.LoadSpriteAsync("left");
        visorViewData.MatchPlayerColor = metadata.MatchPlayerColor;

        return visorViewData;
    }

    public void ReleaseViewData(Object viewData)
    {
        if (viewData.TryCast<VisorViewData>() is { } visorViewData)
        {
            visorViewData.Release();
        }
    }
}