using System;
using Microsoft.Xna.Framework;

namespace GeneticToneMapping
{
    internal interface IToneMap : ICloneable
    {
        int     ParametersCount { get; }
        float   Weight          { get; set; }

        float   GetParameter(int index);
        void    SetParameter(int index, float value);

        void    SetImage(HDRImage hdrImage);
        Vector3 GetLDR(int x, int y);
    }
}
