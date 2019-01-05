using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandTheftAutoroad.OpenCV
{
    public class PIDController
    {
        private LinkedQueueSingle _history;
        public int HistoryLength { get; set; }

        public float KP { get; set; }
        public float KI { get; set; }
        public float KD { get; set; }

        public PIDController(float p, float i, float d)
        {
            HistoryLength = 30;
            _history = new LinkedQueueSingle();

            KP = p;
            KI = i;
            KD = d;
        }

        public float Proportional
        {
            get
            {
                return _history.Last();
            }
        }

        public float Integral
        {
            get
            {
                return _history.Sum();
            }
        }

        public float Derivative
        {
            get
            {
                return (_history.Last() - _history.First()) / _history.Count;
            }
        }

        public void PushValue(float val)
        {
            _history.Enqueue(val);
            if (_history.Count > HistoryLength)
                _history.Dequeue();
        }

        public float Control
        {
            get
            {
                return Proportional * KP + Integral * KI + Derivative * KD;
            }
        }
    }
}
