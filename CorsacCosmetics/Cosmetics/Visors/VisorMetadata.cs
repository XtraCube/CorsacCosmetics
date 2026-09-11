namespace CorsacCosmetics.Cosmetics.Visors;

[System.Serializable]
public struct VisorMetadata : ICosmeticMetadata
{
    public string Name { get; set; } = "Custom Visor";
    public bool MatchPlayerColor { get; set; } = false;
    public bool BehindHats { get; set; } = false;

    public VisorMetadata() { }
}