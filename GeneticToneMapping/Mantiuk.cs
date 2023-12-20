using OpenCvSharp;

namespace GeneticToneMapping
{
    internal struct Mantiuk : IToneMap
    {
        public  float Weight { get; set; } = 1.0f;
        public  int   ParametersCount      => 3;
                
        public  float Gamma                = 1.0f;
        public  float Scale                = 0.75f;
        public  float Saturation           = 1.0f;

        private Mat   _mantiukMat          = new();

        public Mantiuk()
        {
        }

        public float GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return Gamma;
                case 1:
                    return Scale;
                case 2:
                    return Saturation;

            }

            return 0.0f;
        }

        public void SetParameter(int index, float value)
        {
            switch (index)
            {
                case 0:
                    Gamma = value;
                    break;
                case 1:
                    Scale = value;
                    break;
                case 2:
                    Saturation = value;
                    break;
            }
        }

        public Mat GetLDR(HDRImage hdrImage)
        {
            var r = TonemapMantiuk.Create(Gamma, Scale, Saturation);
            r.Process(hdrImage.Data, _mantiukMat);
            return _mantiukMat;
        }

        public object Clone()
        {
            return new Mantiuk
            {
                Gamma      = Gamma,
                Scale      = Scale,
                Saturation = Saturation,
            };
        }
    }
}
