using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandTheftAutoroad.OpenCV
{
    public static class Gradients
    {

        private static Image<T, byte> GradientAbsValueMask<T>(Image<T, byte> source, Graphics dest, char axis = 'x', byte thresholdMin = 0, byte thresholdMax = 255)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            var sobel = new Image<T, short>(w, h);

            if (axis == 'x')
            {
                CvInvoke.Sobel(source, sobel, DepthType.Cv64F, 1, 0, kSize: 3);
            }
            else
            {
                CvInvoke.Sobel(source, sobel, DepthType.Cv64F, 0, 1, kSize: 3);
            }


            Image<T, byte> sobel_scaled = sobel.Convert<T, byte>();

            sobel_scaled = sobel_scaled.Convert<byte>((o) => (byte)Math.Abs(o));

            Image<T, byte> mask = sobel_scaled.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        private static Image<T, byte> GradientMagnitudeMask<T>(Image<T, byte> source, Graphics dest, byte thresholdMin = 0, byte thresholdMax = 255)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            var sobel_x = new Image<T, short>(w, h);
            var sobel_y = new Image<T, short>(w, h);

            CvInvoke.Sobel(source, sobel_x, DepthType.Cv64F, 1, 0, kSize: 3);
            CvInvoke.Sobel(source, sobel_y, DepthType.Cv64F, 0, 1, kSize: 3);

            // Calculate the magnitude
            Image<T, int> powers = sobel_x.Convert<T, byte>().Convert<int>(b => b).Pow(2) + sobel_y.Convert<T, byte>().Convert<int>(b => b).Pow(2);
            var magnitude = powers.Convert<byte>(i => (byte)Math.Sqrt(i));

            Image<T, byte> mask = magnitude.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        private static Image<T, byte> GradientDirectionMask<T>(Image<T, byte> source, Graphics dest, double thresholdMin = 0, double thresholdMax = Math.PI / 2)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            var sobel_x = new Image<T, short>(w, h);
            var sobel_y = new Image<T, short>(w, h);

            CvInvoke.Sobel(source, sobel_x, DepthType.Cv64F, 1, 0, kSize: 3);
            CvInvoke.Sobel(source, sobel_y, DepthType.Cv64F, 0, 1, kSize: 3);

            // Calculate the magnitude

            var direction = sobel_x.Convert(sobel_y, (sx, sy) => Math.Atan2(Math.Abs(sy), Math.Abs(sx)));

            var mask = direction.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        private static Image<T, byte> ColorThresholdMask<T>(Image<T, byte> source, Graphics dest, double thresholdMin = 0, double thresholdMax = Math.PI / 2)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            Image<T, byte> mask = source.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        public static Image<Gray, byte> GetEdges(Image<Bgr, byte> source, Graphics dest)
        {
            int w = source.Width;
            int h = source.Height;

            var hls = new Image<Hls, byte>(w, h);

            CvInvoke.CvtColor(source, hls, ColorConversion.Bgr2Hls);

            var s_channel = hls[2];
            var gradient_x = GradientAbsValueMask(s_channel, dest, axis: 'x', thresholdMin: 20, thresholdMax: 100);
            var gradient_y = GradientAbsValueMask(s_channel, dest, axis: 'y', thresholdMin: 20, thresholdMax: 100);

            var magnitude = GradientMagnitudeMask(s_channel, dest, thresholdMin: 20, thresholdMax: 100);

            var direction = GradientDirectionMask(s_channel, dest, thresholdMin: 0.7, thresholdMax: 1.3);

            Image<Gray, byte> gradient_mask = gradient_x.Convert(gradient_y, magnitude, direction, (gx, gy, m, d) => (byte)((gx != 0 && gy != 0) || (m != 0 && d != 0) ? 255 : 0));
            
            return gradient_mask;
        }
    }
}
