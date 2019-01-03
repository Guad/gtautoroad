using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using System.Linq;
using System;
using System.Collections.Generic;

namespace GrandTheftAutoroad.OpenCV
{
    public class LaneTracker
    {
        public bool Visualization = true;

        private Window[] _l_windows;
        private Window[] _r_windows;

        private Queue<int>[] _l_positions;
        private Queue<int>[] _r_positions;
        private const int _history_count = 5;

        private const int _n_windows = 6;

        private int _width;
        public PIDController PID;

        public LaneTracker()
        {
            PID = new PIDController();
        }

        public Emgu.CV.Image<Emgu.CV.Structure.Bgra, byte> DetectLanes(Image<Bgra, byte> frame)
        {
            _width = frame.Width;
            var edges = SimpleGradients.GetEdges(frame);

            //return edges.Convert<Bgra, byte>();

            //Mat unwarp;
            //var flatEdges = Perspective.FlattenPerspective(edges, out unwarp);
            var flatEdges = edges;

            int h = frame.Height;

            // Initialize
            if (_l_windows == null)
            {
                int w = frame.Width;
                _l_windows = new Window[_n_windows];
                _r_windows = new Window[_n_windows];

                _r_positions = new Queue<int>[_n_windows];
                _l_positions = new Queue<int>[_n_windows];

                
                int window_height = h / _n_windows;

                for (int i = 0; i < _l_windows.Length; i++)
                {
                    _l_positions[i] = new Queue<int>();
                    _r_positions[i] = new Queue<int>();

                    _l_windows[i] = new Window()
                    {
                        Height = window_height,
                        Y = i * window_height + window_height / 2,
                        X = w / 2 - 100,
                        Width = 70,
                        Tolerance = 270,
                    };

                    _r_windows[i] = new Window()
                    {
                        Height = window_height,
                        Y = i * window_height + window_height / 2,
                        X = w /2 + 100,
                        Width = 70,
                        Tolerance = 270,
                    };
                }
            }

            // Scan windows

            Image<Bgra, byte> lanes = new Image<Bgra, byte>(flatEdges.Size);

            List<Point> puntos = new List<Point>();

            int? prevX = null;
            for (int i = 0; i < _l_windows.Length; i++)
            {
                var w = _l_windows[i];

                if (prevX.HasValue) w.X = prevX.Value;
                w.PixelsIn(flatEdges, true);
                if (!prevX.HasValue)
                    w.X = w.MeanX;

                prevX = w.MeanX;

                lanes.Draw(new CircleF(new PointF(prevX.Value, _l_windows[i].Y), 6f), new Bgra(0, 0, 255, 255), 10);

                _l_positions[i].Enqueue(w.MeanX);
                if (_l_positions[i].Count > _history_count)
                    _l_positions[i].Dequeue();


                puntos.Add(new Point((int)_l_positions[i].Average(), w.Y));
            }

            puntos.Reverse();

            prevX = null;
            for (int i = 0; i < _r_windows.Length; i++)
            {
                var w = _r_windows[i];

                if (prevX.HasValue) w.X = prevX.Value;
                w.PixelsIn(flatEdges, true);
                if (!prevX.HasValue)
                    w.X = w.MeanX;

                prevX = w.MeanX;


                lanes.Draw(new CircleF(new PointF(prevX.Value, _l_windows[i].Y), 6f), new Bgra(0, 0, 255, 255), 10);
                _r_positions[i].Enqueue(w.MeanX);
                if (_r_positions[i].Count > _history_count)
                    _r_positions[i].Dequeue();


                puntos.Add(new Point((int)_r_positions[i].Average(), w.Y));
            }

            lanes.FillConvexPoly(puntos.ToArray(), new Bgra(0, 255, 0, 255));


            if (Visualization)
            {
                // Draw the lane
                //Image<Bgra, byte> unwarpedLanes = new Image<Bgra, byte>(frame.Size);
                Image<Bgra, byte> unwarpedLanes = lanes;
                //CvInvoke.WarpPerspective(lanes, unwarpedLanes, unwarp, frame.Size);
                CvInvoke.AddWeighted(unwarpedLanes, 1, frame, 1, 0, frame);


                // Visualize edges
                frame.ROI = new Rectangle(0, 0, 266, 200);
                var chns = flatEdges.Convert<Bgra, byte>();
                var chns_scaled = new Image<Bgra, byte>(frame.ROI.Size);
                CvInvoke.Resize(chns, chns_scaled, frame.ROI.Size);

                chns_scaled.Copy(frame, new Image<Gray, byte>(chns_scaled.Width, chns_scaled.Height, new Gray(255)));

                frame.ROI = Rectangle.Empty;
            }

            return frame;
            //return flatEdges.Convert<Bgra, byte>();
        }

        public float CalculateCorrection()
        {
            //return 0f;
            int targetX = (_l_windows[0].MeanX + _r_windows[0].MeanX) / 2; // Target point
            int currentX = _width / 2;
            int error = targetX - currentX;

            PID.PushValue(error);
            

            return PID.Control;
        }

    }
}
