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
        public static Emgu.CV.Image<Emgu.CV.Structure.Bgra, byte> DetectLanes(Image<Bgra, byte> source, Graphics dest)
        {
            var mask = Gradients.GetEdges(source, dest);
            return mask.Convert<Bgra, byte>();

            /*
            int w = source.Width;
            int h = source.Height;

            var bgr = new Image<Rgb, byte>(w, h);
            var hls = new Image<Hls, float>(w, h);


            source.DrawString("Dep2a: " + hls.Mat.Depth, 300, 160, new Bgra(255, 255, 255, 255));

            CvInvoke.CvtColor(source, bgr, ColorConversion.Bgra2Rgb);
            CvInvoke.CvtColor(bgr, hls, ColorConversion.Rgb2Hls, 3);

            hls.Mat.ConvertTo(hls, DepthType.Cv32F);

            var sat = hls[2];


            source.DrawString("Channels: " + bgr.NumberOfChannels, 100, 100, new Bgra(255, 255, 255, 255));
            source.DrawString("Channel2: " + hls.NumberOfChannels, 100, 120, new Bgra(255, 255, 255, 255));
            source.DrawString("Channel3: " + sat.NumberOfChannels, 100, 140, new Bgra(255, 255, 255, 255));

            source.DrawString("Dep2: " + hls.Mat.Depth, 100, 160, new Bgra(255, 255, 255, 255));
            source.DrawString("Dep3: " + sat.Mat.Depth, 100, 180, new Bgra(255, 255, 255, 255));

            return sat.Convert<Bgra, byte>();
            */
        }



        public static void DrawString<TColor, TDepth>(this Image<TColor, TDepth> source, string text, int x, int y, TColor color)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            source.Draw(text, new Point(x, y), FontFace.HersheyComplex, 0.5, color);
        }
    }
}
