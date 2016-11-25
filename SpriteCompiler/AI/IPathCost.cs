namespace SpriteCompiler.AI
{
    using System;

    public interface IPathCost<C> : IComparable<C>
    {
        C Add(C value);
    }
}
