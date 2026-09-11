using System;
using System.Threading.Tasks;
using Il2CppInterop.Runtime;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Cosmetics;

public abstract class BaseCosmeticResourceLoader<TViewData> : ICosmeticResourceLoader where TViewData : UnityEngine.Object
{
    public abstract CosmeticType CosmeticType { get; }

    public Il2CppSystem.Type SupportedType => Il2CppType.Of<TViewData>();

    public async Task LoadAndCompleteAsync(ProvideHandle provideHandle, CosmeticDescriptor descriptor)
    {
        try
        {
            var viewData = await CreateViewDataAsync(descriptor);

            if (viewData != null)
            {
                provideHandle.Complete(viewData, status: true, exception: null);
            }
            else
            {
                provideHandle.Complete<TViewData>(
                    null!, 
                    status: false, 
                    new Il2CppSystem.Exception($"Loader returned null for {descriptor.Id}")
                );
            }
        }
        catch (Exception ex)
        {
            provideHandle.Complete<TViewData>(null!, status: false, new Il2CppSystem.Exception(ex.Message));
        }
    }

    public void Release(Il2CppSystem.Object asset)
    {
        if (asset.TryCast<TViewData>() is { } typedAsset)
        {
            ReleaseViewData(typedAsset);
        }
    }

    protected abstract Task<TViewData> CreateViewDataAsync(CosmeticDescriptor descriptor);
    protected abstract void ReleaseViewData(TViewData viewData);
}