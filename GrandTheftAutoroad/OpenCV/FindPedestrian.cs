using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandTheftAutoroad.OpenCV
{
    public static class FindPedestrian
    {
        public static Bitmap FindPedestrians(Bitmap bmp)
        {
            var img = OpenCVUtil.BitmapToImage(bmp);
            var output = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);

            using (HOGDescriptor des = new HOGDescriptor())
            using (var g = Graphics.FromImage(output))
            {
                des.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());

                var regions = des.DetectMultiScale(img);
                foreach (var region in regions)
                {
                    g.DrawRectangle(Pens.Red, region.Rect);
                }
            }

            return output;
        }
    }
}
