namespace SpriteCompiler.Adapters
{
    using Priority_Queue;
    using SpriteCompiler.AI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class QueueAdapter<T, C> : IQueue<T>
        where T : ISearchNode<C>
        where C : IComparable<C>
    {
        private readonly SimplePriorityQueue<T, C> queue = new SimplePriorityQueue<T, C>();

        public bool Empty { get { return queue.Count == 0;  } }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item, item.PathCost);
            }
        }

        public void Clear()
        {
            queue.Clear();
        }

        public void Enqueue(T item)
        {
            queue.Enqueue(item, item.PathCost);
        }

        public T Remove()
        {
            return queue.Dequeue();
        }
    }
}
