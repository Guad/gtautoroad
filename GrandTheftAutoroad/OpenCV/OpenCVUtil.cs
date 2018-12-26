using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandTheftAutoroad.OpenCV
{
    public static class OpenCVUtil
    {
        public static Image<Bgr, byte> BitmapToImage(System.Drawing.Bitmap bmp)
        {
            return new Image<Bgr, byte>(bmp);
        }
    }
}
