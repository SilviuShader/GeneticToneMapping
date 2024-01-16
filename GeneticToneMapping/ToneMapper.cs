using OpenCvSharp;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                ldr.PatchNaNs(1.0f);
                result.AddData(ldr, adjustedWeight);
            }
            result.Clamp01();

            return result;
        }
    }
}
