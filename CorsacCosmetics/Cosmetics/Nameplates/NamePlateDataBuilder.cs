using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public sealed class NamePlateDataBuilder
{
    public string Name { get; private set; } = "";

    public string Id { get; private set; } = "";

    public NamePlateDataBuilder SetName(string name)
    {
        Name = name;
        return this;
    }

    public NamePlateDataBuilder SetId(string id)
    {
        Id = id;
        return this;
    }

    public NamePlateData Build()
    {
        var namePlateData = ScriptableObject.CreateInstance<NamePlateData>();
        namePlateData.name = Name;
        namePlateData.Free = true;
        namePlateData.ProductId = Id;
        namePlateData.ViewDataRef = new AssetReference(Id, Il2CppType.Of<NamePlateViewData>());
        return namePlateData;
    }
}