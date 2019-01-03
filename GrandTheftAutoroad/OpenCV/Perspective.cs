using System;
using System.Drawing;
using Emgu.CV;

namespace GrandTheftAutoroad.OpenCV
{
    public static class Perspective
    {
        private static PointF[] _origin = new PointF[]
        {
            new PointF(350, 300), // top left
            new PointF(470, 300), // top right
            new PointF(0, 430), // bottom left
            new PointF(799, 430), // bottom right
        };

        // 800x600
        private static PointF[] _dest = new PointF[]
        {
            new PointF(0, 0), // top left
            new PointF(799, 0), // top right
            new PointF(0, 599), // bottom left
            new PointF(799, 599), // bottom right
        };

        public static Image<TColor, TDepth> FlattenPerspective<TColor, TDepth>(Image<TColor, TDepth> image, out Mat unwarpMatrix)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            var origin = new Emgu.CV.Util.VectorOfPointF(_origin);
            var dest = new Emgu.CV.Util.VectorOfPointF(_dest);

            var transformMatrix = CvInvoke.GetPerspectiveTransform(origin, dest);
            unwarpMatrix = CvInvoke.GetPerspectiveTransform(dest, origin);

            Image<TColor, TDepth> img = new Image<TColor, TDepth>(image.Size);
            CvInvoke.WarpPerspective(image, img, transformMatrix, image.Size);
            return img;
        }
    }
}
