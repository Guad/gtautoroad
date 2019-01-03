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
        public static Emgu.CV.Image<Emgu.CV.Structure.Bgra, byte> DetectLanes(Image<Bgra, byte> source)
        {
            Mat unwarp;
            var newimg = Perspective.FlattenPerspective(source, out unwarp);

            source.ROI = new Rectangle(0, 0, 266, 200);
            var persp_scaled = new Image<Bgra, byte>(source.ROI.Size);
            CvInvoke.Resize(newimg, persp_scaled, source.ROI.Size);

            persp_scaled.Copy(source, new Image<Gray, byte>(persp_scaled.Width, persp_scaled.Height, new Gray(255)));


            var mask = Gradients.GetEdges(newimg);

            source.ROI = new Rectangle(266, 0, 266, 200);
            var chns = mask.Convert<Bgra, byte>();
            var chns_scaled = new Image<Bgra, byte>(source.ROI.Size);
            CvInvoke.Resize(chns, chns_scaled, source.ROI.Size);

            chns_scaled.Copy(source, new Image<Gray, byte>(chns_scaled.Width, chns_scaled.Height, new Gray(255)));

            source.ROI = Rectangle.Empty;

            return source;
        }



        public static void DrawString<TColor, TDepth>(this Image<TColor, TDepth> source, string text, int x, int y, TColor color)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            source.Draw(text, new Point(x, y), FontFace.HersheyComplex, 0.5, color);
        }
    }
}
