using CorsacCosmetics.Cosmetics.Bundle;

namespace CorsacCosmetics.Cosmetics.Visors;

public class CustomVisor
{
    public CustomVisor(
        string id,
        VisorData visorData,
        VisorViewData visorViewData,
        PreviewViewData previewData,
        BundleSource? bundleSource = null
        )
    {
        Id = id;
        VisorData = visorData;
        VisorViewData = visorViewData;
        PreviewData = previewData;
        BundleSource = bundleSource;
    }

    public string Id { get; }

    public VisorData VisorData { get; }

    public VisorViewData VisorViewData { get; }

    public PreviewViewData PreviewData { get; }

    /// <summary>
    /// When set, this visor's sprites are lazily decoded from the bundle file on first access.
    /// Null for folder-loaded cosmetics, which are decoded eagerly.
    /// </summary>
    public BundleSource? BundleSource { get; }

    /// <summary>
    /// A lock object to ensure thread-safe decoding of the visor's sprites.
    /// </summary>
    public readonly object DecodeLock = new();
}