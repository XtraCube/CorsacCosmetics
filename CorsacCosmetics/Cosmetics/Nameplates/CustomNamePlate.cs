using UnityEngine;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public class CustomNamePlate
{
    public CustomNamePlate(
        string id,
        NamePlateMetadata metadata,
        NamePlateData namePlateData,
        NamePlateViewData namePlateViewData,
        PreviewViewData previewData
        )
    {
        Id = id;
        Metadata = metadata;
        NamePlateData = namePlateData;
        NamePlateViewData = namePlateViewData;
        PreviewData = previewData;
    }

    public string Id { get; }
    
    public NamePlateMetadata Metadata { get; }


    public NamePlateData NamePlateData { get; }

    public NamePlateViewData NamePlateViewData { get; }

    public PreviewViewData PreviewData { get; }
}