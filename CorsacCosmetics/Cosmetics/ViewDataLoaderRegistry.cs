using System.Collections.Generic;

namespace CorsacCosmetics.Cosmetics;

public class ViewDataLoaderRegistry
{
    private readonly Dictionary<Il2CppSystem.Type, ICosmeticResourceLoader> _loaders = [];

    public void RegisterLoader(ICosmeticResourceLoader loader)
    {
        if (!_loaders.TryAdd(loader.SupportedType, loader))
        {
            Error($"Loader already registered for type {loader.SupportedType.FullName}");
        }
        else
        {
            Info($"Registered loader for type {loader.SupportedType.FullName}");
        }
    }

    public ICosmeticResourceLoader? GetLoader(Il2CppSystem.Type type)
    {
        return _loaders.GetValueOrDefault(type);
    }
}