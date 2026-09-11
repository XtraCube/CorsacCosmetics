namespace CorsacCosmetics.Cosmetics.Nameplates;

[System.Serializable]
public struct NamePlateMetadata : ICosmeticMetadata
{
    public string Name { get; set; } = "Custom Nameplate";
    public NamePlateMetadata() { }
}