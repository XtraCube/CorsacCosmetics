using System.Text.Json.Serialization;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;

namespace CorsacCosmetics.Cosmetics.Bundle;

[JsonSerializable(typeof(BundleManifest))]
public partial class BundleManifestContext : JsonSerializerContext;

public struct BundleManifest()
{
    public const uint CurrentVersion = 1;

    [JsonIgnore]
    public bool IsValid => Version is > 0 and <= CurrentVersion;

    public uint Version { get; set; } = 0;
    public HatManifest[] Hats { get; set; } = [];
    public VisorManifest[] Visors { get; set; } = [];
    public NameplateManifest[] Nameplates { get; set; } = [];

    public override string ToString()
    {
        return $"BundleManifest (Version {Version}, Hats: {Hats.Length}, Visors: {Visors.Length}, Nameplates: {Nameplates.Length})";
    }
}

public struct HatManifest()
{
    public string Name { get; set; } = "Custom Hat";
    public bool MatchPlayerColor { get; set; }
    public bool BlocksVisors { get; set; }
    public bool InFront { get; set; } = true;
    public bool NoBounce { get; set; } = true;

    public SpriteData PreviewSprite { get; set; } = new();
    public SpriteData MainSprite { get; set; } = new();
    public SpriteData BackSprite { get; set; } = new();
    public SpriteData ClimbSprite { get; set; } = new();
    public SpriteData FloorSprite { get; set; } = new();
    public SpriteData LeftMainSprite { get; set; } = new();
    public SpriteData LeftBackSprite { get; set; } = new();
    public SpriteData LeftClimbSprite { get; set; } = new();
    public SpriteData LeftFloorSprite { get; set; } = new();

    public HatMetadata ToMetadata() => new()
    {
        Name = Name,
        MatchPlayerColor = MatchPlayerColor,
        BlocksVisors = BlocksVisors,
        InFront = InFront,
        NoBounce = NoBounce
    };

    public BundleSource ToBundleSource(string bundlePath, long dataStart) =>
        new BundleSource(bundlePath, dataStart)
            .AddSprite("", MainSprite)
            .AddSprite("preview", PreviewSprite)
            .AddSprite("back", BackSprite)
            .AddSprite("climb", ClimbSprite)
            .AddSprite("floor", FloorSprite)
            .AddSprite("left", LeftMainSprite)
            .AddSprite("leftback", LeftBackSprite)
            .AddSprite("leftclimb", LeftClimbSprite)
            .AddSprite("leftfloor", LeftFloorSprite);
}

public struct VisorManifest()
{
    public string Name { get; set; } = "Custom Visor";
    public bool MatchPlayerColor { get; set; } = false;
    public bool BehindHats { get; set; } = false;

    public SpriteData PreviewSprite { get; set; } = new();
    public SpriteData IdleSprite { get; set; } = new();
    public SpriteData LeftIdleSprite { get; set; } = new();
    public SpriteData FloorSprite { get; set; } = new();
    public SpriteData ClimbSprite { get; set; } = new();

    public VisorMetadata ToMetadata() => new()
    {
        Name = Name,
        MatchPlayerColor = MatchPlayerColor,
        BehindHats = BehindHats
    };

    public BundleSource ToBundleSource(string bundlePath, long dataStart) =>
        new BundleSource(bundlePath, dataStart)
            .AddSprite("", IdleSprite)
            .AddSprite("preview", PreviewSprite)
            .AddSprite("climb", ClimbSprite)
            .AddSprite("floor", FloorSprite)
            .AddSprite("left", LeftIdleSprite);
}

public struct NameplateManifest()
{
    public string Name { get; set; } = "Custom Nameplate";

    public SpriteData PreviewSprite { get; set; } = new();
    public SpriteData NameplateSprite { get; set; } = new();

    public NamePlateMetadata ToMetadata() => new()
    {
        Name = Name
    };

    public BundleSource ToBundleSource(string bundlePath, long dataStart) =>
        new BundleSource(bundlePath, dataStart)
            .AddSprite("", NameplateSprite)
            .AddSprite("preview", PreviewSprite);
}

public struct SpriteData()
{
    public uint Size { get; set; } = 0;
    public uint Offset { get; set; } = 0;

    [JsonIgnore]
    public bool HasData => Size > 0;
}
