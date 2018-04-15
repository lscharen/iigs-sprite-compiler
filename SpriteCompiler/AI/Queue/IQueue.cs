namespace SpriteCompiler.AI.Queue
{
    using System;
    using System.Collections.Generic;

    public interface IQueue<T>
    {
        void Clear();
        bool Empty { get; }
        T Remove();

        void Enqueue(T item);
        void AddRange(IEnumerable<T> items);
    }
}
