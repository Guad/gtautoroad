using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace GrandTheftAutoroad.OpenCV
{
    public class SimpleGradients
    {
        public static Image<Gray, byte> GetEdges(Image<Bgra, byte> source)
        {
            int w = source.Width;
            int h = source.Height;

            // Convert to HLS and select the saturation channel
            var bgr = new Image<Bgr, byte>(w, h);
            var hls = new Image<Hls, byte>(w, h);

            CvInvoke.CvtColor(source, bgr, ColorConversion.Bgra2Bgr);
            CvInvoke.CvtColor(bgr, hls, ColorConversion.Bgr2Hls);

            var s_channel = hls[2];
            var s_channel_thresh = new Image<Gray, byte>(s_channel.Size);
            var s_channel_thresh_blur = new Image<Gray, byte>(s_channel.Size);

            // Threshold the saturation channel
            CvInvoke.Threshold(s_channel, s_channel_thresh, 70, 255, ThresholdType.Binary);

            // Do some denoising
            CvInvoke.MedianBlur(s_channel_thresh, s_channel_thresh_blur, 3);
            var final = s_channel_thresh_blur.Dilate(3);

            // Black out the vehicle
            const int vehicle_w = 70;
            const int vehicle_h = 90;

            final.ROI = new System.Drawing.Rectangle(w/2 - vehicle_w/2, h/2 - vehicle_h/2, vehicle_w, vehicle_h);
            var black_rect = new Image<Gray, byte>(vehicle_w, vehicle_h);
            var mask = new Image<Gray, byte>(vehicle_w, vehicle_h, new Gray(255));
            black_rect.Copy(final, mask);
            final.ROI = System.Drawing.Rectangle.Empty;

            // Clean up
            hls.Dispose();
            bgr.Dispose();
            s_channel_thresh.Dispose();
            s_channel_thresh_blur.Dispose();
            black_rect.Dispose();
            mask.Dispose();

            return final;
        }
    }
}
