using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Cosmetics.Hats;

public sealed class HatDataBuilder(string productId)
{
    public string Name { get; private set; } = "";

    public string ProductId { get; private set; } = productId;

    public string AssetId { get; private set; } = "";

    public bool BlocksVisors { get; private set; }

    public bool NoBounce { get; private set; } = true;

    public bool InFront { get; private set; } = true;

    public bool MatchPlayerColor { get; private set; }

    public HatDataBuilder SetFromMetadata(HatMetadata metadata)
    {
        Name = metadata.Name;
        BlocksVisors = metadata.BlocksVisors;
        NoBounce = metadata.NoBounce;
        InFront = metadata.InFront;
        MatchPlayerColor = metadata.MatchPlayerColor;
        return this;
    }

    public HatDataBuilder SetName(string name)
    {
        Name = name;
        return this;
    }

    public HatDataBuilder SetId(string id)
    {
        ProductId = id;
        return this;
    }

    public HatDataBuilder SetAssetId(string id)
    {
        AssetId = id;
        return this;
    }

    public HatDataBuilder SetBlocksVisors(bool value)
    {
        BlocksVisors = value;
        return this;
    }

    public HatDataBuilder SetNoBounce(bool value)
    {
        NoBounce = value;
        return this;
    }

    public HatDataBuilder SetInFront(bool value)
    {
        InFront = value;
        return this;
    }

    public HatDataBuilder SetMatchPlayerColor(bool value)
    {
        MatchPlayerColor = value;
        return this;
    }

    public HatData Build()
    {
        var hatData = ScriptableObject.CreateInstance<HatData>();
        hatData.name = Name;
        hatData.StoreName = Name;
        hatData.Free = true;
        hatData.ProductId = ProductId;
        hatData.BlocksVisors = BlocksVisors;
        hatData.NoBounce = NoBounce;
        hatData.InFront = InFront;
        hatData.PreviewCrewmateColor = MatchPlayerColor;
        hatData.ViewDataRef = new AssetReference(AssetId, Il2CppType.Of<HatViewData>());
        hatData.PreviewData = new AssetReference(AssetId, Il2CppType.Of<PreviewViewData>());
        return hatData;
    }
}
