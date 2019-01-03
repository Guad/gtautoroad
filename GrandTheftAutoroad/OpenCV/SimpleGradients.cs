using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandTheftAutoroad.OpenCV
{
    public class SimpleGradients
    {
        public static Image<Gray, byte> GetEdges(Image<Bgra, byte> source)
        {
            int w = source.Width;
            int h = source.Height;

            var bgr = new Image<Bgr, byte>(w, h);
            var hls = new Image<Hls, byte>(w, h);


            CvInvoke.CvtColor(source, bgr, ColorConversion.Bgra2Bgr);
            CvInvoke.CvtColor(bgr, hls, ColorConversion.Bgr2Hls);

            //hls.Mat.ConvertTo(hls, DepthType.Cv32F);

            var s_channel = hls[2];
            var s_channel_thresh = new Image<Gray, byte>(s_channel.Size);
            var s_channel_thresh_blur = new Image<Gray, byte>(s_channel.Size);
            CvInvoke.Threshold(s_channel, s_channel_thresh, 70, 255, ThresholdType.Binary);
            CvInvoke.MedianBlur(s_channel_thresh, s_channel_thresh_blur, 3);
            
            return s_channel_thresh_blur;
        }
    }
}
