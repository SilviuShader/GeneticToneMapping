using System;
using Microsoft.Xna.Framework;
using OpenCvSharp;

namespace GeneticToneMapping
{
    internal struct Uncharted2 : IToneMap
    {
        public int      ParametersCount => 7;
        public float    Weight { get; set; } = 1.0f;

        private float[] _parameters;

        public Uncharted2()
        {
            _parameters = new float[7];
        }

        public float GetParameter(int index)
        {
            return _parameters[index];
        }

        public void SetParameter(int index, float value)
        {
            _parameters[index] = value;
        }
        
        public Mat GetLDR(HDRImage hdrImage)
        {
            var exposureBias = 2.0f;
            var input = (exposureBias * Uncharted2Tonemap(hdrImage.Data)).ToMat();
            var mask = new Mat();
            var output = new Mat();

            var w = _parameters[6];
            var whiteScale = 1.0f / Uncharted2Tonemap(w);
            input *= whiteScale;
            
            Cv2.InRange(input, new Scalar(0.0f), new Scalar(1.0f), mask);
            input.CopyTo(output, mask);
            return output;
        }

        private float Uncharted2Tonemap(float x)
        {
            var a = _parameters[0];
            var b = _parameters[1];
            var c = _parameters[2];
            var d = _parameters[3];
            var e = _parameters[4];
            var f = _parameters[5];

            return ((x * (a * x * c * b) + d * e) / (x * (a * x + b) + d * f)) - e / f;
        }

        private Mat Uncharted2Tonemap(Mat m)
        {
            var a = _parameters[0];
            var b = _parameters[1];
            var c = _parameters[2];
            var d = _parameters[3];
            var e = _parameters[4];
            var f = _parameters[5];

            return ((m.Mul((a * m + c * b))+  d * e) / (m.Mul(a * m + b) + d * f)) - e / f;
        }

        public object Clone()
        {
            var result = new Uncharted2
            {
                _parameters = new float[_parameters.Length],
                Weight = Weight
            };

            Array.Copy(_parameters, result._parameters, _parameters.Length);

            return result;
        }
    }
}
