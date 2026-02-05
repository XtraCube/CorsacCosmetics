namespace CorsacCosmetics.Cosmetics.Hats;


public record struct HatMetadata()
{
    public string Name { get; set; } = "Custom Hat";
    public bool MatchPlayerColor { get; set; }
    public bool BlocksVisors { get; set; }
    public bool InFront { get; set; } = true;
    public bool NoBounce { get; set; } = true;
}