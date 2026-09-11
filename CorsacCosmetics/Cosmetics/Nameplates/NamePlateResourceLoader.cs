using System;
using System.Threading.Tasks;
using CorsacCosmetics.Tools;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public class NamePlateResourceLoader : BaseCosmeticResourceLoader<NamePlateViewData>
{
    public override CosmeticType CosmeticType => CosmeticType.NamePlate;

    protected override async Task<NamePlateViewData> CreateViewDataAsync(CosmeticDescriptor descriptor)
    {
        if (descriptor.Type != CosmeticType) 
            throw new ArgumentException($"Descriptor is not of type {CosmeticType}");
        
        var namePlateViewData = ScriptableObject.CreateInstance<NamePlateViewData>();
        namePlateViewData.name = descriptor.DisplayName;
        namePlateViewData.Image = await descriptor.AssetReader.LoadSpriteAsync("");
        return namePlateViewData;
    }

    protected override void ReleaseViewData(NamePlateViewData viewData)
    {
        viewData.Release();
    }
}