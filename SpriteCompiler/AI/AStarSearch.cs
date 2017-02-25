namespace SpriteCompiler.AI
{
    using Queue;
    using System;

    public class AStarSearch<A, S, T, C> : BestFirstSearch<A, S, T, C>
        where T : IHeuristicSearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        public AStarSearch(ISearchStrategy<A, S, T, C> search, Func<IQueue<T>> fringe)
            : base(search, fringe)
        {
        }
    }
}
