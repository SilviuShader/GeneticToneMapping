using OpenCvSharp;

namespace GeneticToneMapping
{
    internal struct Drago : IToneMap
    {
        public  float Weight { get; set; } = 1.0f;
        public  int   ParametersCount      => 3;
        public  float Gamma                = 1.0f;
        public  float Saturation           = 1.0f;
        public  float Bias                 = 0.85f;
                                           
        private Mat   _dragoMat            = new();

        public Drago()
        {
        }

        public float GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return Gamma;
                case 1:
                    return Saturation;
                case 2:
                    return Bias;

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
                    minVal = 0.0f;
                    maxVal = 2.0f;
                    break;
                case 2:
                    minVal = 0.7f;
                    maxVal = 0.9f;
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
                    Saturation = value;
                    break;
                case 2:
                    Bias = value;
                    break;
            }
        }

        public Mat GetLDR(HDRImage hdrImage)
        {
            var r = TonemapDrago.Create(Gamma, Saturation, Bias);
            r.Process(hdrImage.Data, _dragoMat);
            return _dragoMat;
        }

        public object Clone()
        {
            return new Drago
            {
                Gamma = Gamma,
                Saturation = Saturation,
                Bias = Bias
            };
        }
    }
}
