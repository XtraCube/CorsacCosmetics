using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorsacCosmetics.Cosmetics.Sources;

public class SourceRegistry
{
    public static SourceRegistry Instance { get; private set; } = null!;

    private readonly List<ICosmeticSource> _sources = [];

    public SourceRegistry()
    {
        Instance = this;
    }

    public void RegisterSource(ICosmeticSource source) => _sources.Add(source);

    public async Task<List<CosmeticDescriptor>> DiscoverAllAsync()
    {
        var descriptors = new List<CosmeticDescriptor>();
        foreach (var source in _sources)
        {
            try
            {
                var discovered = await source.DiscoverAsync();
                descriptors.AddRange(discovered);
            }
            catch (Exception ex)
            {
                Error($"Error loading from source {source.SourceId} : {ex.Message}");
            }
        }
        return descriptors;
    }
}