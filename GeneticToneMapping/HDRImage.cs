using Microsoft.Xna.Framework;
using SharpEXR;

namespace GeneticToneMapping
{
    internal class HDRImage
    {
        public int        Width  { get; }
        public int        Height { get; }

        public Vector4[]  Data   { get; }

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

        public Vector3 GetPixel(int x, int y)
        {
            var col = Data[x * Height + y];

            return new Vector3(col.X, col.Y, col.Z);
        }
    }
}
