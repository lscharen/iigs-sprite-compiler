namespace SpriteCompiler.AI.Queue
{
    using System.Collections.Generic;

    public class FIFO<T> : IQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();

        public void Clear()
        {
            queue.Clear();
        }

        public bool Empty
        {
            get { return queue.Count == 0; }
        }

        public T Remove()
        {
            return queue.Dequeue();
        }

        public void Enqueue(T item)
        {
            queue.Enqueue(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }
    }
}
