using System;
using Microsoft.Xna.Framework;
using OpenCvSharp;
using SharpDX.MediaFoundation;

namespace GeneticToneMapping
{
    internal struct Uchimura : IToneMap
    {
        public float Weight               { get; set; } = 1.0f;
        public int   ParametersCount                    => 6;
        public float MaxBrightness        { get; set; } = 1.0f;
        public float Contrast             { get; set; } = 0.0f;
        public float LinearStart          { get; set; } = 0.0f;
        public float LinearLength         { get; set; } = 0.01f;
        public float BlackTightnessShape  { get; set; } = 1.0f;
        public float BlackTightnessOffset { get; set; } = 0.0f;

        private static Mat _reinhardMat = new();

        public Uchimura()
        {
        }

        public float GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return MaxBrightness;
                case 1:
                    return Contrast;
                case 2:
                    return LinearStart;
                case 3:
                    return LinearLength;
                case 4:
                    return BlackTightnessShape;
                case 5:
                    return BlackTightnessOffset;
            }

            return 0.0f;
        }

        public void GetParameterRange(int index, out float minVal, out float maxVal)
        {
            minVal = 0.0f;
            maxVal = 1.0f;

            switch (index)
            {
                case 0:
                    minVal = 1.0f;
                    maxVal = 1.0f;
                    break;
                case 1:
                    minVal = 0.01f;
                    maxVal = 1.0f;
                    break;
                case 2:
                    minVal = 0.01f;
                    maxVal = 1.0f;
                    break;
                case 3:
                    minVal = 0.01f;
                    maxVal = 0.99f;
                    break;
                case 4:
                    minVal = 1.0f;
                    maxVal = 3.0f;
                    break;
                case 5:
                    minVal = 0.0f;
                    maxVal = 1.0f;
                    break;
            }
        }

        public void SetParameter(int index, float value)
        {
            switch (index)
            {
                case 0:
                    MaxBrightness        = value;
                    break;               
                case 1:                  
                    Contrast             = value;
                    break;               
                case 2:                  
                    LinearStart          = value;
                    break;               
                case 3:                  
                    LinearLength         = value;
                    break;               
                case 4:                  
                    BlackTightnessShape  = value;
                    break;
                case 5:
                    BlackTightnessOffset = value;
                    break;
            }
        }

        public Mat GetLDR(HDRImage hdrImage)
        {
            var data = new Vector3[hdrImage.Width * hdrImage.Height]; // TODO: Optimize this new
            var newData = new Vec3f[hdrImage.Width, hdrImage.Height];

            OpenCVHelper.CopyMat(ref data, hdrImage.Data);

            var l0 = ((MaxBrightness - LinearStart) * LinearLength) / Contrast;
            var S0 = LinearStart + l0;
            var S1 = LinearStart + Contrast * l0;
            var C2 = (Contrast * MaxBrightness) / (MaxBrightness - S1);
            var CP = -C2 / MaxBrightness;

            var ix = 0;

            foreach (var pixel in data)
            {
                var w0 = Vector3.One - Smoothstep(Vector3.Zero, Vector3.One * LinearStart, pixel);
                var w2 = Step(Vector3.One * (LinearStart + l0), pixel);
                var w1 = Vector3.One - w0 - w2;

                var T = LinearStart * Pow(pixel / LinearStart, BlackTightnessShape) + Vector3.One * BlackTightnessOffset;
                var L = (Vector3.One * LinearStart) + Contrast * (pixel - Vector3.One * LinearStart);
                var S = (Vector3.One * MaxBrightness) - (MaxBrightness - S1) * Exp(CP * (pixel - (Vector3.One * S0)));

                var cout = T * w0 + L * w1 + S * w2;

                cout.X = Math.Clamp(cout.X, 0.0f, 1.0f);
                cout.Y = Math.Clamp(cout.Y, 0.0f, 1.0f);
                cout.Z = Math.Clamp(cout.Z, 0.0f, 1.0f);

                newData[ix / hdrImage.Height, ix % hdrImage.Height] = new Vec3f(cout.X, cout.Y, cout.Z);

                ix++;
            }

            var result = Mat.FromArray(newData);

            return result;
        }

        public object Clone()
        {
            return new Uchimura
            {
                MaxBrightness        = MaxBrightness,
                Contrast             = Contrast,
                LinearStart          = LinearStart,
                LinearLength         = LinearLength,
                BlackTightnessShape  = BlackTightnessShape,
                BlackTightnessOffset = BlackTightnessOffset
            };
        }

        private Vector3 Smoothstep(Vector3 a, Vector3 b, Vector3 val)
        {
            var result = new Vector3
            {
                X = MathHelper.SmoothStep(a.X, b.X, val.X),
                Y = MathHelper.SmoothStep(a.Y, b.Y, val.Y),
                Z = MathHelper.SmoothStep(a.Z, b.Z, val.Z)
            };

            return result;
        }

        private Vector3 Step(Vector3 threshold, Vector3 val)
        {
            var result = Vector3.Zero;

            if (val.X >= threshold.X)
                result.X = 1.0f;

            if (val.Y >= threshold.Y)
                result.Y = 1.0f;

            if (val.Z >= threshold.Z)
                result.Z = 1.0f;

            return result;
        }

        private Vector3 Pow(Vector3 a, float p) => 
            new (MathF.Pow(a.X, p), MathF.Pow(a.Y, p), MathF.Pow(a.Z, p));

        private Vector3 Exp(Vector3 a) => 
            new (MathF.Exp(a.X), MathF.Exp(a.Y), MathF.Exp(a.Z));
    }
}
