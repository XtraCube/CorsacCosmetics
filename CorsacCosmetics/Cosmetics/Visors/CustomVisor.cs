using CorsacCosmetics.Cosmetics.Bundle;

namespace CorsacCosmetics.Cosmetics.Visors;

public class CustomVisor
{
    public CustomVisor(
        string id,
        VisorData visorData,
        BundleSource? bundleSource = null,
        string? fileSource = null
        )
    {
        Id = id;
        VisorData = visorData;
        BundleSource = bundleSource;
        FileSource = fileSource;
    }

    public string Id { get; }

    public VisorData VisorData { get; }

    /// <summary>
    /// When set, this visor's sprites are lazily decoded from the bundle file on first access.
    /// Null for folder-loaded cosmetics.
    /// </summary>
    public BundleSource? BundleSource { get; }

    /// <summary>
    /// When set, this visor's sprites are lazily decoded from the file path on first access.
    /// Null for bundle-loaded cosmetics.
    /// </summary>
    public string? FileSource { get; }

    /// <summary>
    /// A lock object to ensure thread-safe decoding of the visor's sprites.
    /// </summary>
    public readonly object DecodeLock = new();
}