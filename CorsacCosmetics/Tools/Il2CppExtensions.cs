using System;
using System.IO;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace CorsacCosmetics.Tools;

public static class Il2CppExtensions
{
    extension(Il2CppStructArray<byte> destination)
    {
        /// <summary>
        /// Fast memory copy from a byte array to an Il2CppStructArray.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length"></param>
        public unsafe void CopyFrom(byte[] source, int length)
        {
            fixed (byte* sourcePtr = source)
            {
                var destPtr = (byte*)IntPtr.Add(destination.Pointer, 4 * IntPtr.Size).ToPointer();
                Buffer.MemoryCopy(sourcePtr, destPtr, length, length);
            }
        }

        /// <summary>
        /// Reads directly from the stream into the native Il2CppStructArray, avoiding an intermediate managed buffer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        /// <exception cref="EndOfStreamException"></exception>
        public unsafe void CopyFromStream(Stream stream, int length)
        {
            var destPtr = (byte*)IntPtr.Add(destination.Pointer, 4 * IntPtr.Size).ToPointer();
            if (stream.Read(new Span<byte>(destPtr, length)) != length)
                throw new EndOfStreamException("Could not read the expected number of bytes from the stream.");
        }
    }
}