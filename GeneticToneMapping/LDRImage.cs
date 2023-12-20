using OpenCvSharp;
using SharpDX;

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

        public void Clamp01()
        {
            Mat mask = new Mat();
            Mat newDataMat = new Mat();
            Cv2.InRange(Data, new Scalar(0.0f, 0.0f, 0.0f), new Scalar(1.0f, 1.0f, 1.0f), mask);
            Data.CopyTo(newDataMat, mask);

            Data = newDataMat;
        }
    }
}
