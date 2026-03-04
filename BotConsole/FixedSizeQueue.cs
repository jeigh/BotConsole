using System.Collections.Generic;

namespace AntPlayground
{
    public class FixedSizeIntQueue
    {
        private readonly Queue<int> _queue = new Queue<int>();
        private readonly int _maxSize;

        public int HistoryCount { get; set; } = 0;
        public bool IsFull => _queue.Count == _maxSize;

        public FixedSizeIntQueue(int maxSize) => _maxSize = maxSize;
        public int[] ToArray() => _queue.ToArray();
        public int Peek() => _queue.Peek();

        // Returns the evicted value when the queue is at capacity, otherwise null.
        public int? Enqueue(int item)
        {
            int? evicted = null;
            if (_queue.Count == _maxSize)
                evicted = _queue.Dequeue();
            _queue.Enqueue(item);
            HistoryCount += 1;
            return evicted;
        }
    }
}