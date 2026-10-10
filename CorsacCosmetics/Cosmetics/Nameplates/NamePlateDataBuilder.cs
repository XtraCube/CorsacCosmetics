using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public sealed class NamePlateDataBuilder(string productId)
{
    public string Name { get; private set; } = "";

    public string ProductId { get; private set; } = productId;

    public string AssetId { get; private set; } = "";

    public NamePlateDataBuilder SetFromMetadata(NamePlateMetadata metadata)
    {
        Name = metadata.Name;
        return this;
    }

    public NamePlateDataBuilder SetName(string name)
    {
        Name = name;
        return this;
    }

    public NamePlateDataBuilder SetId(string id)
    {
        ProductId = id;
        return this;
    }

    public NamePlateDataBuilder SetAssetId(string id)
    {
        AssetId = id;
        return this;
    }

    public NamePlateData Build()
    {
        var namePlateData = ScriptableObject.CreateInstance<NamePlateData>();
        namePlateData.name = Name;
        namePlateData.Free = true;
        namePlateData.ProductId = ProductId;
        namePlateData.ViewDataRef = new AssetReference(AssetId, Il2CppType.Of<NamePlateViewData>());
        return namePlateData;
    }
}