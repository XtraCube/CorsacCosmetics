using System.Collections.Generic;

namespace CorsacCosmetics.Cosmetics;

public class ViewDataLoaderRegistry
{
    private readonly Dictionary<Il2CppSystem.Type, ICosmeticResourceLoader> _loaders = [];

    public void RegisterLoader(ICosmeticResourceLoader loader)
    {
        _loaders[loader.SupportedType] = loader;
    }

    public ICosmeticResourceLoader GetLoader(Il2CppSystem.Type type)
    {
        return _loaders[type];
    }
}