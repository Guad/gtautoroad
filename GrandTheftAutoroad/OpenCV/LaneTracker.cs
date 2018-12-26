using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Linq;
using System;

namespace GrandTheftAutoroad.OpenCV
{
    public static class LaneTracker
    {
        public static void DetectLanes(Image<Bgr, byte> source, Graphics dest)
        {            
            var mask = Gradients.GetEdges(source, dest);
            Dump(mask, dest);

        }

        private static void EdgeDetection(Image<Bgr, byte> source, Graphics dest)
        {
            int w = source.Width;
            int h = source.Height;

            var hls = new Image<Hls, byte>(w, h);

            CvInvoke.CvtColor(source, hls, ColorConversion.Bgr2Hls);

            var sobel_x = new Image<Bgr, short>(w, h);
            var sobel_y = new Image<Bgr, short>(w, h);

            CvInvoke.Sobel(source, sobel_x, Emgu.CV.CvEnum.DepthType.Cv64F, 1, 0, kSize: 3);
            CvInvoke.Sobel(source, sobel_y, Emgu.CV.CvEnum.DepthType.Cv64F, 0, 1, kSize: 3);

            var sobel_x_scaled = new Image<Bgr, byte>(w, h);
            var sobel_y_scaled = new Image<Bgr, byte>(w, h);

            CvInvoke.ConvertScaleAbs(sobel_x, sobel_x_scaled, 0.0038909912109375d, 0);
            CvInvoke.ConvertScaleAbs(sobel_y, sobel_y_scaled, 0.0038909912109375d, 0);

            var direction = new Image<Bgr, short>(w, h);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    //System.Math.Atan2()
                    //direction[y, x] = new Bgr()
                }
            }

            //dest.DrawImageUnscaled(output.ToBitmap(), 0, 0);



        }



        private static void Dump<TColor, TDepth>(Image<TColor, TDepth> source, Graphics dest)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            dest.DrawImageUnscaled(source.ToBitmap(), 0, 0);
        }
    }
}
