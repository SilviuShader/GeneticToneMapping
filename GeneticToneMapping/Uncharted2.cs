using System;
using Microsoft.Xna.Framework;

namespace GeneticToneMapping
{
    internal struct Uncharted2 : IToneMap
    {
        public  int      ParametersCount      => 7;
        public  float    Weight { get; set; } = 1.0f;
                         
        private float[]  _parameters;

        private HDRImage _workingImage;

        public Uncharted2()
        {
            _parameters   = new float[7];
            _workingImage = null;
        }

        public float GetParameter(int index)
        {
            return _parameters[index];
        }

        public void SetParameter(int index, float value)
        {
            _parameters[index] = value;
        }

        public void SetImage(HDRImage hdrImage)
        {
            _workingImage = hdrImage;
        }

        public Vector3 GetLDR(int x, int y)
        {
            var col = _workingImage.GetPixel(x, y);

            var exposureBias = 2.0f;
            var curr = exposureBias * Uncharted2Tonemap(col);

            var w = _parameters[6];

            var whiteScale = Vector3.One / Uncharted2Tonemap(new Vector3(w, w, w));

            var cout = curr * whiteScale;

            return cout;
        }

        private Vector3 Uncharted2Tonemap(Vector3 x)
        {
            var a = _parameters[0];
            var b = _parameters[1];
            var c = _parameters[2];
            var d = _parameters[3];
            var e = _parameters[4];
            var f = _parameters[5];

            return ((x * (a * x + Vector3.One * c * b) + Vector3.One * d * e) / (x * (a * x + Vector3.One * b) + Vector3.One * d * f)) - Vector3.One * e / f;
        }

        public object Clone()
        {
            var result = new Uncharted2
            {
                _parameters = new float[_parameters.Length],
                _workingImage = null,
                Weight = Weight
            };
            
            Array.Copy(_parameters, result._parameters, _parameters.Length);

            return result;
        }
    }
}
