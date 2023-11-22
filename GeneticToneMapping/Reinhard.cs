using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace GeneticToneMapping
{
    internal struct Reinhard : IToneMap
    {
        public float     White { get; set; }  = 1.0f;
                         
        public int       ParametersCount      => 1;
        public float     Weight { get; set; } = 1.0f;

        private HDRImage _workingImage;

        public Reinhard()
        {
            _workingImage = null;
        }

        public float GetParameter(int index)
        {
            Debug.Assert(index == 0, "Reinahrd has only one parameter");
            return White;
        }

        public void SetParameter(int index, float value)
        {
            Debug.Assert(index == 0, "Reinahrd has only one parameter");
            White = value;
        }

        public void SetImage(HDRImage hdrImage)
        {
            _workingImage = hdrImage;
        }

        public Vector3 GetLDR(int x, int y)
        {
            var col = _workingImage.GetPixel(x, y);

            var lin = ColorHelper.Luminance(col);
            var lout = (lin * (1.0f + lin / (White * White))) / (1.0f + lin);

            return col / lin * lout;
        }

        public object Clone()
        {
            return new Reinhard
            {
                Weight = Weight,
                White = White,
                _workingImage = null
            };
        }
    }
}
