using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandTheftAutoroad.OpenCV
{
    public class LinkedQueue<T>
    {
        private LinkedList<T> _list;

        public LinkedQueue()
        {
            _list = new LinkedList<T>();
        }

        public virtual void Enqueue(T value)
        {
            _list.AddLast(value);
        }

        public virtual void Clear()
        {
            _list.Clear();
        }

        public virtual T Dequeue()
        {
            var node = _list.First;
            _list.RemoveFirst();
            return node.Value;
        }

        public T Peek() => _list.First.Value;

        public T First() => Peek();

        public T Last() => _list.Last.Value;

        public int Count => _list.Count;
    }

    public sealed class LinkedQueueInt : LinkedQueue<int>
    {
        private int _sum;

        public override void Enqueue(int value)
        {
            base.Enqueue(value);
            _sum += value;
        }

        public override int Dequeue()
        {
            int val = base.Dequeue();
            _sum -= val;
            return val;
        }

        public override void Clear()
        {
            base.Clear();
            _sum = 0;
        }

        public float Average()
        {
            return _sum / (float)Count;
        }

        public int Sum()
        {
            return _sum;
        }
    }

    public sealed class LinkedQueueSingle : LinkedQueue<float>
    {
        private float _sum;

        public override void Enqueue(float value)
        {
            base.Enqueue(value);
            _sum += value;
        }

        public override float Dequeue()
        {
            float val = base.Dequeue();
            _sum -= val;
            return val;
        }

        public override void Clear()
        {
            base.Clear();
            _sum = 0;
        }

        public float Average()
        {
            return _sum / (float)Count;
        }

        public float Sum()
        {
            return _sum;
        }
    }
}
