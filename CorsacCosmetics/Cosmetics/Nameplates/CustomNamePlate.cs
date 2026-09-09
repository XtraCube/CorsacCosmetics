using System;

namespace CorsacCosmetics.Cosmetics.Nameplates;

/// <summary>
/// Represents a custom nameplate object.
/// </summary>
public class CustomNamePlate(
    string id,
    NamePlateData namePlateData,
    Func<PreviewViewData> previewViewDataFactory,
    Func<NamePlateViewData> namePlateViewDataFactory)
{
    /// <summary>
    /// Gets the unique ID generated for this nameplate.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Gets the vanilla NamePlateData object associated with this nameplate.
    /// </summary>
    public NamePlateData NamePlateData { get; } = namePlateData;

    /// <summary>
    /// Gets the factory function to create a PreviewViewData for this nameplate.
    /// </summary>
    public Func<PreviewViewData> PreviewViewDataFactory { get; } = previewViewDataFactory;

    /// <summary>
    /// Gets the factory function to create a NamePlateViewData for this nameplate.
    /// </summary>
    public Func<NamePlateViewData> NamePlateViewDataFactory { get; } = namePlateViewDataFactory;

    /// <summary>
    /// A lock object to ensure thread-safe decoding of the nameplate's sprites.
    /// </summary>
    public readonly object DecodeLock = new();
}