using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorsacCosmetics.Cosmetics.Sources;

public class SourceRegistry
{
    private readonly List<ICosmeticSource> _sources = [];

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