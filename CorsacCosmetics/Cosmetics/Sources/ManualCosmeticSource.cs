using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorsacCosmetics.Cosmetics.Sources;

public class ManualCosmeticSource(IEnumerable<CosmeticDescriptor> descriptors) : ICosmeticSource
{
    public string SourceId => "ManualCosmeticSource";

    public Task<IEnumerable<CosmeticDescriptor>> DiscoverAsync()
    {
        return Task.FromResult(descriptors);
    }
}