
using OpenCvSharp;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace GeneticToneMapping
{
    internal class OpenCVHelper
    {
        public static unsafe void CopyMat(ref Vec3f[] destination, Mat source)
        {
            fixed (void* ptr = destination)
                Unsafe.CopyBlock(ptr, source.DataPointer, (uint)source.Width * (uint)source.Height * 3 * sizeof(float));
        }

        public static unsafe void CopyMat(ref Vector3[] destination, Mat source)
        {
            fixed (void* ptr = destination)
                Unsafe.CopyBlock(ptr, source.DataPointer, (uint)source.Width * (uint)source.Height * 3 * sizeof(float));
        }
    }
}
