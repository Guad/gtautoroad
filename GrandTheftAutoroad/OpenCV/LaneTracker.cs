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

        private int[] _corrections_l;
        private int[] _corrections_r;

        private const int _history_count = 20;

        private const int _n_windows = 6;

        private int _width;
        public PIDController PID;

        public LaneTracker()
        {
            PID = new PIDController(0.005f, 0f, 0f);
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

            int original_start_left = _width / 2 - 40;
            int original_start_right = _width / 2 + 40;
            int startX_left = original_start_left;
            int startX_right = original_start_right;
            int tolerance = 370;

            // Initialize
            if (_l_windows == null)
            {
                int w = frame.Width;
                _l_windows = new Window[_n_windows];
                _r_windows = new Window[_n_windows];

                _r_positions = new Queue<int>[_n_windows];
                _l_positions = new Queue<int>[_n_windows];

                _corrections_l = new int[_n_windows];
                _corrections_r = new int[_n_windows];

                
                int window_height = h / _n_windows;

                for (int i = 0; i < _l_windows.Length; i++)
                {
                    _l_positions[i] = new Queue<int>();
                    _r_positions[i] = new Queue<int>();

                    _l_windows[i] = new Window()
                    {
                        Height = window_height,
                        Y = i * window_height + window_height / 2,
                        X = startX_left,
                        Width = 70,
                        Tolerance = tolerance,
                    };

                    _r_windows[i] = new Window()
                    {
                        Height = window_height,
                        Y = i * window_height + window_height / 2,
                        X = startX_right,
                        Width = 70,
                        Tolerance = tolerance,
                    };
                }
            }

            // Scan windows

            Image<Bgra, byte> lanes = new Image<Bgra, byte>(flatEdges.Size);

            List<Point> puntos = new List<Point>();

            const int drastic_error = 50;
            const int max_errors = 180;
            const int too_close = 160;
            int dev = 70;

            for (int i = 0; i < _l_windows.Length; i++)
            {
                if (_l_positions[i].Count > 0)
                {
                    var l = _l_positions[i].Last();
                    var r = _r_positions[i].Last();

                    if (Math.Abs(l - r) < too_close)
                    {
                        startX_left = original_start_left;
                        startX_right = original_start_right;
                    }
                    else
                    {
                        int c = (l + r) / 2;

                        startX_left = c - dev;
                        startX_right = c + dev;
                    }
                }


                var w = _l_windows[i];


                int oldx = w.X;
                w.X = startX_left;

                while (!w.PixelsIn(flatEdges) && w.X > 0)
                {
                    w.X -= w.Width / 4;
                }

                w.X = w.MeanX;

                if (_l_positions[i].Count > 0 && Math.Abs(_l_positions[i].Peek() - w.X) > drastic_error)
                {
                    // This is an error.
                    if (_corrections_l[i] > max_errors)
                    {
                        _corrections_l[i] = 0;
                        _l_positions[i].Clear();
                        // Not a correction, real new position
                    }
                    else
                    {
                        _corrections_l[i]++;
                        w.X = oldx;
                    }
                }
                else
                    _corrections_l[i] = 0;


                w.PixelsIn(flatEdges, true);


                _l_positions[i].Enqueue(w.MeanX);
                if (_l_positions[i].Count > _history_count)
                    _l_positions[i].Dequeue();

                var p = new Point((int)_l_positions[i].Average(), w.Y);
                lanes.Draw(new CircleF(p, 6f), new Bgra(0, 0, 255, 255), 10);
                puntos.Add(p);
            }

            puntos.Reverse();

            for (int i = 0; i < _r_windows.Length; i++)
            {
                if (_r_positions[i].Count > 0)
                {
                    var l = _l_positions[i].Last();
                    var r = _r_positions[i].Last();

                    if (Math.Abs(l - r) < too_close)
                    {
                        startX_left = original_start_left;
                        startX_right = original_start_right;
                    }
                    else
                    {
                        int c = (l + r) / 2;

                        startX_left = c - dev;
                        startX_right = c + dev;
                    }
                }

                var w = _r_windows[i];

                int oldx = w.X;
                w.X = startX_right;

                while (!w.PixelsIn(flatEdges) && w.X < _width)
                {
                    w.X += w.Width / 4;
                }

                w.X = w.MeanX;

                if (_r_positions[i].Count > 0 && Math.Abs(_r_positions[i].Peek() - w.X) > drastic_error)
                {
                    // This is an error.
                    if (_corrections_r[i] > max_errors)
                    {
                        _corrections_r[i] = 0;
                        _r_positions[i].Clear();
                        // Not a correction, real new position
                    }
                    else
                    {
                        _corrections_r[i]++;
                        w.X = oldx;
                    }
                }
                else
                    _corrections_r[i] = 0;

                w.PixelsIn(flatEdges, true);


                _r_positions[i].Enqueue(w.MeanX);
                if (_r_positions[i].Count > _history_count)
                    _r_positions[i].Dequeue();

                var p = new Point((int)_r_positions[i].Average(), w.Y);


                lanes.Draw(new CircleF(p, 6f), new Bgra(0, 0, 255, 255), 10);
                puntos.Add(p);
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
            float targetX = (float)((_l_positions[0].Average() + _r_positions[0].Average()) / 2f); // Target point
            float currentX = _width / 2;
            float error = targetX - currentX;

            PID.PushValue(error);
            

            return PID.Control;
        }

    }
}
