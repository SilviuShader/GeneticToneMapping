using System;
using OpenCvSharp;

namespace GeneticToneMapping
{
    internal interface IToneMap : ICloneable
    {
        int     ParametersCount { get; }
        float   Weight          { get; set; }

        float   GetParameter(int index);
        void    SetParameter(int index, float value);

        Mat     GetLDR(HDRImage hdrImage);
    }
}
