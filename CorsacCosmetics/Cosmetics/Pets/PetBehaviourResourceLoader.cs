using System;
using System.Threading.Tasks;

namespace CorsacCosmetics.Cosmetics.Pets;

public class PetBehaviourResourceLoader : BaseCosmeticResourceLoader<PetBehaviour>
{
    public override CosmeticType CosmeticType => CosmeticType.Pet;

    protected override async Task<PetBehaviour> CreateViewDataAsync(CosmeticDescriptor descriptor)
    {
        if (descriptor.Type != CosmeticType)
            throw new ArgumentException($"Descriptor is not of type {CosmeticType}");

        var metadata = (PetMetadata)descriptor.Metadata;
        var gameObject = await descriptor.AssetReader.LoadGameObjectAsync(metadata.ReferenceName);
        if (gameObject == null)
        {
            throw new NullReferenceException(
                "Cannot get PetBehaviour for a null GameObject! Check your AssetBundle reference!");
        }

        var pet = gameObject.GetComponent<PetBehaviour>();
        if (!pet.data) pet.data = descriptor.ToCosmeticData<PetData>();
        return pet;
    }

    protected override void ReleaseViewData(PetBehaviour pet)
    {
        // no-op, game will cleanup the pet on its own.
    }
}