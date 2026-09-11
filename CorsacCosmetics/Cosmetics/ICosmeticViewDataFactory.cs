using System.Threading.Tasks;

namespace CorsacCosmetics.Cosmetics;

public interface ICosmeticViewDataFactory
{
    CosmeticType SupportedType { get; }
    Task<UnityEngine.Object> CreateViewData(CosmeticDescriptor descriptor);
    void ReleaseViewData(UnityEngine.Object viewData);
}