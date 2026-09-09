using CorsacCosmetics.Cosmetics.Bundle;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public class CustomNamePlate
{
    public CustomNamePlate(
        string id,
        NamePlateData namePlateData,
        NamePlateViewData namePlateViewData,
        PreviewViewData previewData,
        BundleSource? bundleSource = null
        )
    {
        Id = id;
        NamePlateData = namePlateData;
        NamePlateViewData = namePlateViewData;
        PreviewData = previewData;
        BundleSource = bundleSource;
    }

    public string Id { get; }

    public NamePlateData NamePlateData { get; }

    public NamePlateViewData NamePlateViewData { get; }

    public PreviewViewData PreviewData { get; }

    /// <summary>
    /// When set, this nameplate's sprites are lazily decoded from the bundle file on first access.
    /// Null for folder-loaded cosmetics, which are decoded eagerly.
    /// </summary>
    public BundleSource? BundleSource { get; }
}