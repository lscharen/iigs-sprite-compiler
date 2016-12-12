namespace SpriteCompiler.AI
{
    using System;

    public interface IHeuristicFunction<S, C> where C : ICost<C>
    {
        C Eval(S state);
    }
}
