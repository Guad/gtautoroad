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

        private static Image<T, byte> GradientAbsValueMask<T>(Image<T, float> sobel, byte thresholdMin = 0, byte thresholdMax = 255)
            where T : struct, Emgu.CV.IColor
        {
            float max = 0;
            sobel = sobel.Convert(s =>
            {
                float v = Math.Abs(s);
                if (v > max) max = v;
                return v;
            });

            var sobel_scaled = sobel.Convert<byte>(f => (byte) ((f / max) * 255));

            Image<T, byte> mask = sobel_scaled.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        private static Image<T, byte> GradientMagnitudeMask<T>(Image<T, float> sobel_x, Image<T, float> sobel_y, byte thresholdMin = 0, byte thresholdMax = 255)
            where T : struct, Emgu.CV.IColor
        {
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

        private static Image<T, byte> GradientDirectionMask<T>(Image<T, float> sobel_x, Image<T, float> sobel_y, double thresholdMin = 0, double thresholdMax = Math.PI / 2)
            where T : struct, Emgu.CV.IColor
        {
            // Calculate the direction

            var direction = sobel_x.Convert(sobel_y, (sx, sy) => Math.Atan2(Math.Abs(sy), Math.Abs(sx)));

            var mask = direction.Convert<byte>((o) => (byte)(o >= thresholdMin && o <= thresholdMax ? 255 : 0));

            return mask;
        }

        private static Image<T, byte> ColorThresholdMask<T>(Image<T, float> source, byte min = 0, byte max = 255)
            where T : struct, Emgu.CV.IColor
        {
            int w = source.Width;
            int h = source.Height;

            Image<T, byte> mask = source.Convert<byte>((o) => (byte)(o >= min && o <= max ? 255 : 0));

            return mask;
        }

        public static Image<Gray, byte> GetEdges(Image<Bgra, byte> source)
        {
            int w = source.Width;
            int h = source.Height;

            var bgr = new Image<Bgr, byte>(w, h);
            var hls = new Image<Hls, float>(w, h);

            
            CvInvoke.CvtColor(source, bgr, ColorConversion.Bgra2Bgr);
            CvInvoke.CvtColor(bgr, hls, ColorConversion.Bgr2Hls);

            hls.Mat.ConvertTo(hls, DepthType.Cv32F);

            var s_channel = hls[2];

            byte thresMin = 20;
            byte thresMax = 100;

            var sobel_x = new Image<Gray, float>(w, h);
            var sobel_y = new Image<Gray, float>(w, h);

            CvInvoke.Sobel(s_channel, sobel_x, DepthType.Cv32F, 1, 0, kSize: 3);
            CvInvoke.Sobel(s_channel, sobel_y, DepthType.Cv32F, 0, 1, kSize: 3);


            var gradient_x = GradientAbsValueMask(sobel_x, thresholdMin: thresMin, thresholdMax: thresMax);
            var gradient_y = GradientAbsValueMask(sobel_y, thresholdMin: thresMin, thresholdMax: thresMax);

            var magnitude = GradientMagnitudeMask(sobel_x, sobel_y, thresholdMin: thresMin, thresholdMax: thresMax);

            var direction = GradientDirectionMask(sobel_x, sobel_y, thresholdMin: 0.7, thresholdMax: 1.3);

            var color_mask = ColorThresholdMask(s_channel, min: 170, max: 255);

            Image<Gray, byte> gradient_mask = 
                gradient_x.Convert(gradient_y, magnitude, direction, 
                    (gx, gy, m, d) => {
                        bool grads = gx != 0 && gy != 0;
                        bool mags = m != 0 && d != 0;

                        if (grads || mags) return (byte)255;
                        return (byte)0;
                        });

                        
            var mask = gradient_mask.Convert(color_mask, (gm, cm) => (byte)(gm != 0 || cm != 0 ? 255 : 0));

            //mask = mask.Dilate(1).Erode(1);

            return mask;
        }
    }
}
