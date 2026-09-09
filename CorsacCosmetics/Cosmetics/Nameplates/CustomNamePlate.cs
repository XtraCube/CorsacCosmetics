using CorsacCosmetics.Cosmetics.Bundle;

namespace CorsacCosmetics.Cosmetics.Nameplates;

public class CustomNamePlate
{
    public CustomNamePlate(
        string id,
        NamePlateData namePlateData,
        BundleSource? bundleSource = null,
        string? fileSource = null
        )
    {
        Id = id;
        NamePlateData = namePlateData;
        BundleSource = bundleSource;
        FileSource = fileSource;
    }

    public string Id { get; }

    public NamePlateData NamePlateData { get; }

    /// <summary>
    /// When set, this nameplate's sprites are lazily decoded from the bundle file on first access.
    /// Null for folder-loaded cosmetics.
    /// </summary>
    public BundleSource? BundleSource { get; }

    /// <summary>
    /// When set, this nameplate's sprites are lazily decoded from the file path on first access.
    /// Null for bundle-loaded cosmetics.
    /// </summary>
    public string? FileSource { get; }

    /// <summary>
    /// A lock object to ensure thread-safe decoding of the nameplate's sprites.
    /// </summary>
    public readonly object DecodeLock = new();
}