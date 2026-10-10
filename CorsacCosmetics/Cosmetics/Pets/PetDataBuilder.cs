using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Cosmetics.Pets;

public sealed class PetDataBuilder(string productId)
{
    public string Name { get; private set; } = "";

    public string ProductId { get; private set; } = productId;

    public string AssetId { get; private set; } = "";

    public bool MatchPlayerColor { get; private set; }

    public PetDataBuilder SetFromMetadata(PetMetadata metadata)
    {
        Name = metadata.Name;
        MatchPlayerColor = metadata.MatchPlayerColor;
        return this;
    }

    public PetDataBuilder SetName(string name)
    {
        Name = name;
        return this;
    }

    public PetDataBuilder SetId(string id)
    {
        ProductId = id;
        return this;
    }

    public PetDataBuilder SetAssetId(string id)
    {
        AssetId = id;
        return this;
    }

    public PetDataBuilder SetMatchPlayerColor(bool matchPlayerColor)
    {
        MatchPlayerColor = matchPlayerColor;
        return this;
    }

    public PetData Build()
    {
        var petData = ScriptableObject.CreateInstance<PetData>();
        petData.name = Name;
        petData.StoreName = StringNames.PetLabel;
        petData.Free = true;
        petData.ProductId = ProductId;
        petData.PreviewCrewmateColor = MatchPlayerColor;
        petData.PetPrefabRef = new AssetReference(AssetId, Il2CppType.Of<PetBehaviour>());
        petData.PreviewData = new AssetReference(AssetId, Il2CppType.Of<PreviewViewData>());
        return petData;
    }
}