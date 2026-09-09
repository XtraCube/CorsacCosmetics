using CorsacCosmetics.Cosmetics.Bundle;

namespace CorsacCosmetics.Cosmetics.Hats;

public class CustomHat
{
    public CustomHat(
        string id,
        HatData hatData,
        HatViewData viewData,
        PreviewViewData previewData,
        BundleSource? bundleSource = null
        )
    {
        Id = id;
        HatData = hatData;
        HatViewData = viewData;
        PreviewData = previewData;
        BundleSource = bundleSource;
    }

    public string Id { get; }
    public HatData HatData { get; }
    public HatViewData HatViewData { get; }
    public PreviewViewData PreviewData { get; }

    /// <summary>
    /// When set, this hat's sprites are lazily decoded from the bundle file on first access.
    /// Null for folder-loaded cosmetics, which are decoded eagerly.
    /// </summary>
    public BundleSource? BundleSource { get; }
}