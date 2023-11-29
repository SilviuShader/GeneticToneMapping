﻿using Microsoft.Xna.Framework;
using SharpEXR;

namespace GeneticToneMapping
{
    internal class HDRImage
    {
        public int        Width  { private set; get; }
        public int        Height { private set; get; }

        public Vector4[]  Data   { private set; get; }

        public HDRImage(string filename)
        {
            var exrFile = EXRFile.FromFile(filename);
            var part = exrFile.Parts[0];
            part.OpenParallel(filename);
            
            var floats = part.GetFloats(ChannelConfiguration.RGB, true, GammaEncoding.Linear, true);

            Width  = part.DataWindow.Width;
            Height = part.DataWindow.Height;

            Data = new Vector4[Width * Height];
            var ix = 0;
            for (var w = 0; w < part.DataWindow.Width; w++)
            for (var h = 0; h < part.DataWindow.Height; h++)
            {
                Data[w * part.DataWindow.Height + h] = new Vector4(floats[ix * 4], floats[ix * 4 + 1],
                    floats[ix * 4 + 2], floats[ix * 4 + 3]);
                ix++;
            }

            part.Close();
        }

        public void Slice(int newWidth, int newHeight)
        {
            var newData = new Vector4[newWidth * newHeight];
            for (var x = 0; x < newWidth; x++)
                for (var y = 0; y < newHeight; y++)
                    newData[x * newHeight + y] = Data[x * Height + y];

            Width = newWidth;
            Height = newHeight;
            Data = newData;
        }

        public Vector3 GetPixel(int x, int y)
        {
            var col = Data[x * Height + y];

            return new Vector3(col.X, col.Y, col.Z);
        }
    }
}
