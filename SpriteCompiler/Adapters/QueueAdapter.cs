namespace SpriteCompiler.Adapters
{
    using Priority_Queue;
    using SpriteCompiler.AI;
    using SpriteCompiler.AI.Queue;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class QueueAdapter<T, C> : IQueue<T>
        where T : ISearchNode<C>
        where C : ICost<C>
    {
        private readonly SimplePriorityQueue<T, C> queue = new SimplePriorityQueue<T, C>();

        public bool Empty { get { return queue.Count == 0;  } }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
#if VERBOSE_DEBUG
                Console.WriteLine("Enqueuing " + item + " with cost " + item.EstCost);
#endif
                queue.Enqueue(item, item.EstCost);
            }
        }

        public void Clear()
        {
            queue.Clear();
        }

        public void Enqueue(T item)
        {
            queue.Enqueue(item, item.EstCost);
        }

        public T Remove()
        {
            return queue.Dequeue();
        }
    }
}
