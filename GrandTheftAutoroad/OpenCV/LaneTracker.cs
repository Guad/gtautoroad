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
        public bool Visualization { get; set; }
        public PIDController PID { get; }

        private Window[] _windows_l;
        private Window[] _windows_r;

        private LinkedQueueInt[] _positions_l;
        private LinkedQueueInt[] _positions_r;

        private int[] _corrections_l;
        private int[] _corrections_r;

        private int[] _original_start;

        private int _screenWidth;
        private int _screenHeight;

        private const int HISTORY_SIZE = 20;
        private const int WINDOW_COUNT = 6;
        private const int WINDOW_TOLERANCE = 400;

        private const int DRASTIC_ERROR_THRESHOLD = 50;
        private const int MAX_ERRORS = 180;
        private const int MIN_DISTANCE_BETWEEN_NODES = 160;
        private const int CENTER_DEVIATION = 70;

        public LaneTracker()
        {
            PID = new PIDController(0.005f, 0f, 0f);
            Visualization = true;
            _original_start = new int[2];
        }

        public Image<Bgra, byte> DetectLanes(Image<Bgra, byte> frame)
        {
            _screenWidth = frame.Width;
            _screenHeight = frame.Height;

            var edges = SimpleGradients.GetEdges(frame);

            _original_start[0] = _screenWidth / 2 - 40;
            _original_start[1] = _screenWidth / 2 + 40;

            // Initialize
            if (_windows_l == null)
            {
                Initialize();
            }

            // Scan windows
            List<Point> puntos = new List<Point>();

            // Scan left windows
            puntos.AddRange(ScanWindows(edges, _windows_l, _positions_l, _corrections_l, -1));

            // Reverse puntos so the polygon isn't twisted
            puntos.Reverse();

            // Scan right windows
            puntos.AddRange(ScanWindows(edges, _windows_r, _positions_r, _corrections_r, 1));

            if (Visualization)
            {
                Image<Bgra, byte> lanes = new Image<Bgra, byte>(edges.Size);

                foreach (var p in puntos) lanes.Draw(new CircleF(p, 6f), new Bgra(0, 0, 255, 255), 10);

                // Draw the lane
                lanes.FillConvexPoly(puntos.ToArray(), new Bgra(0, 255, 0, 255));

                CvInvoke.AddWeighted(lanes, 1, frame, 1, 0, frame);

                // Visualize edges in the top left part of the screen
                frame.ROI = new Rectangle(0, 0, 266, 200);
                var chns = edges.Convert<Bgra, byte>();
                var chns_scaled = new Image<Bgra, byte>(frame.ROI.Size);
                CvInvoke.Resize(chns, chns_scaled, frame.ROI.Size);

                chns_scaled.Copy(frame, new Image<Gray, byte>(chns_scaled.Width, chns_scaled.Height, new Gray(255)));

                frame.ROI = Rectangle.Empty;

                // Clean up
                lanes.Dispose();
                chns.Dispose();
                chns_scaled.Dispose();
            }

            return frame;
        }

        public float CalculateCorrection()
        {
            float targetX = ((_positions_l[0].Average() + _positions_r[0].Average()) / 2f); // Target point
            float currentX = _screenWidth / 2;
            float error = targetX - currentX;

            PID.PushValue(error);

            return PID.Control;
        }

        private void Initialize()
        {
            _windows_l = new Window[WINDOW_COUNT];
            _windows_r = new Window[WINDOW_COUNT];

            _positions_r = new LinkedQueueInt[WINDOW_COUNT];
            _positions_l = new LinkedQueueInt[WINDOW_COUNT];

            _corrections_l = new int[WINDOW_COUNT];
            _corrections_r = new int[WINDOW_COUNT];


            int window_height = _screenHeight / WINDOW_COUNT;

            for (int i = 0; i < _windows_l.Length; i++)
            {
                _positions_l[i] = new LinkedQueueInt();
                _positions_r[i] = new LinkedQueueInt();

                _windows_l[i] = new Window()
                {
                    Height = window_height,
                    Y = i * window_height + window_height / 2,
                    X = _original_start[0],
                    Width = 70,
                    Tolerance = WINDOW_TOLERANCE,
                };

                _windows_r[i] = new Window()
                {
                    Height = window_height,
                    Y = i * window_height + window_height / 2,
                    X = _original_start[1],
                    Width = 70,
                    Tolerance = WINDOW_TOLERANCE,
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="windows"></param>
        /// <param name="positions"></param>
        /// <param name="direction">If this is the left windows, pass -1, otherwise 1</param>
        private List<Point> ScanWindows(Image<Gray, byte> edges, Window[] windows, LinkedQueueInt[] positions, int[] corrections, int direction)
        {
            List<Point> puntos = new List<Point>();

            for (int i = 0; i < windows.Length; i++)
            {
                // Get starting position from history, or from default if too close to the other window.
                int originalStart = _original_start[(direction + 1) / 2];
                int startX = originalStart;

                if (_positions_l[i].Count > 0 && _positions_r[i].Count > 0)
                {
                    var l = _positions_l[i].Last();
                    var r = _positions_r[i].Last();

                    // Too close to the other window, reset.
                    if (Math.Abs(l - r) < MIN_DISTANCE_BETWEEN_NODES)
                    {
                        startX = originalStart;
                    }
                    else
                    {
                        int c = (l + r) / 2;

                        startX = c + CENTER_DEVIATION * direction;
                    }
                }

                var w = windows[i];

                int oldx = w.X;
                w.X = startX;

                // Find the lane
                int step = w.Width / 4 * direction;
                while (!w.CountPixels(edges) && w.X > 0 && w.X <= _screenWidth)
                {
                    w.X += step; 
                }

                w.X = w.MeanX;

                // Check if the distance between last position and current is too big
                if (positions[i].Count > 0 && startX != originalStart && Math.Abs(positions[i].Peek() - w.X) > DRASTIC_ERROR_THRESHOLD)
                {
                    // This is an error.

                    // Has this been a new position for a while?
                    if (corrections[i] > MAX_ERRORS)
                    {
                        // This is a real new position
                        corrections[i] = 0;
                        positions[i].Clear();
                    }
                    else
                    {
                        corrections[i]++;
                        w.X = oldx;
                    }
                }
                else
                    corrections[i] = 0; // Reset the counter

                w.CountPixels(edges, Visualization);

                // Push the new position to the history
                positions[i].Enqueue(w.MeanX);
                if (positions[i].Count > HISTORY_SIZE)
                    positions[i].Dequeue();

                // Draw it
                var p = new Point((int)positions[i].Average(), w.Y);
                puntos.Add(p);
            }

            return puntos;
        }
    }
}
