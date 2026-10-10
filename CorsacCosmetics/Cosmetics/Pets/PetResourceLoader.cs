using System;
using System.Threading.Tasks;

namespace CorsacCosmetics.Cosmetics.Pets;

public class PetResourceLoader : BaseCosmeticResourceLoader<PetBehaviour>
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

        gameObject = UnityEngine.Object.Instantiate(gameObject);
        var pet = gameObject.GetComponent<PetBehaviour>();
        pet.data = descriptor.ToCosmeticData<PetData>();
        return pet;
    }

    protected override void ReleaseViewData(PetBehaviour pet)
    {
        UnityEngine.Object.Destroy(pet.gameObject);
    }
}