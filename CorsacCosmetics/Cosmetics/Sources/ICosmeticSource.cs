using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorsacCosmetics.Cosmetics.Sources;

public interface ICosmeticSource
{
    string SourceId { get; }
    Task<IEnumerable<CosmeticDescriptor>> DiscoverAsync();
}