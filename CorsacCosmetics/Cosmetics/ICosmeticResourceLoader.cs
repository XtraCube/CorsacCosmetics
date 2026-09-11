using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Cosmetics;

public interface ICosmeticResourceLoader
{
    Il2CppSystem.Type SupportedType { get; }
    
    Task LoadAndCompleteAsync(ProvideHandle provideHandle, CosmeticDescriptor descriptor);
    
    void Release(Il2CppSystem.Object asset);
}