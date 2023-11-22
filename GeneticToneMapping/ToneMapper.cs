using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace GeneticToneMapping
{
    internal static class ToneMapper
    {
        public static Color[] ToneMap(HDRImage image, IEnumerable<IToneMap> toneMaps)
        {
            var result = new Color[image.Data.Length];
            Array.Fill(result, Color.Black);

            var totalWeights = 0.0f;

            foreach (var toneMap in toneMaps)
            {
                toneMap.SetImage(image);
                totalWeights += toneMap.Weight;
            }

            for (var i = 0; i < image.Data.Length; i++)
            {
                var accumulatedColor = Vector3.Zero;
                foreach (var toneMap in toneMaps)
                {
                    var adjustedWeight = toneMap.Weight / totalWeights;
                    var ldr = toneMap.GetLDR(i / image.Height, i % image.Height);
                    ldr = Vector3.Clamp(ldr, Vector3.Zero, Vector3.One);
                    accumulatedColor += ldr * adjustedWeight;
                }
                result[i] = new Color(accumulatedColor);
            }

            return result;
        }
    }
}
