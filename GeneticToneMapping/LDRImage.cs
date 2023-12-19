using OpenCvSharp;

namespace GeneticToneMapping
{
    internal class LDRImage
    {
        public int Width  { private set; get; }
        public int Height { private set; get; }

        public Mat Data   { private set; get; }

        public LDRImage(HDRImage hdrImage)
        {
            Width = hdrImage.Width;
            Height = hdrImage.Height;
            Data = Mat.Zeros(hdrImage.Data.Rows, hdrImage.Data.Cols, MatType.CV_32FC3);
        }

        public void AddData(Mat newData, float weight) =>
            Data += newData * weight;
    }
}
