namespace CorsacCosmetics.Cosmetics.Visors;

public record struct VisorMetadata()
{
    public string Name { get; set; } = "Custom Visor";

    public bool MatchPlayerColor { get; set; } = false;

    public bool BehindHats { get; set; } = false;
}