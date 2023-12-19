using System.Diagnostics;
using OpenCvSharp;

namespace GeneticToneMapping
{
    internal struct Reinhard : IToneMap
    {
        public float       White { get; set; }  = 1.0f;
                           
        public int         ParametersCount      => 1;
        public float       Weight { get; set; } = 1.0f;

        private static Mat _reinhardMat         = new();

        public Reinhard()
        {
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

        public Mat GetLDR(HDRImage hdrImage)
        {
            var r = TonemapReinhard.Create(White);
            r.Process(hdrImage.Data, _reinhardMat);
            return _reinhardMat;
        }

        public object Clone()
        {
            return new Reinhard
            {
                Weight = Weight,
                White = White
            };
        }
    }
}
