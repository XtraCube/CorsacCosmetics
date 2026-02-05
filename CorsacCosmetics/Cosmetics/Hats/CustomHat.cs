using UnityEngine;

namespace CorsacCosmetics.Cosmetics.Hats;

public class CustomHat
{
    public CustomHat(
        string id,
        HatMetadata metadata,
        HatData hatData,
        HatViewData viewData,
        PreviewViewData previewData
        )
    {
        Id = id;
        Metadata = metadata;
        HatData = hatData;
        HatViewData = viewData;
        PreviewData = previewData;
    }

    public string Id { get; }
    public HatMetadata Metadata { get; }
    public HatData HatData { get; }
    public HatViewData HatViewData { get; }
    public PreviewViewData PreviewData { get; }
}