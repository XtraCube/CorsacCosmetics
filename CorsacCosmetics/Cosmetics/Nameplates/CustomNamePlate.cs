using UnityEngine;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public class CustomNamePlate
{
    public CustomNamePlate(
        string id,
        NamePlateMetadata metadata,
        Sprite namePlateSprite,
        NamePlateData namePlateData,
        NamePlateViewData namePlateViewData,
        PreviewViewData previewData
        )
    {
        Id = id;
        Metadata = metadata;
        NamePlateSprite = namePlateSprite;
        NamePlateData = namePlateData;
        NamePlateViewData = namePlateViewData;
        PreviewData = previewData;
    }

    public string Id { get; }
    
    public NamePlateMetadata Metadata { get; }

    public Sprite NamePlateSprite { get; }

    public NamePlateData NamePlateData { get; }

    public NamePlateViewData NamePlateViewData { get; }

    public PreviewViewData PreviewData { get; }
}