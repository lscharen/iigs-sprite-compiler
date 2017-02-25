namespace SpriteCompiler.AI
{
    using Queue;
    using System;

    public class BestFirstSearch<A, S, T, C> : AbstractStateSpaceSearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
    {
        public BestFirstSearch(ISearchStrategy<A, S, T, C> strategy, Func<IQueue<T>> fringe)
            : base(strategy, fringe)
        {
        }
    }
}
