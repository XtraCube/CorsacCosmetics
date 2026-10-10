namespace CorsacCosmetics.Cosmetics.Pets;

public struct PetMetadata : ICosmeticMetadata
{
    public string Name { get; set; } = "Custom Pet";

    public string ReferenceName { get; set; } = "CustomPet.prefab";

    public bool MatchPlayerColor { get; set; } = false;

    public PetMetadata() { }
}