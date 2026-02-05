using UnityEngine;

namespace CorsacCosmetics.Cosmetics.Visors;

public class CustomVisor
{
    public CustomVisor(
        string id,
        VisorMetadata metadata,
        Sprite visorSprite,
        VisorData visorData,
        VisorViewData visorViewData,
        PreviewViewData previewData
        )
    {
        Id = id;
        Metadata = metadata;
        VisorSprite = visorSprite;
        VisorData = visorData;
        VisorViewData = visorViewData;
        PreviewData = previewData;
    }

    public string Id { get; }
    
    public VisorMetadata Metadata { get; }

    public Sprite VisorSprite { get; }

    public VisorData VisorData { get; }

    public VisorViewData VisorViewData { get; }

    public PreviewViewData PreviewData { get; }
}