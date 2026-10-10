using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Cosmetics.Visors;

public class VisorDataBuilder(string productId)
{
    public string Name { get; private set; } = "";

    public string ProductId { get; private set; } = productId;

    public string AssetId { get; private set; } = "";

    public bool BehindHats { get; private set; }

    public bool MatchPlayerColor { get; private set; }

    public VisorDataBuilder SetFromMetadata(VisorMetadata metadata)
    {
        Name = metadata.Name;
        BehindHats = metadata.BehindHats;
        MatchPlayerColor = metadata.MatchPlayerColor;
        return this;
    }

    public VisorDataBuilder SetName(string name)
    {
        Name = name;
        return this;
    }

    public VisorDataBuilder SetId(string id)
    {
        ProductId = id;
        return this;
    }

    public VisorDataBuilder SetAssetId(string id)
    {
        AssetId = id;
        return this;
    }

    public VisorDataBuilder SetBehindHats(bool behindHats)
    {
        BehindHats = behindHats;
        return this;
    }

    public VisorDataBuilder SetMatchPlayerColor(bool value)
    {
        MatchPlayerColor = value;
        return this;
    }

    public VisorData Build()
    {
        var visorData = ScriptableObject.CreateInstance<VisorData>();
        visorData.name = Name;
        visorData.Free = true;
        visorData.ProductId = ProductId;
        visorData.behindHats = BehindHats;
        visorData.PreviewCrewmateColor = MatchPlayerColor;
        visorData.ViewDataRef = new AssetReference(AssetId, Il2CppType.Of<VisorViewData>());
        visorData.PreviewData = new AssetReference(AssetId, Il2CppType.Of<PreviewViewData>());
        return visorData;
    }
}