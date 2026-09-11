using System.Collections.Generic;

namespace CorsacCosmetics.Cosmetics;

public class ViewDataFactoryRegistry
{
    private readonly Dictionary<CosmeticType, ICosmeticViewDataFactory> _factories = [];

    public void RegisterFactory(ICosmeticViewDataFactory factory)
    {
        _factories[factory.SupportedType] = factory;
    }

    public ICosmeticViewDataFactory GetFactory(CosmeticType cosmeticType)
    {
        return _factories[cosmeticType];
    }
}