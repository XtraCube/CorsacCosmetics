using CorsacCosmetics.Cosmetics.Bundle;

namespace CorsacCosmetics.Cosmetics.Hats;

public class CustomHat
{
    public CustomHat(
        string id,
        HatData hatData,
        BundleSource? bundleSource = null,
        string? fileSource = null
        )
    {
        Id = id;
        HatData = hatData;
        BundleSource = bundleSource;
        FileSource = fileSource;
    }

    public string Id { get; }
    public HatData HatData { get; }

    /// <summary>
    /// When set, this hat's sprites are lazily decoded from the bundle file on first access.
    /// Null for folder-loaded cosmetics.
    /// </summary>
    public BundleSource? BundleSource { get; }

    /// <summary>
    /// When set, this hat's sprites are lazily decoded from the png file on first access.
    /// Null for bundle-loaded cosmetics.
    /// </summary>
    public string? FileSource { get; }

    /// <summary>
    /// A lock object to ensure thread-safe decoding of the hat's sprites.
    /// </summary>
    public readonly object DecodeLock = new();
}