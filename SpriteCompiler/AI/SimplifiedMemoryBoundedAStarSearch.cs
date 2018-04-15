namespace SpriteCompiler.AI
{
    using Queue;
    using System;

    public class SimplifiedMemoryBoundedAStarSearch<A, S, T, C> : BestFirstSearch<A, S, T, C>
        where T : ISMASearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        public SimplifiedMemoryBoundedAStarSearch(ISearchStrategy<A, S, T, C> search, Func<IQueue<T>> fringe)
            : base(search, fringe)
        {
        }
    }

    public interface ISMASearchNode<A, S, T, C> : IHeuristicSearchNode<A, S, T, C> where C : ICost<C>
    {
        C BestForgottenSuccessor { get; }
    }
}
