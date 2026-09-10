using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CorsacCosmetics.Cosmetics.Bundle;
using CorsacCosmetics.Cosmetics.Bundle.V2;
using CorsacCosmetics.Cosmetics.Hats;
using CorsacCosmetics.Cosmetics.Nameplates;
using CorsacCosmetics.Cosmetics.Visors;
using CorsacCosmetics.Unity;
using Il2CppInterop.Runtime;
using Il2CppSystem.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CorsacCosmetics.Cosmetics;

public class CosmeticsLoader
{
    private static CosmeticsLoader? _cosmeticsLoader;
    public static CosmeticsLoader Instance => _cosmeticsLoader ??= new CosmeticsLoader();

    private readonly Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> _emptyKeys = new();

    public Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object> EmptyKeys { get; }

    private CosmeticReleaseGroup CosmeticGroup { get; }

    // ID -> Name
    private Dictionary<string, string> CustomGroups { get; }

    // used to prevent groups with empty # of certain elements showing in inventory
    public Group HatGroups { get; }
    public Group VisorGroups { get; }
    public Group NameplateGroups { get; }

    private readonly BundleLoader _bundleLoader;
    private readonly BundleLoaderV2 _bundleLoaderV2;

    private readonly HatLoader _hatLoader = new();
    private readonly VisorLoader _visorLoader  = new();
    private readonly NameplateLoader _nameplateLoader = new();

    private CosmeticsLoader()
    {
        EmptyKeys = new Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>(_emptyKeys.Pointer);
        CosmeticGroup = ScriptableObject.CreateInstance<CosmeticReleaseGroup>();
        CosmeticGroup.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        CustomGroups = [];
        CustomGroups.Add("default", "Custom Cosmetics");

        HatGroups = new Group(CustomGroups);
        VisorGroups = new Group(CustomGroups);
        NameplateGroups = new Group(CustomGroups);

        _bundleLoader = new BundleLoader(_hatLoader, _visorLoader, _nameplateLoader);
        _bundleLoaderV2 = new BundleLoaderV2(_hatLoader, _visorLoader, _nameplateLoader, CustomGroups);
    }

    public void LoadCosmetics()
    {
        Info("Loading bundles...");
        foreach (var file in Directory.GetFiles(CosmeticPaths.BundlePath, "*.ccb"))
        {
            if (file == null) continue;
            var v1Loaded = false;
            try
            {
                v1Loaded = _bundleLoader.LoadBundle(file);
            }
            catch (Exception e)
            {
                Error($"Error while loading bundle {file} with v1 loader:\n{e}");
            }

            if (v1Loaded)
            {
                continue;
            }

            try
            {
                _bundleLoaderV2.LoadBundle(file);
            }
            catch (Exception e)
            {
                Error($"Error while loading bundle {file} with v2 loader:\n{e}");
            }
        }

        Info("Loading hats...");
        _hatLoader.LoadCosmetics(CosmeticPaths.HatPath);

        Info("Loading visors...");
        _visorLoader.LoadCosmetics(CosmeticPaths.VisorPath);

        Info("Loading nameplates...");
        _nameplateLoader.LoadCosmetics(CosmeticPaths.NameplatePath);

        Info("Setting up cosmetic group...");
        foreach (var id in _hatLoader.CustomHats.Keys)
        {
            var group = Names.GetGroup(id);
            HatGroups.AddGroup(group);
            CosmeticGroup.ids.Add(id);
        }
        foreach (var id in _visorLoader.CustomVisors.Keys)
        {
            var group = Names.GetGroup(id);
            VisorGroups.AddGroup(group);
            CosmeticGroup.ids.Add(id);
        }
        foreach (var id in _nameplateLoader.CustomNamePlates.Keys)
        {
            var group = Names.GetGroup(id);
            NameplateGroups.AddGroup(group);
            CosmeticGroup.ids.Add(id);
        }
    }

    public void InstallCosmetics(ReferenceData referenceData)
    {
        Info("Installing hats...");
        _hatLoader.InstallCosmetics(referenceData);

        Info("Installing visors...");
        _visorLoader.InstallCosmetics(referenceData);

        Info("Installing nameplates");
        _nameplateLoader.InstallCosmetics(referenceData);

        Info("Installing cosmetic group...");
        var newGroups = referenceData.Groups.releaseGroups.ToList();
        newGroups.Add(CosmeticGroup);
        referenceData.Groups.releaseGroups = newGroups.ToArray();
    }

    public bool LocateCosmetic(
        string id,
        Il2CppSystem.Type type
    )
    {
        if (_hatLoader.LocateCosmetic(id, type)
            || _visorLoader.LocateCosmetic(id, type)
            || _nameplateLoader.LocateCosmetic(id, type))
        {
            return true;
        }

        Error($"Failed to locate cosmetic for id {id} and type {type.FullName}");
        return false;
    }

    public bool ReleaseCosmetic(
        IResourceLocation location,
        Il2CppSystem.Object obj
    )
    {
        if (_hatLoader.ReleaseCosmetic(location, obj) 
            || _visorLoader.ReleaseCosmetic(location, obj) 
            || _nameplateLoader.ReleaseCosmetic(location, obj))
        {
            return true;
        }

        Error($"Failed to release cosmetic for location {location.InternalId} and object {obj.GetIl2CppType().NameOrDefault}");        
        return false;
    }

    public bool ProvideCosmetic(
        ProvideHandle provideHandle,
        string id,
        Il2CppSystem.Type type,
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