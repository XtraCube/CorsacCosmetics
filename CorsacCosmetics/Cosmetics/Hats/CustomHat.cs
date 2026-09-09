using System;

namespace CorsacCosmetics.Cosmetics.Hats;

/// <summary>
/// Represents a custom hat object.
/// </summary>
public class CustomHat(
    string id,
    HatData hatData,
    Func<PreviewViewData> previewViewDataFactory,
    Func<HatViewData> hatViewDataFactory)
{
    /// <summary>
    /// Gets the unique ID generated for this hat.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Gets the vanilla HatData object associated with this hat.
    /// </summary>
    public HatData HatData { get; } = hatData;

    /// <summary>
    /// Gets the factory function to create a PreviewViewData for this hat.
    /// </summary>
    public Func<PreviewViewData> PreviewViewDataFactory { get; } = previewViewDataFactory;

    /// <summary>
    /// Gets the factory function to create a HatViewData for this hat.
    /// </summary>
    public Func<HatViewData> HatViewDataFactory { get; } = hatViewDataFactory;

    /// <summary>
    /// A lock object to ensure thread-safe decoding of the hat's sprites.
    /// </summary>
    public readonly object DecodeLock = new();
}