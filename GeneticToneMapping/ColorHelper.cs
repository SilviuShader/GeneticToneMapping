using Microsoft.Xna.Framework;

namespace GeneticToneMapping
{
    internal static class ColorHelper
    {
        public static float Luminance(Vector3 color)
        {
            return Vector3.Dot(color, new Vector3(0.299f, 0.587f, 0.114f));
        }
    }
}
