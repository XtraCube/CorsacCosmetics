using UnityEngine;

namespace CorsacCosmetics.Loader;

public class CustomHat
{
    public CustomHat(string id, HatMetadata metadata, Sprite hatSprite, HatViewData viewData,
        PreviewViewData previewData, HatData hatData)
    {
        Id = id;
        Metadata = metadata;
        HatSprite = hatSprite;
        HatViewData = viewData;
        PreviewData = previewData;
        HatData = hatData;
    }

    public string Id { get; }
    public HatMetadata Metadata { get; }
    public Sprite HatSprite { get; }
    public HatViewData HatViewData { get; }
    public PreviewViewData PreviewData { get; }
    public HatData HatData { get; }
}