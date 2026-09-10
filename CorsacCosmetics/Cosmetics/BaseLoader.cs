using System.Diagnostics.CodeAnalysis;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Cosmetics;

public abstract class BaseLoader
{
    public abstract void InstallCosmetics(ReferenceData refData);

    public abstract void LoadCosmetics(string directory);

    public abstract bool LocateCosmetic(string id, Il2CppSystem.Type type);

    public abstract bool ProvideCosmetic(ProvideHandle handle, string id, Il2CppSystem.Type type);

    public abstract bool ReleaseCosmetic(IResourceLocation location, Il2CppSystem.Object obj);
}