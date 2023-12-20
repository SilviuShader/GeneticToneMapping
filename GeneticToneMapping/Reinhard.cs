using OpenCvSharp;

namespace GeneticToneMapping
{
    internal struct Reinhard : IToneMap
    {
        public float       Weight { get; set; }     = 1.0f;
        public int         ParametersCount          => 4;
        public float       Gamma { get; set; }      = 1.0f;
        public float       Intensity { get; set; }  = 0.0f;
        public float       LightAdapt { get; set; } = 1.0f;
        public float       ColorAdapt { get; set; } = 0.0f;

        private static Mat _reinhardMat             = new();

        public Reinhard()
        {
        }

        public float GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return Gamma;
                case 1:
                    return Intensity;
                case 2:
                    return LightAdapt;
                case 3:
                    return ColorAdapt;
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
                    minVal = 0.5f;
                    maxVal = 3.0f;
                    break;
                case 1:
                    minVal = -8.0f;
                    maxVal = 8.0f;
                    break;
                case 2:
                    minVal = 0.0f;
                    maxVal = 1.0f;
                    break;
                case 3:
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
                    Gamma = value;
                    break;
                case 1:
                    Intensity = value;
                    break;
                case 2:
                    LightAdapt = value;
                    break;
                case 3:
                    ColorAdapt = value;
                    break;
            }
        }

        public Mat GetLDR(HDRImage hdrImage)
        {
            var r = TonemapReinhard.Create(Gamma, Intensity, LightAdapt, ColorAdapt);
            r.Process(hdrImage.Data, _reinhardMat);
            return _reinhardMat;
        }

        public object Clone()
        {
            return new Reinhard
            {
                Gamma      = Gamma,
                Intensity  = Intensity,
                LightAdapt = LightAdapt,
                ColorAdapt = ColorAdapt
            };
        }
    }
}
