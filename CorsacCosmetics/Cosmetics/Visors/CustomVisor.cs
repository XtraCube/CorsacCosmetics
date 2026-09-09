using System;

namespace CorsacCosmetics.Cosmetics.Visors;

public class CustomVisor(
    string id,
    VisorData visorData,
    Func<PreviewViewData> previewViewDataFactory,
    Func<VisorViewData> visorViewDataFactory)
{
    /// <summary>
    /// Gets the unique ID generated for this visor.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Gets the vanilla VisorData object associated with this visor.
    /// </summary>
    public VisorData VisorData { get; } = visorData;

    /// <summary>
    /// Gets the factory function to create a PreviewViewData for this visor.
    /// </summary>
    public Func<PreviewViewData> PreviewViewDataFactory { get; } = previewViewDataFactory;

    /// <summary>
    /// Gets the factory function to create a VisorViewData for this visor
    /// </summary>
    public Func<VisorViewData> VisorViewDataFactory { get; } = visorViewDataFactory;

    /// <summary>
    /// A lock object to ensure thread-safe decoding of the visor's sprites.
    /// </summary>
    public readonly object DecodeLock = new();
}