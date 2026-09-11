using System.Collections.Generic;

namespace CorsacCosmetics.Cosmetics;

public class CosmeticsCatalog
{
    private static CosmeticsCatalog _instance = null!;
    public static CosmeticsCatalog Instance => _instance;
    
    private readonly Dictionary<string, CosmeticDescriptor> _descriptorsById = new();

    // ID -> Name
    private Dictionary<string, string> CustomGroups { get; }

    // used to prevent groups with empty # of certain elements showing in inventory
    public Group HatGroups { get; }
    public Group VisorGroups { get; }
    public Group NameplateGroups { get; }

    public CosmeticsCatalog()
    {
        _instance = this;

        CustomGroups = [];
        CustomGroups.Add("default", "Custom Cosmetics");

        HatGroups = new Group(CustomGroups);
        VisorGroups = new Group(CustomGroups);
        NameplateGroups = new Group(CustomGroups);
    }

    public void Register(CosmeticDescriptor descriptor)
    {
        _descriptorsById[descriptor.Id] = descriptor;
        switch (descriptor.Type)
        {
            case CosmeticType.Hat:
                CustomGroups.TryAdd(descriptor.GroupId, descriptor.Group);
                HatGroups.AddGroup(descriptor.GroupId);
                break;
            case CosmeticType.Visor:
                CustomGroups.TryAdd(descriptor.GroupId, descriptor.Group);
                VisorGroups.AddGroup(descriptor.GroupId);
                break;
            case CosmeticType.NamePlate:
                CustomGroups.TryAdd(descriptor.GroupId, descriptor.Group);
                NameplateGroups.AddGroup(descriptor.GroupId);
                break;
        }
    }

    public CosmeticDescriptor? Get(string id) => _descriptorsById.GetValueOrDefault(id);
}