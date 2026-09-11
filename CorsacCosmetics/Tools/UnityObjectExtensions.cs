// https://github.com/NuclearPowered/Reactor/blob/9bd370bfc10d666431efdb70f4cd02f0d9eefe91/Reactor/Utilities/Extensions/UnityObjectExtensions.cs#L1

using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace CorsacCosmetics.Tools;

/// <summary>
/// Provides extension methods for <see cref="UnityObject"/>.
/// </summary>
public static class UnityObjectExtensions
{
    /// <param name="obj">The object to stop from being destroyed.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    extension<T>(T obj) where T : UnityObject
    {
        /// <summary>
        /// Stops <paramref name="obj"/> from being destroyed.
        /// </summary>
        /// <returns>Passed <paramref name="obj"/>.</returns>
        public T DontDestroy()
        {
            obj.hideFlags |= HideFlags.HideAndDontSave;

            return obj.DontDestroyOnLoad();
        }

        /// <summary>
        /// Stops <paramref name="obj"/> from being unloaded.
        /// </summary>
        /// <returns>Passed <paramref name="obj"/>.</returns>
        public T DontUnload()
        {
            obj.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            return obj;
        }

        /// <summary>
        /// Stops <paramref name="obj"/> from being destroyed on load.
        /// </summary>
        /// <returns>Passed <paramref name="obj"/>.</returns>
        public T DontDestroyOnLoad()
        {
            UnityObject.DontDestroyOnLoad(obj);

            return obj;
        }
    }

    /// <param name="obj">The object to destroy.</param>
    extension(UnityObject obj)
    {
        /// <summary>
        /// Destroys the <paramref name="obj"/>.
        /// </summary>
        public void Destroy()
        {
            UnityObject.Destroy(obj);
        }

        /// <summary>
        /// Destroys the <paramref name="obj"/> immediately.
        /// </summary>
        public void DestroyImmediate()
        {
            UnityObject.DestroyImmediate(obj);
        }
    }
}