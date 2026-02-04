using UnityEngine;

namespace CorsacCosmetics.Loader;

public class CustomHat
{
    public string Id { get; }
    public HatMetadata Metadata { get; }
    public Sprite HatSprite { get; }
    public HatViewData HatViewData { get; }

    public CustomHat(string id, HatMetadata metadata, Sprite hatSprite, HatViewData viewData)
    {
        Id = id;
        Metadata = metadata;
        HatSprite = hatSprite;
        HatViewData = viewData;
    }
}