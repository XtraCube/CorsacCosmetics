using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Cosmetics.Visors;

public class VisorDataBuilder
{
    public string Name { get; private set; } = "";

    public string Id { get; private set; } = "";

    public bool BehindHats { get; private set; }

    public bool MatchPlayerColor { get; private set; }

    public VisorDataBuilder SetName(string name)
    {
        Name = name;
        return this;
    }

    public VisorDataBuilder SetId(string id)
    {
        Id = id;
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
        visorData.ProductId = Id;
        visorData.behindHats = BehindHats;
        visorData.PreviewCrewmateColor = MatchPlayerColor;
        visorData.ViewDataRef = new AssetReference(Id, Il2CppType.Of<VisorViewData>());
        visorData.PreviewData = new AssetReference(Id, Il2CppType.Of<PreviewViewData>());
        return visorData;
    }
}