namespace SpriteCompiler.AI
{
    using System;

    public interface IQueue<T>
    {
        void Clear();
        bool Empty { get; }
        T Remove();
        void Enqueue(T item);
    }
}
