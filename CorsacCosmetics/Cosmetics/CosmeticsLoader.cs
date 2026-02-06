using System;
using System.Diagnostics.CodeAnalysis;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;
using CorsacCosmetics.Unity;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Cosmetics;

public class CosmeticsLoader
{
    private CosmeticsLoader()
    {
        EmptyKeys = new Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>(_emptyKeys.Pointer);
    }

    private static CosmeticsLoader? _cosmeticsLoader;
    public static CosmeticsLoader Instance => _cosmeticsLoader ??= new CosmeticsLoader();

    private readonly Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> _emptyKeys = new();

    public Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object> EmptyKeys { get; }

    private readonly HatLoader _hatLoader = new();
    private readonly VisorLoader  _visorLoader = new();
    private readonly NamePlateLoader _nameplateLoader = new();

    public void LoadCosmetics()
    {
        Info("Loading hats...");
        _hatLoader.LoadCosmetics(CosmeticPaths.HatPath);

        Info("Loading visors...");
        _visorLoader.LoadCosmetics(CosmeticPaths.VisorPath);

        Info("Loading nameplates...");
        _nameplateLoader.LoadCosmetics(CosmeticPaths.NameplatePath);
    }

    public void InstallCosmetics(ReferenceData referenceData)
    {
        Info("Installing hats...");
        _hatLoader.InstallCosmetics(referenceData);

        Info("Installing visors...");
        _visorLoader.InstallCosmetics(referenceData);

        Info("Installing nameplates");
        _nameplateLoader.InstallCosmetics(referenceData);
    }

    public bool LocateCosmetic(
        string id,
        string type,
        [NotNullWhen(true)] out Il2CppSystem.Type? il2CPPType
    )
    {
        il2CPPType = null;
        try
        {
            il2CPPType = type switch
            {
                ReferenceType.Preview => Il2CppType.Of<PreviewViewData>(),
                _ => null
            };

            return il2CPPType != null
                   || _hatLoader.LocateCosmetic(id, type, out il2CPPType)
                   || _visorLoader.LocateCosmetic(id, type, out il2CPPType)
                   || _nameplateLoader.LocateCosmetic(id, type, out il2CPPType);
        }
        catch (Exception e)
        {
            Error($"Unexpected error while locating cosmetic {id}:\n{e.ToString()}");
            return false;
        }
    }

    public bool ProvideCosmetic(
        ProvideHandle provideHandle,
        string id,
        string type,
        [NotNullWhen(false)] out Exception? exception
        )
    {
        exception = null;
        try
        {
            var result = 
                _hatLoader.ProvideCosmetic(provideHandle, id, type) 
                || _visorLoader.ProvideCosmetic(provideHandle, id, type)
                || _nameplateLoader.ProvideCosmetic(provideHandle, id, type);

            return result ? true : throw new Exception($"No cosmetic found for {id} and type {type}");
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }

    public bool TryGetHat(string id, [NotNullWhen(true)] out CustomHat? hat)
    {
        return _hatLoader.CustomHats.TryGetValue(id, out hat);
    }

    public bool TryGetVisor(string id, [NotNullWhen(true)] out CustomVisor? visor)
    {
        return _visorLoader.CustomVisors.TryGetValue(id, out visor);
    }

    public bool TryGetNamePlate(string id, [NotNullWhen(true)] out CustomNamePlate? namePlate)
    {
        return _nameplateLoader.CustomNamePlates.TryGetValue(id, out namePlate);
    }
}