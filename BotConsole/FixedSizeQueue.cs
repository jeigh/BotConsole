using System.Collections.Generic;

namespace AntPlayground
{
    public class FixedSizeIntQueue
    {
        private readonly Queue<int> _queue = new Queue<int>();
        private readonly int _maxSize;
        
        public int HistoryCount { get; set; } = 0;

        public FixedSizeIntQueue(int maxSize) => _maxSize = maxSize;
        public int[] ToArray() => _queue.ToArray();
        

        public void Enqueue(int item)
        {
            if (_queue.Count == _maxSize) _queue.Dequeue();
            _queue.Enqueue(item);
            HistoryCount += 1;
        }
    }
}