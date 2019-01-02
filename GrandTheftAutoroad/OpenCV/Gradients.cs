using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
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

        private static Image<T, byte> GradientAbsValueMask<T>(Image<T, float> source, Graphics dest, char axis = 'x', byte thresholdMin = 0, byte thresholdMax = 255)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            var sobel = new Image<T, float>(w, h);

            if (axis == 'x')
                CvInvoke.Sobel(source, sobel, DepthType.Cv32F, 1, 0, kSize: 3);
            else
                CvInvoke.Sobel(source, sobel, DepthType.Cv32F, 0, 1, kSize: 3);

            float max = 0;
            sobel = sobel.Convert(s =>
            {
                float v = Math.Abs(s);
                if (v > max) max = v;
                return v;
            });

            var sobel_scaled = sobel.Convert<byte>(f => (byte) ((f / max) * 255));

            //var sobel_scaled = sobel.Convert<T, byte>();
            //var sobel_scaled_u = sobel.Convert<byte>((o) => (byte)Math.Abs(o));

            Image<T, byte> mask = sobel_scaled.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        private static Image<T, byte> GradientMagnitudeMask<T>(Image<T, float> source, Graphics dest, byte thresholdMin = 0, byte thresholdMax = 255)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            var sobel_x = new Image<T, float>(w, h);
            var sobel_y = new Image<T, float>(w, h);

            CvInvoke.Sobel(source, sobel_x, DepthType.Cv32F, 1, 0, kSize: 3);
            CvInvoke.Sobel(source, sobel_y, DepthType.Cv32F, 0, 1, kSize: 3);

            // Calculate the magnitude
            Image<T, float> powers = sobel_x.Pow(2) + sobel_y.Pow(2);

            float max = 0f;
            var magnitude = powers.Convert(i =>
            {
                float v = (float)Math.Sqrt(i);
                if (v > max) max = v;
                return v;
            });
            //var magnitude_scaled = magnitude.Convert<T, byte>();
            
            var magnitude_scaled = magnitude.Convert<byte>(f => (byte)((f / max) * 255));

            Image<T, byte> mask = magnitude_scaled.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        private static Image<T, byte> GradientDirectionMask<T>(Image<T, float> source, Graphics dest, double thresholdMin = 0, double thresholdMax = Math.PI / 2)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            var sobel_x = new Image<T, float>(w, h);
            var sobel_y = new Image<T, float>(w, h);

            CvInvoke.Sobel(source, sobel_x, DepthType.Cv32F, 1, 0, kSize: 3);
            CvInvoke.Sobel(source, sobel_y, DepthType.Cv32F, 0, 1, kSize: 3);

            // Calculate the direction

            var direction = sobel_x.Convert(sobel_y, (sx, sy) => Math.Atan2(Math.Abs(sy), Math.Abs(sx)));

            var mask = direction.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        private static Image<T, byte> ColorThresholdMask<T>(Image<T, float> source, Graphics dest, double thresholdMin = 0, double thresholdMax = Math.PI / 2)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            Image<T, byte> mask = source.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        public static Image<Gray, byte> GetEdges(Image<Bgra, byte> source, Graphics dest)
        {
            int w = source.Width;
            int h = source.Height;

            var bgr = new Image<Bgr, byte>(w, h);
            var hls = new Image<Hls, float>(w, h);

            
            CvInvoke.CvtColor(source, bgr, ColorConversion.Bgra2Bgr);
            CvInvoke.CvtColor(bgr, hls, ColorConversion.Bgr2Hls);

            hls.Mat.ConvertTo(hls, DepthType.Cv32F);

            var s_channel = hls[2];

            byte thresMin = (byte) Main._min;
            byte thresMax = (byte) Main._max;

            var gradient_x = GradientAbsValueMask(s_channel, dest, axis: 'x', thresholdMin: thresMin, thresholdMax: thresMax);
            var gradient_y = GradientAbsValueMask(s_channel, dest, axis: 'y', thresholdMin: thresMin, thresholdMax: thresMax);

            var magnitude = GradientMagnitudeMask(s_channel, dest, thresholdMin: thresMin, thresholdMax: thresMax);

            var direction = GradientDirectionMask(s_channel, dest, thresholdMin: 0.7, thresholdMax: 1.3);

            Image<Gray, byte> gradient_mask = 
                gradient_x.Convert(gradient_y, magnitude, direction, 
                    (gx, gy, m, d) => {
                        bool grads = gx != 0 && gy != 0;
                        bool mags = m != 0 && d != 0;

                        if (grads || mags) return (byte)255;
                        return (byte)0;
                        });

            gradient_mask = gradient_mask.Dilate(1).Erode(1);
            
            return gradient_mask;
        }
    }
}
