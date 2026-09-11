namespace CorsacCosmetics.Cosmetics.Hats;

[System.Serializable]
public struct HatMetadata : ICosmeticMetadata
{
    public string Name { get; set; } = "Custom Hat";
    public bool MatchPlayerColor { get; set; } = false;
    public bool BlocksVisors { get; set; } = false;
    public bool InFront { get; set; } = true;
    public bool NoBounce { get; set; } = true;

    public HatMetadata() { }
}