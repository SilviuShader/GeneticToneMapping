using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using OpenCvSharp;

namespace GeneticToneMapping
{
    internal struct TumblinRushmeier : IToneMap
    {
        public float Ldmax = 1.0f;
        public float Cmax = 1.0f;

        public int ParametersCount => 2;
        public float Weight { get; set; } = 1.0f;

        public TumblinRushmeier()
        {
        }

        public float GetParameter(int index)
        {
            Debug.Assert(index is >= 0 and <= 1, "Tumblin Rushmeier has 2 parameters.");
            switch (index)
            {
                case 0:
                    return Ldmax;
                case 1:
                    return Cmax;
            }

            return -1;
        }

        public void GetParameterRange(int index, out float minVal, out float maxVal)
        {
            minVal = 0.0f;
            maxVal = 1.0f;

            switch (index)
            {
                case 0:
                    minVal = 1.0f;
                    maxVal = 300.0f;
                    break;
                case 1:
                    minVal = 1.0f;
                    maxVal = 100.0f;
                    break;
            }
        }

        public void SetParameter(int index, float value)
        {
            Debug.Assert(index is >= 0 and <= 1, "Tumblin Rushmeier has 2 parameters.");
            switch (index)
            {
                case 0:
                    Ldmax = value;
                    break;
                case 1:
                    Cmax = value;
                    break;
            }
        }

        public Mat GetLDR(HDRImage hdrImage)
        {
            var data = new Vector3[hdrImage.Width * hdrImage.Height]; // TODO: Optimize this new
            var newData = new Vec3f[hdrImage.Width, hdrImage.Height];

            OpenCVHelper.CopyMat(ref data, hdrImage.Data);

            var totalLuminance = 0.0f;

            foreach (var pixel in data)
                totalLuminance += ColorHelper.Luminance(pixel);

            var averageLuminance = totalLuminance / data.Length;

            var logLrw = MathF.Log10(averageLuminance) + 0.84f;
            var alphaRw = 0.4f * logLrw + 2.92f;
            var betaRw = -0.4f * logLrw * logLrw - 2.584f * logLrw + 2.0208f;
            var Lwd = Ldmax / MathF.Sqrt(Cmax);
            var logLd = MathF.Log10(Lwd) + 0.84f;
            var alphaD = 0.4f * logLd + 2.92f;
            var betaD = -0.4f * logLd * logLd - 2.584f * logLd + 2.0208f;

            var ix = 0;

            foreach (var pixel in data)
            {
                var lin = ColorHelper.Luminance(pixel);
                var lout = MathF.Pow(lin, alphaRw / alphaD) / Ldmax * MathF.Pow(10.0f, (betaRw - betaD) / alphaD) -
                           (1.0f / Cmax);

                var newPixel = new Vec3f(pixel.X / (lin * lout),
                    pixel.Y / (lin * lout), pixel.Z / (lin * lout));

                newPixel.Item0 = Math.Clamp(newPixel.Item0, 0.0f, 1.0f);
                newPixel.Item1 = Math.Clamp(newPixel.Item1, 0.0f, 1.0f);
                newPixel.Item2 = Math.Clamp(newPixel.Item2, 0.0f, 1.0f);

                newData[ix / hdrImage.Height, ix % hdrImage.Height] = newPixel;

                ix++;
            }

            var result = Mat.FromArray(newData);

            return result;
        }

        public object Clone()
        {
            return new TumblinRushmeier
            {
                Cmax = Cmax,
                Ldmax = Ldmax,
                Weight = Weight
            };
        }
    }
}
