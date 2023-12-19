using System.Collections.Generic;

namespace GeneticToneMapping
{
    internal static class ToneMapper
    {
        public static LDRImage ToneMap(HDRImage image, IEnumerable<IToneMap> toneMaps)
        {
            // TODO: Use a static variable when called from genetic algorithm and set values to 0 at each iteration
            var result = new LDRImage(image);

            var totalWeights = 0.0f;
            foreach (var toneMap in toneMaps)
                totalWeights += toneMap.Weight;

            foreach (var toneMap in toneMaps)
            {
                var adjustedWeight = toneMap.Weight / totalWeights;
                var ldr = toneMap.GetLDR(image);
                result.AddData(ldr, adjustedWeight);
            }

            result.Data.PatchNaNs();

            return result;


            //var result = new Color[image.Data.Length];
            //Array.Fill(result, Color.Black);

            //var totalWeights = 0.0f;

            //foreach (var toneMap in toneMaps)
            //{
            //    toneMap.SetImage(image);
            //    totalWeights += toneMap.Weight;
            //}

            //for (var i = 0; i < image.Data.Length; i++)
            //{
            //    var accumulatedColor = Vector3.Zero;
            //    foreach (var toneMap in toneMaps)
            //    {
            //        var adjustedWeight = toneMap.Weight / totalWeights;
            //        var ldr = toneMap.GetLDR(i / image.Height, i % image.Height);
            //        ldr = Vector3.Clamp(ldr, Vector3.Zero, Vector3.One);
            //        accumulatedColor += ldr * adjustedWeight;
            //    }
            //    result[i] = new Color(accumulatedColor);
            //}

            //return result;
        }
    }
}
