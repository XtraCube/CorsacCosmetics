using System;
using System.Threading.Tasks;
using CorsacCosmetics.Tools;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public class NamePlateViewDataFactory : ICosmeticViewDataFactory
{
    public CosmeticType SupportedType => CosmeticType.NamePlate;

    public async Task<Object> CreateViewData(CosmeticDescriptor descriptor)
    {
        if (descriptor.Type != SupportedType) throw new ArgumentException($"Descriptor is not of type {SupportedType}");

        var namePlateViewData = ScriptableObject.CreateInstance<NamePlateViewData>();
        namePlateViewData.Image = await descriptor.AssetReader.LoadSpriteAsync("");
        return namePlateViewData;
    }

    public void ReleaseViewData(Object viewData)
    {
        if (viewData.TryCast<NamePlateViewData>() is { } namePlateViewData)
        {
            namePlateViewData.Release();
        }
    }
}