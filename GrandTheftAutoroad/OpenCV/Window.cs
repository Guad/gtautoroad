using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandTheftAutoroad.OpenCV
{
    public class Window
    {
        // Center X
        public int X { get; set; }

        // Center Y
        public int Y { get; set; }

        public int Width { get; set; } = 200;
        public int Height { get; set; }

        public int Tolerance { get; set; } = 50;

        public int MeanX { get; set; }

        public bool PixelsIn(Image<Gray, byte> img, bool debug = false)
        {
            int count = 0;
            int xc = 0;
            int yc = 0;
            var rect = new Rectangle(X - Width / 2, Y - Height / 2, Width, Height);

            
            for (int y = Math.Max(0, rect.Y); y <= Math.Min(img.Height-1, rect.Y + rect.Height); y++)
            {
                for (int x = Math.Max(0, rect.X); x <= Math.Min(img.Width-1, rect.X + rect.Width); x++)
                {
                    if (img.Data[y, x, 0] != 0)
                    {
                        xc += x;
                        yc += y;
                        count++;
                    }
                }
            }

            //int t = Tolerance;
            int t = Main._tolerance;

            if (count > t)
                MeanX = xc / count;
            else
                MeanX = X;

            if (debug)
            {
                img.Draw(count+"", new Point(X - Width / 2 + 5, Y), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1.2f, new Gray(255), 5);
                img.Draw(rect, new Gray(count > Tolerance ? 255 : 127), 5);
            }

            return count > t;
        }
    }
}
