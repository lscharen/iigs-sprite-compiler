namespace SpriteCompiler.AI
{
    using System;

    public interface IHeuristicFunction<S, C> where C : IPathCost<C>
    {
        C Eval(S state);
    }
}
