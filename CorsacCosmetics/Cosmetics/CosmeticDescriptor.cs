using System;
using System.Security.Cryptography;
using System.Text;
using CorsacCosmetics.Cosmetics.AssetReaders;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;

namespace CorsacCosmetics.Cosmetics;

public class CosmeticDescriptor
{
    public string Id { get; }
    public string DisplayName { get; }
    public string Group { get; }
    public CosmeticType Type { get; }
    public string SourceId { get; }
    public string GroupId { get; }
    
    public ICosmeticMetadata Metadata { get; }
    public ICosmeticAssetReader AssetReader { get; }

    public CosmeticDescriptor(
        string sourceId, 
        string group, 
        string name, 
        CosmeticType type,
        ICosmeticMetadata metadata,
        ICosmeticAssetReader reader
        )
    {
        SourceId = sourceId;
        DisplayName = name;
        Group = group;
        Type = type;
        Metadata = metadata;
        AssetReader = reader;
        GroupId = GenerateGuid(sourceId, group, type.ToString());
        Id = GenerateGuid(sourceId, group, type.ToString(), name);
    }

    public CosmeticData ToCosmeticData()
    {
        return Type switch
        {
            CosmeticType.Hat when Metadata is HatMetadata hatMetadata => new HatDataBuilder(Id)
                .SetFromMetadata(hatMetadata).Build(),
            CosmeticType.Visor when Metadata is VisorMetadata visorMetadata => new VisorDataBuilder(Id)
                .SetFromMetadata(visorMetadata).Build(),
            CosmeticType.NamePlate when Metadata is NamePlateMetadata namePlateMetadata => new NamePlateDataBuilder(Id)
                .SetFromMetadata(namePlateMetadata).Build(),
            _ => throw new NotSupportedException()
        };
    }

    public T ToCosmeticData<T>() where T : CosmeticData
    {
        return ToCosmeticData().Cast<T>();
    }

    private const byte UnitSeparator = 0x1F; 
    private static string GenerateGuid(params string[] parts)
    {
        using var incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

        for (var i = 0; i < parts.Length; i++)
        {
            var currentPart = parts[i];
            
            incrementalHash.AppendData(Encoding.UTF8.GetBytes(currentPart));

            if (i < parts.Length - 1)
            {
                ReadOnlySpan<byte> separatorSpan = [UnitSeparator];
                incrementalHash.AppendData(separatorSpan);
            }
        }

        Span<byte> hashBytes = stackalloc byte[16];
        incrementalHash.GetCurrentHash(hashBytes);

        return new Guid(hashBytes).ToString();
    }
}