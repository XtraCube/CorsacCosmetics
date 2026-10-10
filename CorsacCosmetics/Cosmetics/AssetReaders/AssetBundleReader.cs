using System;
using System.Collections;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CorsacCosmetics.Unity;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace CorsacCosmetics.Cosmetics.AssetReaders;

/// <summary>
/// Loads an asset from a Unity AssetBundle.
/// </summary>
public class AssetBundleReader(AssetBundle bundle) : ICosmeticAssetReader
{
    public Task<GameObject?> LoadGameObjectAsync(string objectKey)
    {
        var tcs = new TaskCompletionSource<GameObject?>();
        if (bundle == null)
        {
            tcs.SetException(new NullReferenceException("The target AssetBundle is null or has been destroyed."));
            return tcs.Task;
        }

        Coroutines.Instance.StartCoroutine(CoLoadAsset().WrapToIl2Cpp());
        return tcs.Task;

        IEnumerator CoLoadAsset()
        {
            AssetBundleRequest request;

            try
            {
                request = bundle.LoadAssetAsync(objectKey, Il2CppType.Of<GameObject>());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
                yield break;
            }

            while (true)
            {
                try
                {
                    if (request.isDone)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                    yield break;
                }

                yield return request;
            }

            try
            {
                var asset = request.GetResult();
                if (asset == null)
                {
                    tcs.SetResult(null);
                    yield break;
                }

                var go = asset.Cast<GameObject>();
                if (go == null)
                {
                    tcs.SetResult(null);
                    yield break;
                }

                tcs.SetResult(go);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        }
    }
}