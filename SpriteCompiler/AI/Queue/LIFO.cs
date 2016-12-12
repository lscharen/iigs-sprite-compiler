namespace SpriteCompiler.AI.Queue
{
    using System.Collections.Generic;

    public class LIFO<T> : IQueue<T>
    {
        private readonly Stack<T> stack = new Stack<T>();

        public void Clear()
        {
            stack.Clear();
        }

        public bool Empty
        {
            get { return stack.Count == 0; }
        }

        public T Remove()
        {
            return stack.Pop();
        }

        public void Enqueue(T item)
        {
            stack.Push(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                stack.Push(item);
            }
        }
    }
}
