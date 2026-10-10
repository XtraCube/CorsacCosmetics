using System.Threading.Tasks;
using CorsacCosmetics.Cosmetics.Pets;
using CorsacCosmetics.Tools;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics;

public class PreviewResourceLoader : BaseCosmeticResourceLoader<PreviewViewData>
{
    public override CosmeticType CosmeticType => CosmeticType.Preview;

    protected override async Task<PreviewViewData> CreateViewDataAsync(CosmeticDescriptor descriptor)
    {
        var viewData = ScriptableObject.CreateInstance<PreviewViewData>();
        viewData.name = descriptor.DisplayName;

        if (descriptor.Metadata is PetMetadata petMetadata)
        {
            var gameObj = await descriptor.AssetReader.LoadGameObjectAsync(petMetadata.ReferenceName);
            if (gameObj != null)
            {
                viewData.PreviewSprite = gameObj.GetComponentInChildren<SpriteRenderer>().sprite;
            }
        }
        else
        {
            viewData.PreviewSprite = await descriptor.AssetReader.LoadSpriteAsync("preview")
                                     ?? await descriptor.AssetReader.LoadSpriteAsync("");
        }

        return viewData;
    }

    protected override void ReleaseViewData(PreviewViewData viewData)
    {
        viewData.Release();
    }
}